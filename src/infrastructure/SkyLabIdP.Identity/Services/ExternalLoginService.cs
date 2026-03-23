using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos;
using SkyLabIdP.Application.Dtos.LoginUserInfo;
using SkyLabIdP.Application.Dtos.User.Authentication;
using SkyLabIdP.Application.SystemApps.Services.ServiceSettings;
using SkyLabIdP.Domain.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Identity.Services
{
    public class ExternalLoginService(ExternalLoginServiceSettings settings) : IExternalLoginHandler
    {
        private readonly UserManager<ApplicationUser> _userManager = settings.UserManager;
        private readonly IApplicationDbContext _context = settings.Context;
        private readonly IJwtService _jwtService = settings.JwtService;
        private readonly ITenantUserServiceFactory _tenantUserServiceFactory = settings.TenantUserServiceFactory;
        private readonly IDataProtectionService _dataprotectionservice = settings.DataProtectionService;
        private readonly ILoginNotificationService _loginNotificationService = settings.LoginNotificationService;
        private readonly ITokenStorageService _tokenStorageService = settings.TokenStorageService;
        private readonly ILogger _logger = settings.Logger;

        public async Task<AuthenticateResponse> HandleExternalLoginAsync(
            string externalUserId,
            string provider,
            IEnumerable<Claim> claims,
            string email,
            string name,
            string tenantId)
        {
            try
            {
                var userService = _tenantUserServiceFactory.GetServiceByTenantId(tenantId);
                await _context.BeginTransactionAsync(); // 開始交易

                var (user, errorResponse) = await FindOrCreateExternalUserAsync(
                    externalUserId, provider, email, name, tenantId, userService);

                if (errorResponse != null)
                {
                    return errorResponse;
                }

                // 檢查用戶是否需要補充資料
                var needsCompletion = !user!.HasCompletedRegistration;

                // 生成令牌
                var accessToken = await _jwtService.GenerateAccessTokenAsync(user, tenantId);
                var refreshToken = await _jwtService.GenerateRefreshTokenAsync(user, tenantId);

                // 將 refresh token 存入 Redis（與一般登入流程一致）
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var parsedRefreshToken = tokenHandler.ReadJwtToken(refreshToken);
                    await _tokenStorageService.StoreRefreshTokenAsync(user.Id, tenantId, refreshToken, parsedRefreshToken.ValidTo);
                    _logger.LogInformation("外部登入 refresh token 已存入 Redis，用戶ID: {UserId}", user.Id);
                }

                // 獲取用戶資訊
                var userInfo = await userService.GetLoginUserInfoAsync(user.Id, CancellationToken.None);

                var response = new AuthenticateResponse(
                    userInfo,
                    _dataprotectionservice.Protect(user.Id),
                    accessToken,
                    refreshToken,
                    mustResetPassword: false,
                    mustChangePassword: false)
                {
                    OperationResult = new OperationResult(true, needsCompletion ? "需要補充資料" : "登入成功", StatusCodes.Status200OK),
                    IsExternalLogin = true,
                    ExternalProvider = provider,
                    NeedsProfileCompletion = needsCompletion
                };

                FireLoginNotificationSafe(tenantId, email, user.Email, isSuccess: true, provider);

                await _context.CommitTransactionAsync(); // 提交交易
                return response;
            }
            catch (Exception ex)
            {
                await _context.RollbackTransactionAsync(); // 回滾交易
                _logger.LogError(ex, "處理外部登入時出錯: {Provider}:{ExternalId}", provider, externalUserId);
                return new AuthenticateResponse
                {
                    OperationResult = new OperationResult(false, "處理外部登入時出錯", StatusCodes.Status500InternalServerError)
                };
            }
        }

        private async Task<(ApplicationUser? user, AuthenticateResponse? errorResponse)> FindOrCreateExternalUserAsync(
            string externalUserId, string provider, string email, string name, string tenantId, IUserService userService)
        {
            // 尋找是否已有此外部帳號
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.ExternalId == externalUserId && u.ExternalProvider == provider);

            if (user != null)
            {
                return (user, null);
            }

            // 檢查是否有相同的電子郵件
            user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                // 如果有相同電子郵件，進行帳號綁定
                user.ExternalId = externalUserId;
                user.ExternalProvider = provider;
                user.IsExternalAccount = true;
                await _userManager.UpdateAsync(user);
                _logger.LogInformation("已將外部帳號 {Provider}:{ExternalId} 綁定到現有用戶 {UserId}", provider, externalUserId, user.Id);
                return (user, null);
            }

            // 建立新使用者
            var username = $"{provider.ToLower()}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";

            user = new ApplicationUser
            {
                UserName = username,
                Email = email,
                EmailConfirmed = true, // 外部登入視為已驗證
                ExternalId = externalUserId,
                ExternalProvider = provider,
                IsExternalAccount = true,
                IsApproved = true, // 外部登入無需審核
                IsActive = true,
                LockoutEnabled = false,
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError("建立外部登入用戶失敗: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                FireLoginNotificationSafe(tenantId, email, email, isSuccess: false, provider, "建立外部登入用戶失敗");
                return (null, new AuthenticateResponse
                {
                    OperationResult = new OperationResult(false, "建立外部登入用戶失敗", StatusCodes.Status500InternalServerError)
                });
            }

            _logger.LogInformation("已建立新外部登入用戶: {UserName}, Email: {Email}", username, email);
            // 建立基本的用戶詳情
            await userService.CreateExternalUserDetailAsync(
                user.Id, username, name, email, tenantId, CancellationToken.None);
            await _context.SaveChangesAsync(CancellationToken.None);

            _logger.LogInformation("已創建外部登入用戶 {Provider}:{ExternalId} 用戶ID: {UserId}", provider, externalUserId, user.Id);
            return (user, null);
        }

        private void FireLoginNotificationSafe(
            string tenantId, string userName, string? officialEmail, bool isSuccess, string provider, string? failureReason = null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _loginNotificationService.SendLoginNotificationAsync(
                        tenantId: tenantId,
                        userName: userName,
                        officialEmail: officialEmail ?? userName,
                        isSuccess: isSuccess,
                        failureReason: failureReason,
                        ipAddress: "Unknown");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "觸發外部登入通知時發生未預期的錯誤。TenantId: {TenantId}, UserName: {UserName}, Provider: {Provider}",
                        tenantId, userName, provider);
                }
            });
        }

        public async Task<OperationResult> CompleteRegistrationAsync(string userId, ExternalUserRegistrationDto userDetails)
        {
            try
            {
                var userService = _tenantUserServiceFactory.GetServiceByTenantId(userDetails.TenantId);
                await _context.BeginTransactionAsync(); // 開始交易

                // 使用 IUserService 的新方法，將租戶判斷委託給具體的服務實現
                var result = await userService.UpdateExternalUserRegistrationAsync(
                    userId,
                    userDetails,
                    CancellationToken.None);

                if (result.Success)
                {
                    await _context.CommitTransactionAsync(); // 提交交易
                }
                else
                {
                    await _context.RollbackTransactionAsync(); // 回滾交易
                }

                return result;
            }
            catch (Exception ex)
            {
                await _context.RollbackTransactionAsync(); // 回滾交易
                _logger.LogError(ex, "完成註冊流程時出錯: {UserId}", userId);
                return new OperationResult(false, "完成註冊流程時出錯", StatusCodes.Status500InternalServerError);
            }
        }
    }
}