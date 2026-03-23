using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos.LoginUserInfo;
using SkyLabIdP.Application.Dtos.User.Authentication;
using SkyLabIdP.Application.Dtos;
using SkyLabIdP.Domain;
using SkyLabIdP.Domain.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using SkyLabIdP.Application.SystemApps.Users.Commands.LoginUser;
using SkyLabIdP.Application.SystemApps.Users.Commands.ChangePassWord;
using SkyLabIdP.Domain.Enums;
using SkyLabIdP.Application.Dtos.SkyLabDocUserDetail;
using SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Commands.CreateAccout;
using SkyLabIdP.Application.SystemApps.Users.Commands.ResetPassword;
using SkyLabIdP.Application.Dtos.FunctionGroup;
using System.Security.Claims;
using SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Queries;
using SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Commands.PutAccountDetail;
using SkyLabIdP.Application.Dtos.Email;
using SkyLabIdP.Application.Dtos.Function;
using SkyLabIdP.Application.Dtos.AcctMaintain;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;
using SkyLabIdP.Application.SystemApps.Services.ServiceSettings;

namespace SkyLabIdP.Application.SystemApps.Services;

public abstract class AbstractLoginUserInfoService : IUserService
{
    private const string UnknownValue = "Unknown";
    private const string InvalidCredentialsMessage = "帳號或密碼錯誤";
    private const string UserNotExistsMessage = "使用者不存在";

    protected readonly IUnitOfWork _unitOfWork;
    protected readonly UserManager<ApplicationUser> _userManager;
    protected readonly IJwtService _jwtService;
    protected readonly IEmailService _emailService;
    protected readonly ILoginNotificationService _loginNotificationService;
    protected readonly IConfiguration _configuration;
    protected readonly IDataProtectionService _dataProtectionService;
    protected readonly ILogger<AbstractLoginUserInfoService> _logger;
    protected readonly ISaltGenerator _saltGenerator;

    protected readonly SkyLabIdPMapper _mapper;
    protected readonly AsyncRetryPolicy _retryPolicy;

    protected readonly IDistributedCache _cache;

    protected readonly ICaptchaService _captchaService;

    private string _currentTenantId = string.Empty; // 當前請求的租戶ID

    protected AbstractLoginUserInfoService(LoginUserInfoServiceSettings loginUserInfoServiceSettings)
    {

        _unitOfWork = loginUserInfoServiceSettings.UnitOfWork;
        _userManager = loginUserInfoServiceSettings.UserManager;
        _configuration = loginUserInfoServiceSettings.Configuration;
        _dataProtectionService = loginUserInfoServiceSettings.Dataprotectionservice;
        _jwtService = loginUserInfoServiceSettings.JwtService;
        _logger = loginUserInfoServiceSettings.Logger;
        _emailService = loginUserInfoServiceSettings.EmailService;
        _loginNotificationService = loginUserInfoServiceSettings.LoginNotificationService;
        _saltGenerator = loginUserInfoServiceSettings.SaltGenerator;
        _mapper = loginUserInfoServiceSettings.Mapper;
        _cache = loginUserInfoServiceSettings.Cache; // 注入 IDistributedCache
        _captchaService = loginUserInfoServiceSettings.CaptchaService;
        _retryPolicy = Policy
            .Handle<SqlException>(ex => ex.Number == 1205) // SQL Server deadlock
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (exception, timeSpan, retryCount, context) =>
            {
                _logger.LogWarning("Deadlock detected, retrying... Attempt {RetryCount}", retryCount);
            });

    }
    /// <summary>
    /// 取得當前租戶ID
    /// </summary>
    public string GetCurrentTenantId()
    {
        return _currentTenantId;
    }
    private LoginUserInfoDto GetEmptyloginUserInfoDto(string loginUserId)
    {
        return new LoginUserInfoDto
        {
            UserId = _dataProtectionService.Protect(loginUserId),
            IsUserEligible = false,
            BranchCode = string.Empty,
            RegionCode = string.Empty,
            IsActive = false,
            IsApproved = false,
            LockoutEnabled = false,
            SystemRole = string.Empty,
            UserName = string.Empty,
            OfficialEmail = string.Empty,
            FunctionGroups = new List<FunctionGroupDto>()
        };
    }


    /// <summary>
    /// Handle login user command
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<AuthenticateResponse> HandleLoginUserCommandAsync(LoginUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("開始處理登入請求，使用者: {UserName}, 租戶: {TenantId}", request.UserName, request.TenantId);
        
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                
                var isCaptchaEnabled = _configuration["Captcha:EnableCaptcha"]?.ToLower() == "true";
                if (isCaptchaEnabled && !await _captchaService.ValidateCaptchaCodeAsync(_dataProtectionService.Unprotect(request.CaptchaId), request.CaptchaCode, cancellationToken))
                {
                    _logger.LogWarning("驗證碼錯誤，使用者: {UserName}", request.UserName);
                    return new AuthenticateResponse
                    {
                        OperationResult = new OperationResult(false, "驗證碼錯誤", StatusCodes.Status400BadRequest)
                    };
                }
                
                var user = await FindUserAsync(request.UserName);
                if (user == null) 
                {
                    _logger.LogWarning("找不到使用者: {UserName}", request.UserName);
                    return UserNotFoundResponse(request.UserName, ipAddress: request.IpAddress);
                }

                // 驗證租戶
                var tenantError = await ValidateTenantForLoginAsync(user, request, cancellationToken);
                if (tenantError != null)
                    return tenantError;
                
                _currentTenantId = request.TenantId;
                await ReMoveLoginUserInfoByCacheAsync(user, cancellationToken);
                
                _logger.LogDebug("取得使用者 {UserId} 登入資訊", user.Id);
                var loginUserInfo = await GetLoginUserInfoAsync(user.Id, cancellationToken);
                
                if (await ValidateUserAsync(user, loginUserInfo, cancellationToken) is AuthenticateResponse errorResponse)
                {
                    _logger.LogWarning("使用者 {UserId} 驗證失敗: {ErrorMessage}", user.Id, errorResponse.OperationResult?.Messages?.FirstOrDefault());
                    return errorResponse;
                }
                
                _logger.LogDebug("檢查使用者 {UserId} 密碼", user.Id);
                if (await CheckPasswordAsync(user, request.Password, cancellationToken, request.UserName, request.IpAddress) is AuthenticateResponse passwordErrorResponse)
                {
                    _logger.LogWarning("使用者 {UserId} 密碼驗證失敗", user.Id);
                    return passwordErrorResponse;
                }
                
                return await CreateSuccessResponseAsync(user, loginUserInfo, cancellationToken, 
                    userName: request.UserName, 
                    ipAddress: request.IpAddress);
            }
            catch (SqlException ex) when (ex.Number == 1205)
            {
                throw; // Retry policy already logs deadlock retries
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "處理登入請求時發生未預期的錯誤，使用者: {UserName}", request.UserName);
                return await HandleExceptionAsync(ex);
            }
        });
    }

    /// <summary>
    /// 驗證登入請求的租戶有效性
    /// </summary>
    private async Task<AuthenticateResponse?> ValidateTenantForLoginAsync(
        ApplicationUser user, LoginUserCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.TenantId))
        {
            _logger.LogWarning("使用者 {UserName} 登入時未指定租戶ID", request.UserName);
            return new AuthenticateResponse
            {
                OperationResult = new OperationResult(false, "必須指定租戶ID", StatusCodes.Status400BadRequest)
            };
        }

        if (!Enum.TryParse<Tenants>(request.TenantId, ignoreCase: true, out _))
        {
            _logger.LogWarning("使用者 {UserName} 使用無效的租戶ID {TenantId} 進行登入", request.UserName, request.TenantId);
            TriggerLoginNotification(
                userName: request.UserName,
                officialEmail: null,
                ipAddress: request.IpAddress ?? UnknownValue,
                isSuccess: false,
                failureReason: "無效的租戶ID");
            return new AuthenticateResponse
            {
                OperationResult = new OperationResult(false, "無效的租戶ID", StatusCodes.Status400BadRequest)
            };
        }

        var isValidTenant = await ValidateUserTenantAsync(user.Id, request.TenantId, cancellationToken);
        if (!isValidTenant)
        {
            _logger.LogWarning("使用者 {UserName} 嘗試使用不屬於的租戶 {TenantId} 進行登入", request.UserName, request.TenantId);
            TriggerLoginNotification(
                userName: request.UserName,
                officialEmail: user.Email,
                ipAddress: request.IpAddress ?? UnknownValue,
                isSuccess: false,
                failureReason: "使用者不屬於指定的租戶");
            return new AuthenticateResponse
            {
                OperationResult = new OperationResult(false, "使用者不屬於指定的租戶", StatusCodes.Status403Forbidden)
            };
        }

        return null;
    }

    // 定義快取鍵，包含租戶ID以區分不同租戶的快取
    private static string LoginUserInfoCacheKey(string tenantId, string loginUserId)
    {
        return $"LoginUserInfo_{tenantId}_{loginUserId}".ToUpper();
    }
    /// <summary>
    /// Get login user info
    /// </summary>
    /// <param name="loginUserId"></param>
    /// <returns></returns>
    public async Task<LoginUserInfoDto> GetLoginUserInfoAsync(string loginUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(loginUserId))
        {
            _logger.LogWarning("GetLoginUserInfoAsync 接收到空的 loginUserId");
            return GetEmptyloginUserInfoDto(loginUserId);
        }

        // Then replace the placeholder with:
        var cacheKey = LoginUserInfoCacheKey(_currentTenantId, loginUserId);
        _logger.LogDebug("檢查快取，CacheKey: {CacheKey}", cacheKey);

        // 嘗試從快取中獲取資料
        var cachedBytes = await _cache.GetAsync(cacheKey, cancellationToken);
        if (cachedBytes != null && cachedBytes.Length > 0)
        {
            try
            {
                // 反序列化快取資料
                var cachedData = Encoding.UTF8.GetString(cachedBytes);
                var cachedUserInfo = JsonSerializer.Deserialize<LoginUserInfoDto>(cachedData);
                if (cachedUserInfo != null)
                {
                    _logger.LogInformation("從快取中獲取 LoginUserInfo，UserId: {UserId}，TenantId: {TenantId}", loginUserId, _currentTenantId);
                    return cachedUserInfo;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "反序列化快取資料失敗，將重新查詢資料庫，CacheKey: {CacheKey}", cacheKey);
            }
        }

        _logger.LogDebug("快取中沒有資料，從資料庫查詢，UserId: {UserId}, TenantId: {TenantId}", loginUserId, _currentTenantId);

        // 先查詢 ApplicationUser，後續可重用
        var user = await _userManager.FindByIdAsync(loginUserId);
        if (user == null)
        {
            _logger.LogWarning("找不到 ApplicationUser，UserId: {UserId}", loginUserId);
            return GetEmptyloginUserInfoDto(loginUserId);
        }

        // 獲取用戶資訊（子類只查 UserDetail 表，不再 JOIN AspNetUsers）
        var userInfo = await GetTenantUserInfoAsync(loginUserId, user);

        if (userInfo.IsUserEligible)
        {
            _logger.LogDebug("使用者符合資格，開始處理權限和功能群組，UserId: {UserId}", loginUserId);

            var claimsDictionary = await GetUserClaimsDictionaryAsync(user);
            var functionGroupDtos = await GetFunctionGroupsAsync(cancellationToken);

            // Process the function permissions based on claims
            ProcessFunctionPermissions(functionGroupDtos, claimsDictionary);

            // Add function group DTOs or permissions info to userInfo if needed
            userInfo.FunctionGroups = functionGroupDtos;
                
            _logger.LogDebug("成功處理使用者權限和功能群組，UserId: {UserId}", loginUserId);
        }
        else
        {
            _logger.LogWarning("使用者不符合資格，UserId: {UserId}, IsUserEligible: {IsUserEligible}", 
                loginUserId, userInfo.IsUserEligible);
        }

        // 將結果序列化並存入快取
        try
        {
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) // 設定快取過期時間，根據需求調整
            };
            var serializedData = JsonSerializer.Serialize(userInfo);
            var serializedBytes = Encoding.UTF8.GetBytes(serializedData);
            await _cache.SetAsync(cacheKey, serializedBytes, cacheOptions, cancellationToken);

            _logger.LogInformation("將 LoginUserInfo 存入快取，UserId: {UserId}，TenantId: {TenantId}", loginUserId, _currentTenantId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "存入快取失敗，但不影響主要流程，CacheKey: {CacheKey}", cacheKey);
        }

        return userInfo;
    }
    // 子類必須實現此方法來提供特定租戶的使用者資訊（只查 UserDetail 表，AspNetUsers 資料由 ApplicationUser 提供）
    protected abstract Task<LoginUserInfoDto> GetTenantUserInfoAsync(string loginUserId, ApplicationUser user);





    /// <summary>
    /// 驗證使用者是否屬於指定的租戶
    /// </summary>
    /// <param name="userId">使用者ID</param>
    /// <param name="tenantId">租戶ID</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>如果使用者屬於該租戶則返回true</returns>
    private async Task<bool> ValidateUserTenantAsync(string userId, string tenantId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.UserTenants.ExistsAsync(userId, tenantId, cancellationToken);
    }

    /// <summary>
    /// Find user by user name
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    private async Task<ApplicationUser> FindUserAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        user ??= _userManager.Users.FirstOrDefault(u => u.Email == userName);
        return user ?? new ApplicationUser();
    }
    /// <summary>
    /// Find user by user id
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    private async Task<ApplicationUser> FindUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException($"沒有使用者ID '{userId}'");
        return user;
    }
    /// <summary>
    /// User not found response
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    private AuthenticateResponse UserNotFoundResponse(string userName, string? ipAddress = null)
    {
        _logger.LogWarning("User {UserName} 登入失敗 - 帳號或密碼錯誤", userName);
        
        // 觸發失敗登入通知（非阻塞）
        TriggerLoginNotification(
            userName: userName,
            officialEmail: null,
            ipAddress: ipAddress ?? UnknownValue,
            isSuccess: false,
            failureReason: InvalidCredentialsMessage);
        
        // 🔒 統一錯誤訊息，避免洩露帳號是否存在 (API8:2023)
        return new AuthenticateResponse
        {
            OperationResult = new OperationResult(false, InvalidCredentialsMessage, StatusCodes.Status401Unauthorized)
        };
    }
    /// <summary>
    /// Validate user
    /// </summary>
    /// <param name="user"></param>
    /// <param name="loginUserInfo"></param>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<AuthenticateResponse?> ValidateUserAsync(ApplicationUser user, LoginUserInfoDto loginUserInfo, CancellationToken cancellationToken)
    {
        if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.Now)
            return await LockoutResponseAsync(user, cancellationToken);

        if (user.IsMigrated && !user.IsMigratedAndReSetPWed)
            return await MigrateResponseAsync(user, loginUserInfo, cancellationToken);

        if (await IsPasswordExpiredAsync(user))
            return await PasswordExpiredResponseAsync(user, loginUserInfo, cancellationToken);

        return null;
    }
    /// <summary>
    /// Check password is expired
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    private async Task<bool> IsPasswordExpiredAsync(ApplicationUser user)
    {
        _logger.LogDebug("檢查使用者 {UserId} 密碼是否過期", user.Id);
        
        try
        {
            var lastPasswordChange = await _unitOfWork.PasswordHistories.GetLatestByUserIdAsync(user.Id);

            if (!int.TryParse(_configuration["PasswordLimitDays"], out int passwordLimitDays))
            {
                passwordLimitDays = (int)SystemInfo.DefPasswordLimitDays;
                _logger.LogWarning("無法解析 PasswordLimitDays 設定，使用預設值: {DefaultDays}", passwordLimitDays);
            }

            if (lastPasswordChange == null)
            {
                _logger.LogInformation("使用者 {UserId} 沒有密碼變更歷史記錄，視為未過期", user.Id);
                return false;
            }

            var daysSinceChange = (DateTime.UtcNow - lastPasswordChange.PasswordChangeDate).TotalDays;
            var isExpired = daysSinceChange > passwordLimitDays;
            
            _logger.LogInformation("使用者 {UserId} 密碼距離上次變更 {DaysSinceChange:F1} 天，限制 {PasswordLimitDays} 天，是否過期: {IsExpired}", 
                user.Id, daysSinceChange, passwordLimitDays, isExpired);

            return isExpired;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檢查使用者 {UserId} 密碼過期狀態時發生錯誤", user.Id);
            // 發生錯誤時假設密碼未過期，避免阻擋使用者登入
            return false;
        }
    }
    /// <summary>
    /// Check password
    /// </summary>
    /// <param name="user"></param>
    /// <param name="password"></param>
    /// <param name="loginUserInfo"></param>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<AuthenticateResponse?> CheckPasswordAsync(ApplicationUser user, string password, CancellationToken cancellationToken, string? userName = null, string? ipAddress = null)
    {
        var result = await _userManager.CheckPasswordAsync(user, password);
        if (!result)
        {
            await _userManager.AccessFailedAsync(user);
            return await HandleFailedPasswordAsync(user, cancellationToken, userName, ipAddress);
        }
        return null;
    }
    /// <summary>
    /// Handle failed password
    /// </summary>
    /// <param name="user"></param>
    /// <param name="loginUserInfo"></param>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<AuthenticateResponse> HandleFailedPasswordAsync(ApplicationUser user, CancellationToken cancellationToken, string? userName = null, string? ipAddress = null)
    {
        if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.Now)
            return await LockoutResponseAsync(user, cancellationToken);

        _logger.LogWarning("User {UserName} 登入失敗 - 密碼錯誤", user.UserName);

        // 觸發失敗登入通知（非阻塞）
        TriggerLoginNotification(
            userName: userName ?? user.UserName ?? user.Email ?? user.Id,
            officialEmail: user.Email,
            ipAddress: ipAddress ?? UnknownValue,
            isSuccess: false,
            failureReason: InvalidCredentialsMessage);

        await ReMoveLoginUserInfoByCacheAsync(user, cancellationToken);
        // 🔒 統一錯誤訊息，避免洩露具體失敗原因 (API8:2023)
        return new AuthenticateResponse(GetEmptyloginUserInfoDto(user.Id), _dataProtectionService.Protect(user.Id), "")
        {
            OperationResult = new OperationResult(false, InvalidCredentialsMessage, StatusCodes.Status401Unauthorized)
        };
    }
    /// <summary>
    /// Create success response
    /// </summary>
    /// <param name="user"></param>
    /// <param name="loginUserInfo"></param>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<AuthenticateResponse> CreateSuccessResponseAsync(
        ApplicationUser user,
        LoginUserInfoDto loginUserInfo,
        CancellationToken cancellationToken,
        string? userName = null,
        string? ipAddress = null)
    {
        _logger.LogDebug("開始創建成功登入回應，使用者: {UserId}, 租戶: {TenantId}", user.Id, _currentTenantId);
        
        try
        {
            var userDetail = await GetUserDetailAsync(user.Id, cancellationToken);

            if (userDetail == null)
            {
                _logger.LogWarning("找不到使用者詳細資料，使用者: {UserId}, 租戶: {TenantId}", user.Id, _currentTenantId);
                if (user.UserName == null)
                {
                    throw new InvalidOperationException("UserName cannot be null.");
                }
                return await UserDetailNotFoundResponseAsync(user.UserName, loginUserInfo);
            }

            // 如果用戶處於鎖定狀態，且鎖定結束時間不為null，則解鎖用戶
            if (user.LockoutEnd != null)
            {
                _logger.LogInformation("使用者 {UserId} 處於鎖定狀態，正在解鎖", user.Id);
                await UnlockUserAsync(user);
            }

            if (user.LockoutEnabled && user.AccessFailedCount < 3 && user.LockoutEnd == null)
            {
                _logger.LogWarning("使用者 {UserId} 被手動鎖定", user.Id);
                return await UserManuallyLockedResponseAsync(user, cancellationToken);
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("使用者 {UserId} 已停用", user.Id);
                return await UserInactiveResponseAsync(user, cancellationToken);
            }

            // 完成登入後復原使用者狀態
            await UnlockUserAsync(user);

            // 檢查密碼是否過期
            var passwordExpiryResponse = await CheckPasswordExpiryInSuccessFlowAsync(user, loginUserInfo, cancellationToken);
            if (passwordExpiryResponse != null)
                return passwordExpiryResponse;

            if (user.IsMigrated && !user.IsMigratedAndReSetPWed)
            {
                _logger.LogInformation("使用者 {UserId} 是遷移帳號且尚未重設密碼", user.Id);
                return await MigrateResponseAsync(user, loginUserInfo, cancellationToken);
            }

            bool isLockedOut = user.LockoutEnabled && user.AccessFailedCount == 0 && user.LockoutEnd.HasValue && user.LockoutEnd < DateTimeOffset.Now;
            if (isLockedOut)
            {
                _logger.LogInformation("使用者 {UserId} 鎖定已過期，正在解鎖", user.Id);
                await UnlockUserAsync(user);
            }

            // 更新最後登入時間 (Phase 3: Dapper UoW transaction)
            _logger.LogDebug("更新使用者 {UserId} 最後登入時間", user.Id);
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                await UpdateLastLoginTimeAsync(user.Id, cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
            // 清除快取，異步執行以提升效能
            _ = Task.Run(() => ReMoveLoginUserInfoByCacheAsync(user, cancellationToken), cancellationToken);

            loginUserInfo = await GetLoginUserInfoAsync(user.Id, cancellationToken);

            // 生成存取令牌和刷新令牌
            _logger.LogDebug("正在生成 JWT 令牌，使用者: {UserId}, 租戶: {TenantId}", user.Id, _currentTenantId);
            string accessToken = await _jwtService.GenerateAccessTokenAsync(user, _currentTenantId);
            string refreshToken = await _jwtService.GenerateRefreshTokenAsync(user, _currentTenantId);

            _logger.LogInformation("使用者 {UserId} 登入成功，租戶: {TenantId}", user.Id, _currentTenantId);
            
            // 觸發成功登入通知（非阻塞）
            TriggerLoginNotification(
                userName: userName ?? user.UserName ?? user.Email ?? user.Id,
                officialEmail: user.Email,
ipAddress: ipAddress ?? UnknownValue,
                isSuccess: true);
            
            return new AuthenticateResponse(loginUserInfo, _dataProtectionService.Protect(user.Id), accessToken, refreshToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "創建成功登入回應時發生錯誤，使用者: {UserId}, 租戶: {TenantId}", user.Id, _currentTenantId);
            
            return new AuthenticateResponse
            {
                OperationResult = new OperationResult(false, "登入處理失敗", StatusCodes.Status500InternalServerError)
            };
        }
    }

    /// <summary>
    /// 在成功登入流程中檢查密碼是否過期
    /// </summary>
    private async Task<AuthenticateResponse?> CheckPasswordExpiryInSuccessFlowAsync(
        ApplicationUser user, LoginUserInfoDto loginUserInfo, CancellationToken cancellationToken)
    {
        var lastPasswordChange = await _unitOfWork.PasswordHistories.GetLatestByUserIdAsync(user.Id, cancellationToken);
        var maxPasswordAgeInDays = _configuration.GetValue<int>("SecuritySettings:MaxPasswordAgeInDays", 90);

        if (lastPasswordChange == null)
            return null;

        var daysSinceChange = (DateTime.UtcNow - lastPasswordChange.PasswordChangeDate).TotalDays;
        if (daysSinceChange > maxPasswordAgeInDays)
        {
            _logger.LogWarning("使用者 {UserId} 密碼已過期", user.Id);
            return await PasswordExpiredResponseAsync(user, loginUserInfo, cancellationToken);
        }

        return null;
    }

    /// <summary>
    /// 獲取使用者詳細資料，由子類實現具體租戶邏輯
    /// </summary>
    /// <param name="userId">使用者ID</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>使用者詳細資料</returns>
    protected abstract Task<object> GetUserDetailAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// 更新使用者最後登入時間，由子類實現具體租戶邏輯
    /// </summary>
    /// <param name="userId">使用者ID</param>
    /// <param name="cancellationToken">取消權杖</param>
    protected abstract Task UpdateLastLoginTimeAsync(string userId, CancellationToken cancellationToken);
    /// <summary>
    /// Unlocks the user if necessary.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    private async Task UnlockUserAsync(ApplicationUser user)
    {
        await _userManager.ResetAccessFailedCountAsync(user);
        await _userManager.SetLockoutEnabledAsync(user, false);
        await _userManager.SetLockoutEndDateAsync(user, null);
    }

    private async Task ReMoveLoginUserInfoByCacheAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        // 清除快取
        var cacheKey = LoginUserInfoCacheKey(_currentTenantId, user.Id);
        await _cache.RemoveAsync(cacheKey, cancellationToken);
        _logger.LogInformation("清除快取，CacheKey: {CacheKey}", cacheKey);
    }

    /// <summary>
    /// User detail not found response
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="loginUserInfo"></param>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private Task<AuthenticateResponse> UserDetailNotFoundResponseAsync(string userName, LoginUserInfoDto loginUserInfo)
    {
        _logger.LogWarning("User {UserName} 使用者不存在.", userName);

        return Task.FromResult(new AuthenticateResponse(loginUserInfo, _dataProtectionService.Protect(userName), "")
        {
            OperationResult = new OperationResult(false, UserNotExistsMessage, StatusCodes.Status404NotFound)
        });
    }
    /// <summary>
    /// User inactive response
    /// </summary>
    /// <param name="user"></param>
    /// <param name="loginUserInfo"></param>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<AuthenticateResponse> UserInactiveResponseAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        _logger.LogWarning("User {UserName} 停用中.", user.UserName);
        // 清除快取，異步執行以提升效能
        _ = Task.Run(() => ReMoveLoginUserInfoByCacheAsync(user, cancellationToken), cancellationToken);
        var loginUserInfo = await GetLoginUserInfoAsync(user.Id, cancellationToken);
        return new AuthenticateResponse(loginUserInfo, _dataProtectionService.Protect(user.Id), "")
        {
            OperationResult = new OperationResult(false, "帳號停用中。請聯繫系統管理者。", StatusCodes.Status401Unauthorized)
        };
    }
    /// <summary>
    /// User manually locked response
    /// </summary>
    /// <param name="user"></param>
    /// <param name="loginUserInfo"></param>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<AuthenticateResponse> UserManuallyLockedResponseAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        _logger.LogWarning("User {UserName} 帳號已被手動鎖定.", user.UserName);
        // 清除快取，異步執行以提升效能
        _ = Task.Run(() => ReMoveLoginUserInfoByCacheAsync(user, cancellationToken), cancellationToken);
        var loginUserInfo = await GetLoginUserInfoAsync(user.Id, cancellationToken);
        return new AuthenticateResponse(loginUserInfo, _dataProtectionService.Protect(user.Id), "")
        {
            OperationResult = new OperationResult(false, "“帳號已被鎖定。請聯繫系統管理者”", StatusCodes.Status401Unauthorized)
        };
    }
    /// <summary>
    /// Lockout response
    /// </summary>
    /// <param name="user"></param>
    /// <param name="loginUserInfo"></param>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task<AuthenticateResponse> LockoutResponseAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        _logger.LogWarning("User {UserName} 密碼連續錯誤超過三次，帳號已被鎖定到 {LockoutEnd}.", user.UserName, user.LockoutEnd);
        await _userManager.SetLockoutEnabledAsync(user, true);
        await ReMoveLoginUserInfoByCacheAsync(user, cancellationToken);
        return new AuthenticateResponse(await GetLoginUserInfoAsync(user.Id, cancellationToken), _dataProtectionService.Protect(user.Id), "")
        {
            OperationResult = new OperationResult(false, "密碼連續錯誤超過三次，帳號已被鎖定。請十分鐘後再進行登入", StatusCodes.Status401Unauthorized)
        };
    }
    /// <summary>
    /// Migrate response
    /// </summary>
    /// <param name="user"></param>
    /// <param name="loginUserInfo"></param>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task<AuthenticateResponse> MigrateResponseAsync(ApplicationUser user, LoginUserInfoDto loginUserInfo, CancellationToken cancellationToken)
    {
        _logger.LogInformation("User {UserName} 是舊帳號需要重設密碼.", user.UserName);
        loginUserInfo = await GetLoginUserInfoAsync(user.Id, cancellationToken);

        // 生成存取令牌和刷新令牌
        string accessToken = await _jwtService.GenerateAccessTokenAsync(user, _currentTenantId);
        string refreshToken = await _jwtService.GenerateRefreshTokenAsync(user, _currentTenantId);

        return new AuthenticateResponse(loginUserInfo, _dataProtectionService.Protect(user.Id), accessToken, refreshToken, mustResetPassword: true);
    }
    /// <summary>
    /// Password expired response
    /// </summary>
    /// <param name="user"></param>
    /// <param name="loginUserInfo"></param>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task<AuthenticateResponse> PasswordExpiredResponseAsync(ApplicationUser user, LoginUserInfoDto loginUserInfo, CancellationToken cancellationToken)
    {
        _logger.LogWarning("User {UserName} 密碼更新已超過三個月.", user.UserName);
        loginUserInfo = await GetLoginUserInfoAsync(user.Id, cancellationToken);

        // 生成存取令牌和刷新令牌
        string accessToken = await _jwtService.GenerateAccessTokenAsync(user, _currentTenantId);
        string refreshToken = await _jwtService.GenerateRefreshTokenAsync(user, _currentTenantId);

        return new AuthenticateResponse(loginUserInfo, _dataProtectionService.Protect(user.Id), accessToken, refreshToken, mustChangePassword: true)
        {
            OperationResult = new OperationResult(false, "密碼更新已超過三個月，請重新設定密碼", StatusCodes.Status403Forbidden)
        };
    }
    /// <summary>
    /// Handle exception
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<AuthenticateResponse> HandleExceptionAsync(Exception ex)
    {
        _logger.LogError(ex, "處理登入請求時發生錯誤");
        
        // 嘗試發送錯誤通知 Email，但不讓它影響主要流程
        try
        {
            await _emailService.SendErrorEmailAsync(ex);
            _logger.LogDebug("錯誤通知 Email 發送成功");
        }
        catch (Exception emailEx)
        {
            _logger.LogWarning(emailEx, "發送錯誤通知 Email 失敗，但不影響主要流程");
        }

        return new AuthenticateResponse
        {
            OperationResult = new OperationResult(false, "登入處理失敗", StatusCodes.Status500InternalServerError)
        };
    }


    /// <summary>
    /// Handle change password command
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<OperationResult> HandleChangePassWordCommandAsync(ChangePassWordCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        string? unprotectedUserId = null;
        string? userName = null;
        
        _logger.LogInformation("開始處理修改密碼請求，UserId: {ProtectedUserId}", request.ChangePassWordRequest.UserId);
        
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // 步驟1：解密使用者ID
            _logger.LogDebug("步驟1：解密使用者ID");
            unprotectedUserId = _dataProtectionService.Unprotect(request.ChangePassWordRequest.UserId);
            
            // 步驟2：查詢使用者
            var user = await _userManager.FindByIdAsync(unprotectedUserId);
            if (user == null) 
            {
                _logger.LogWarning("步驟2失敗：找不到使用者，UserId: {UserId}", unprotectedUserId);
                return UserNotFoundResponse();
            }
            userName = user.UserName;

            // 步驟3：檢查新密碼是否與使用者名稱相同
            if (IsUserNameEqualsNewPassword(user, request.ChangePassWordRequest.NewPassword)) 
            {
                _logger.LogWarning("步驟3失敗：新密碼與使用者名稱相同，UserName: {UserName}", userName);
                return UserNameEqualsNewPassword();
            }

            // 步驟4：驗證當前密碼
            if (!await ValidateCurrentPasswordAsync(user, request.ChangePassWordRequest.Password)) 
            {
                _logger.LogWarning("步驟4失敗：當前密碼驗證失敗，UserName: {UserName}", userName);
                return InvalidPasswordResponse();
            }

            // 步驟5：驗證新密碼與確認密碼是否一致
            if (!ValidateNewPassword(request.ChangePassWordRequest.NewPassword, request.ChangePassWordRequest.ConfirmPassword)) 
            {
                _logger.LogWarning("步驟5失敗：新密碼與確認密碼不一致，UserName: {UserName}", userName);
                return PasswordMismatchResponse();
            }

            // 步驟6：檢查新密碼是否與前三次密碼相同
            if (await IsPasswordSameAsPreviousAsync(user, request.ChangePassWordRequest.NewPassword)) 
            {
                _logger.LogWarning("步驟6失敗：新密碼與前三次密碼中的一次相同，UserName: {UserName}", userName);
                return PasswordSameAsOldResponse();
            }

            // 步驟7：驗證密碼規則
            if (!await ValidatePasswordRules(user, request.ChangePassWordRequest.NewPassword)) 
            {
                _logger.LogWarning("步驟7失敗：新密碼不符合密碼規則，UserName: {UserName}", userName);
                return InvalidPasswordRulesResponse();
            }

            // 步驟8：變更密碼
            _logger.LogDebug("步驟8：執行密碼變更，UserName: {UserName}", userName);
            if (!await ChangePasswordAsync(user, request.ChangePassWordRequest.Password, request.ChangePassWordRequest.NewPassword)) 
            {
                _logger.LogError("步驟8失敗：密碼變更失敗，UserName: {UserName}", userName);
                return PasswordChangeFailedResponse();
            }

            // 步驟9：儲存密碼歷史記錄
            if (!await SavePasswordHistoryAsync(user, request.ChangePassWordRequest.NewPassword, cancellationToken)) 
            {
                _logger.LogError("步驟9失敗：密碼歷史記錄儲存失敗，UserName: {UserName}", userName);
                return PasswordHistorySaveFailedResponse();
            }

            // 步驟10：提交資料庫交易
            _logger.LogDebug("步驟10：提交資料庫交易，UserName: {UserName}", userName);
            await _unitOfWork.CommitAsync(cancellationToken);
            
            // 步驟11：清除使用者快取
            await ReMoveLoginUserInfoByCacheAsync(user, cancellationToken);
            
            stopwatch.Stop();
            _logger.LogInformation("修改密碼成功完成，UserName: {UserName}，總執行時間: {ElapsedMs}ms", 
                userName, stopwatch.ElapsedMilliseconds);
            
            return new OperationResult(true, "已正確修改密碼，請重新登入", StatusCodes.Status200OK);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "修改密碼過程發生例外錯誤，UserName: {UserName}，UserId: {UserId}，執行時間: {ElapsedMs}ms，錯誤類型: {ExceptionType}，錯誤訊息: {ErrorMessage}", 
                userName ?? UnknownValue, 
                unprotectedUserId ?? UnknownValue, 
                stopwatch.ElapsedMilliseconds,
                ex.GetType().Name, 
                ex.Message);
            
            // 嘗試回滾交易
            try
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                _logger.LogDebug("資料庫交易已成功回滾");
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "資料庫交易回滾失敗，UserName: {UserName}，回滾錯誤: {RollbackError}", 
                    userName ?? UnknownValue, rollbackEx.Message);
            }
            
            // 嘗試發送錯誤通知 Email，但不讓它影響主要流程
            try
            {
                await _emailService.SendErrorEmailAsync(ex);
            }
            catch (Exception emailEx)
            {
                _logger.LogWarning(emailEx, "發送錯誤通知 Email 失敗，但不影響主要流程，Email錯誤: {EmailError}", emailEx.Message);
            }
            
            return new OperationResult(false, "修改密碼失敗", StatusCodes.Status500InternalServerError);
        }
    }
    /// <summary>
    /// Handle reset password command
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<OperationResult> HandleResetPasswordAsync(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var UnprotectUserId = _dataProtectionService.Unprotect(request.UserId);
            var user = await FindUserByIdAsync(UnprotectUserId);
            if (user == null) return UserNotFoundResponse();

            if (IsUserNameEqualsNewPassword(user, request.NewPassword)) return UserNameEqualsNewPassword();

            if (!ValidateNewPassword(request.NewPassword, request.ConfirmPassword)) return PasswordMismatchResponse();

            if (await IsPasswordSameAsPreviousAsync(user, request.NewPassword)) return PasswordSameAsOldResponse();

            if (!await ValidatePasswordRules(user, request.NewPassword)) return InvalidPasswordRulesResponse();

            if (!await ResetUserPasswordAsync(user, request.Token, request.NewPassword)) return PasswordResetFailedResponse();

            if (!await UpdateUserStateAsync(user)) return UserStateUpdateFailedResponse();

            if (!await SavePasswordHistoryAsync(user, request.NewPassword, cancellationToken)) return PasswordHistorySaveFailedResponse();

            await _unitOfWork.CommitAsync(cancellationToken);
            await ReMoveLoginUserInfoByCacheAsync(user, cancellationToken);
            return new OperationResult(true, "重設密碼成功", StatusCodes.Status200OK);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reset password failed.");
            await _unitOfWork.RollbackAsync(cancellationToken);
            await _emailService.SendErrorEmailAsync(ex);
            return new OperationResult(false, "重設密碼失敗", StatusCodes.Status500InternalServerError);
        }
    }
    /// <summary>
    /// User State Update Failed Response
    /// </summary>
    /// <returns></returns>
    private static OperationResult UserStateUpdateFailedResponse()
    {
        return new OperationResult(false, "使用者狀態更新失敗", StatusCodes.Status500InternalServerError);
    }
    /// <summary>
    /// Check if the new password is the same as the previous password
    /// </summary>
    /// <param name="user"></param>
    /// <param name="newPassword"></param>
    /// <returns></returns>
    private async Task<bool> IsPasswordSameAsPreviousAsync(ApplicationUser user, string newPassword)
    {
        _logger.LogDebug("開始檢查新密碼是否與前三次密碼相同，UserId: {UserId}", user.Id);
        
        try
        {
            var passwordHistories = (await _unitOfWork.PasswordHistories.GetLastNByUserIdAsync(user.Id, 3)).ToList();

            _logger.LogDebug("查詢到 {HistoryCount} 筆密碼歷史記錄，UserId: {UserId}", passwordHistories.Count, user.Id);

            if (!passwordHistories.Any())
            {
                _logger.LogDebug("沒有密碼歷史記錄，允許使用新密碼，UserId: {UserId}", user.Id);
                return false;
            }

            for (int i = 0; i < passwordHistories.Count; i++)
            {
                var history = passwordHistories[i];
                _logger.LogDebug("檢查第 {Index} 次歷史密碼，變更日期: {ChangeDate}，UserId: {UserId}", 
                    i + 1, history.PasswordChangeDate, user.Id);
                
                try
                {
                    var decryptedSalt = _dataProtectionService.Unprotect(history.PasswordSalt);
                    var verificationResult = _userManager.PasswordHasher.VerifyHashedPassword(
                        user, history.HashedPassword, newPassword + decryptedSalt);
                    
                    if (verificationResult == PasswordVerificationResult.Success)
                    {
                        _logger.LogWarning("密碼驗證失敗：新密碼與第 {Index} 次歷史密碼相同（變更日期: {ChangeDate}），UserId: {UserId}", 
                            i + 1, history.PasswordChangeDate, user.Id);
                        return true;
                    }
                    
                    _logger.LogDebug("第 {Index} 次歷史密碼驗證通過（不相同），UserId: {UserId}", i + 1, user.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "解密或驗證第 {Index} 次歷史密碼時發生錯誤，跳過此記錄，UserId: {UserId}", i + 1, user.Id);
                }
            }
            
            _logger.LogInformation("密碼歷史檢查通過：新密碼與前 {HistoryCount} 次密碼皆不相同，UserId: {UserId}", 
                passwordHistories.Count, user.Id);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "檢查密碼歷史時發生錯誤，為安全考量拒絕密碼變更，UserId: {UserId}", user.Id);
            return true; // 發生錯誤時，為安全考量，假設密碼重複
        }
    }
    /// <summary>
    /// Reset user password
    /// </summary>
    /// <param name="user"></param>
    /// <param name="token"></param>
    /// <param name="newPassword"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<bool> ResetUserPasswordAsync(ApplicationUser user, string token, string newPassword)
    {
        var result = await _userManager.ResetPasswordAsync(user, _dataProtectionService.Unprotect(token), newPassword);
        return result.Succeeded;
    }

    /// <summary>
    /// Password reset failed response
    /// </summary>
    /// <returns></returns>
    private static OperationResult PasswordResetFailedResponse()
    {
        return new OperationResult(false, "密碼重設失敗。", StatusCodes.Status500InternalServerError);
    }
    /// <summary>
    /// Update user state
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<bool> UpdateUserStateAsync(ApplicationUser user)
    {
        if (user.IsMigrated && !user.IsMigratedAndReSetPWed)
        {
            user.IsMigratedAndReSetPWed = true;
            var updateResult = await _userManager.UpdateAsync(user);
            return updateResult.Succeeded;
        }
        return true;
    }
    /// <summary>
    /// User not found response
    /// </summary>
    /// <returns></returns>
    private static OperationResult UserNotFoundResponse()
    {
        // 🔒 統一錯誤訊息，避免洩露帳號存在性 (API8:2023)
        return new OperationResult(false, "帳號或密碼錯誤", StatusCodes.Status401Unauthorized);
    }
    /// <summary>
    /// Password mismatch response
    /// </summary>
    /// <param name="user"></param>
    /// <param name="password"></returns>
    /// <returns></returns>
    private async Task<bool> ValidateCurrentPasswordAsync(ApplicationUser user, string password)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }
    /// <summary>
    /// Invalid password response
    /// </summary>
    /// <returns></returns>
    private static OperationResult InvalidPasswordResponse()
    {
        // 🔒 統一錯誤訊息，避免洩露具體失敗原因 (API8:2023)
        return new OperationResult(false, "帳號或密碼錯誤", StatusCodes.Status401Unauthorized);
    }
    /// <summary>
    /// Validate new password not same as confirm Password
    /// </summary>
    /// <param name="newPassword"></param>
    /// <param name="confirmPassword"></param>
    /// <returns></returns>
    private bool ValidateNewPassword(string newPassword, string confirmPassword)
    {
        _logger.LogDebug("開始驗證新密碼與確認密碼是否一致");
        
        if (string.IsNullOrEmpty(newPassword))
        {
            _logger.LogWarning("驗證失敗：新密碼為空值");
            return false;
        }
        
        if (string.IsNullOrEmpty(confirmPassword))
        {
            _logger.LogWarning("驗證失敗：確認密碼為空值");
            return false;
        }
        
        var isMatch = newPassword == confirmPassword;
        
        if (isMatch)
        {
            _logger.LogDebug("密碼驗證成功：新密碼與確認密碼一致，密碼長度: {PasswordLength}", newPassword.Length);
        }
        else
        {
            _logger.LogWarning("密碼驗證失敗：新密碼與確認密碼不一致，新密碼長度: {NewPasswordLength}，確認密碼長度: {ConfirmPasswordLength}", 
                newPassword.Length, confirmPassword.Length);
        }
        
        return isMatch;
    }
    /// <summary>
    /// Password mismatch response
    /// </summary>
    /// <returns></returns>
    private static OperationResult PasswordMismatchResponse()
    {
        return new OperationResult(false, "新密碼與確認新密碼不一致", StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// Password same as last three response
    /// </summary>
    /// <returns></returns>
    private static OperationResult PasswordSameAsOldResponse()
    {
        return new OperationResult(false, "新密碼不可以跟最後三次密碼相同", StatusCodes.Status400BadRequest);
    }
    /// <summary>
    /// Validate password rules
    /// </summary>
    /// <param name="user"></param>
    /// <param name="newPassword"></param>
    /// <returns></returns>
    private async Task<bool> ValidatePasswordRules(ApplicationUser user, string newPassword)
    {
        _logger.LogDebug("開始驗證密碼規則，UserId: {UserId}，UserName: {UserName}", user.Id, user.UserName);
        
        try
        {
            var passwordValidators = _userManager.PasswordValidators;
            _logger.LogDebug("取得 {ValidatorCount} 個密碼驗證器", passwordValidators.Count());

            var allErrors = new List<string>();
            var validatorIndex = 0;

            foreach (var validator in passwordValidators)
            {
                validatorIndex++;
                _logger.LogDebug("執行第 {ValidatorIndex} 個密碼驗證器：{ValidatorType}，UserId: {UserId}", 
                    validatorIndex, validator.GetType().Name, user.Id);
                
                var validatorResult = await validator.ValidateAsync(_userManager, user, newPassword);
                
                if (!validatorResult.Succeeded)
                {
                    var errors = validatorResult.Errors.Select(e => e.Description).ToList();
                    allErrors.AddRange(errors);
                    
                    _logger.LogWarning("第 {ValidatorIndex} 個密碼驗證器驗證失敗，錯誤數量: {ErrorCount}，UserId: {UserId}，錯誤內容: {Errors}", 
                        validatorIndex, errors.Count, user.Id, string.Join("; ", errors));
                }
                else
                {
                    _logger.LogDebug("第 {ValidatorIndex} 個密碼驗證器驗證通過，UserId: {UserId}", validatorIndex, user.Id);
                }
            }

            var isValid = !allErrors.Any();
            
            if (isValid)
            {
                _logger.LogInformation("所有密碼規則驗證通過，共 {ValidatorCount} 個驗證器，UserId: {UserId}", 
                    validatorIndex, user.Id);
            }
            else
            {
                _logger.LogWarning("密碼規則驗證失敗，總錯誤數量: {ErrorCount}，UserId: {UserId}，所有錯誤: {AllErrors}", 
                    allErrors.Count, user.Id, string.Join("; ", allErrors));
            }
            
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "驗證密碼規則時發生錯誤，為安全考量拒絕密碼，UserId: {UserId}，錯誤訊息: {ErrorMessage}", 
                user.Id, ex.Message);
            return false; // 發生錯誤時，為安全考量，假設密碼不符合規則
        }
    }
    /// <summary>
    /// Invalid password rules response
    /// </summary>
    /// <returns></returns>
    private static OperationResult InvalidPasswordRulesResponse()
    {
        return new OperationResult(false, "新密碼不符合規則", StatusCodes.Status400BadRequest);
    }
    /// <summary>
    /// Change password
    /// </summary>
    /// <param name="user"></param>
    /// <param name="currentPassword"></param>
    /// <param name="newPassword"></param>
    /// <returns></returns>
    private async Task<bool> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword)
    {
        _logger.LogDebug("開始執行密碼變更，UserId: {UserId}，UserName: {UserName}", user.Id, user.UserName);
        
        try
        {
            var changePasswordStartTime = System.Diagnostics.Stopwatch.StartNew();
            
            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            
            changePasswordStartTime.Stop();
            
            if (result.Succeeded)
            {
                _logger.LogInformation("密碼變更成功，UserId: {UserId}，UserName: {UserName}，執行時間: {ElapsedMs}ms", 
                    user.Id, user.UserName, changePasswordStartTime.ElapsedMilliseconds);
                return true;
            }
            else
            {
                var errors = result.Errors.Select(e => $"{e.Code}: {e.Description}").ToList();
                _logger.LogError("密碼變更失敗，UserId: {UserId}，UserName: {UserName}，錯誤數量: {ErrorCount}，執行時間: {ElapsedMs}ms，錯誤詳情: {Errors}", 
                    user.Id, user.UserName, errors.Count, changePasswordStartTime.ElapsedMilliseconds, string.Join("; ", errors));
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "密碼變更過程發生例外錯誤，UserId: {UserId}，UserName: {UserName}，錯誤類型: {ExceptionType}，錯誤訊息: {ErrorMessage}", 
                user.Id, user.UserName, ex.GetType().Name, ex.Message);
            return false;
        }
    }
    /// <summary>
    /// Password change failed response
    /// </summary>
    /// <returns></returns>
    private static OperationResult PasswordChangeFailedResponse()
    {
        return new OperationResult(false, "修改密碼失敗", StatusCodes.Status400BadRequest);
    }
    /// <summary>
    /// Save password history
    /// </summary>
    /// <param name="user"></param>
    /// <param name="newPassword"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<bool> SavePasswordHistoryAsync(ApplicationUser user, string newPassword, CancellationToken cancellationToken)
    {
        _logger.LogDebug("開始儲存密碼歷史記錄，UserId: {UserId}，UserName: {UserName}", user.Id, user.UserName);
        
        try
        {
            var saveHistoryStartTime = System.Diagnostics.Stopwatch.StartNew();
            
            // 生成安全鹽值
            _logger.LogDebug("生成密碼鹽值，UserId: {UserId}", user.Id);
            var salt = _saltGenerator.GenerateSecureSalt();
            
            // 加密密碼
            _logger.LogDebug("開始加密密碼，UserId: {UserId}", user.Id);
            var hashedPassword = _userManager.PasswordHasher.HashPassword(user, newPassword + salt);
            
            // 保護鹽值
            _logger.LogDebug("開始保護鹽值，UserId: {UserId}", user.Id);
            var protectedSalt = _dataProtectionService.Protect(salt);
            
            // 建立密碼歷史記錄
            var newPasswordHistory = new PasswordHistory
            {
                UserId = user.Id,
                HashedPassword = hashedPassword,
                PasswordSalt = protectedSalt,
                PasswordChangeDate = DateTime.Now
            };
            
            _logger.LogDebug("密碼歷史記錄物件建立完成，準備寫入資料庫，UserId: {UserId}，變更時間: {ChangeDate}", 
                user.Id, newPasswordHistory.PasswordChangeDate);
            
            // 加入到資料庫
            await _unitOfWork.PasswordHistories.AddAsync(newPasswordHistory, cancellationToken);
            
            saveHistoryStartTime.Stop();
            
            _logger.LogInformation("密碼歷史記錄儲存成功，UserId: {UserId}，UserName: {UserName}，執行時間: {ElapsedMs}ms", 
                user.Id, user.UserName, saveHistoryStartTime.ElapsedMilliseconds);
            
            // 記錄目前總共有多少筆密碼歷史
            try
            {
                var totalHistoryCount = await _unitOfWork.PasswordHistories.GetCountByUserIdAsync(user.Id, cancellationToken);
                
                _logger.LogDebug("使用者目前共有 {TotalHistoryCount} 筆密碼歷史記錄，UserId: {UserId}", 
                    totalHistoryCount, user.Id);
            }
            catch (Exception countEx)
            {
                _logger.LogWarning(countEx, "計算密碼歷史記錄總數時發生錯誤，但不影響主要流程，UserId: {UserId}", user.Id);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "儲存密碼歷史記錄時發生例外錯誤，UserId: {UserId}，UserName: {UserName}，錯誤類型: {ExceptionType}，錯誤訊息: {ErrorMessage}", 
                user.Id, user.UserName, ex.GetType().Name, ex.Message);
            return false;
        }
    }
    /// <summary>
    /// Password history save failed response
    /// </summary>
    /// <returns></returns>
    private static OperationResult PasswordHistorySaveFailedResponse()
    {
        return new OperationResult(false, "密碼歷程紀錄失敗", StatusCodes.Status500InternalServerError);
    }
    /// <summary>
    /// Handle User Name not Equals New Password
    /// </summary>
    /// <param name="user"></param>
    /// <param name="newPassword"></param>
    /// <returns></returns>
    private static bool IsUserNameEqualsNewPassword(ApplicationUser user, string newPassword)
    {
        if (user.UserName == null) return true;
        return user.UserName.Equals(newPassword, StringComparison.CurrentCultureIgnoreCase);
    }
    /// <summary>
    /// User name equals new password response
    /// </summary>
    /// <returns></returns>
    private static OperationResult UserNameEqualsNewPassword()
    {
        return new OperationResult(false, "密碼不可以與使用者帳號一致", StatusCodes.Status404NotFound);
    }
    /// <summary>
    /// Handle create account command
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<SkyLabDocUserDetailResponse> HandleCreateAccoutCommandAsync(CreateAccoutCommand request, CancellationToken cancellationToken)
    {
        var entity = _mapper.RequestToSkyLabDocUserDetail(request.skylabDocUserDetailRequest);
        entity.CreateBy = entity.ApplicationUser.Id;
        entity.CreateDatetime = DateTime.Now;
        entity.LastUpdatedBy = entity.ApplicationUser.Id;
        entity.LastUpdateDatetime = DateTime.Now;
        entity.ApplicationUser.Email = request.skylabDocUserDetailRequest.OfficialEmail;
        entity.ApplicationUser.UserName = request.skylabDocUserDetailRequest.UserName;
        await _unitOfWork.SkyLabDocUserDetails.AddAsync(entity, cancellationToken);
        var response = _mapper.SkyLabDocUserDetailToResponse(entity);
        response.FileId = _dataProtectionService.Protect(response.FileId);
        response.UserId = _dataProtectionService.Protect(response.UserId);
        response.SystemRole = _dataProtectionService.Protect(response.SystemRole);
        response.operationResult = new OperationResult(true, "新增成功", 200);
        return response;
    }
    /// <summary>
    /// Handle put account command
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<AcctMaintainQueryVM> HandlePutAccountCommandAsync(PutAccountCommad request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var loginUserInfo = await GetLoginUserInfoAsync(request.LoginUserId, cancellationToken);
            if (!loginUserInfo.IsUserEligible)
            {
                return new AcctMaintainQueryVM
                {
                    OperationResult = new OperationResult(false, UserNotExistsMessage, StatusCodes.Status404NotFound)
                };
            }

            var decryptedUserId = _dataProtectionService.Unprotect(request.SkyLabDocUserDetailDto.UserId);
            var user = await _userManager.FindByIdAsync(decryptedUserId);
            if (user == null)
            {
                return new AcctMaintainQueryVM
                {
                    OperationResult = new OperationResult(false, UserNotExistsMessage, StatusCodes.Status404NotFound)
                };
            }
           
            var skylabDocUserDetail = await _unitOfWork.SkyLabDocUserDetails.GetByUserIdAsync(decryptedUserId, cancellationToken);
            if (skylabDocUserDetail == null)
            {
                _logger.LogWarning("SkyLabDocUserDetail not found.");
                return new AcctMaintainQueryVM
                {
                    OperationResult = new OperationResult(false, UserNotExistsMessage, StatusCodes.Status404NotFound)
                };
            }
           
            // 更新使用者詳細資料
            UpdateUserDetails(skylabDocUserDetail, request, loginUserInfo.UserId);
            await _unitOfWork.SkyLabDocUserDetails.UpdateAsync(skylabDocUserDetail, cancellationToken);

            // 更新使用者權限
            await UpdateUserClaims(user, request.AcctMaintainFunctionPermissionRequest.FunctionGroups);
            await _unitOfWork.CommitAsync(cancellationToken);
           
            // 檢查是否需要發送退回審核的郵件通知
            if (!request.SkyLabDocUserDetailDto.IsApproved && !string.IsNullOrEmpty(request.SkyLabDocUserDetailDto.ReasonsForDisapproval))
            {
                await SendRejectionEmail(skylabDocUserDetail, request.SkyLabDocUserDetailDto.ReasonsForDisapproval);
            }
            await ReMoveLoginUserInfoByCacheAsync(user, cancellationToken);
            return new AcctMaintainQueryVM { OperationResult = new OperationResult(true, "更新成功", StatusCodes.Status200OK) };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Update failed.");
            return new AcctMaintainQueryVM { OperationResult = new OperationResult(false, "使用者資料更新失敗", StatusCodes.Status500InternalServerError) };
        }
    }
    /// <summary>
    /// Update user details
    /// </summary>
    /// <param name="skylabDocUserDetail"></param>
    /// <param name="request"></param>
    /// <param name="loginUserId"></param>

    private static void UpdateUserDetails(SkyLabDocUserDetail skylabDocUserDetail, PutAccountCommad request, string loginUserId)
    {
        skylabDocUserDetail.FullName = request.SkyLabDocUserDetailDto.FullName;
        skylabDocUserDetail.JobTitle = request.SkyLabDocUserDetailDto.JobTitle;
        skylabDocUserDetail.BranchCode = request.SkyLabDocUserDetailDto.BranchCode;
        skylabDocUserDetail.OfficialEmail = request.SkyLabDocUserDetailDto.OfficialEmail;
        skylabDocUserDetail.OfficialPhone = request.SkyLabDocUserDetailDto.OfficialPhone;
        skylabDocUserDetail.SubordinateUnit = request.SkyLabDocUserDetailDto.SubordinateUnit;
        skylabDocUserDetail.LastUpdatedBy = loginUserId;
        skylabDocUserDetail.LastUpdateDatetime = DateTime.Now;
        skylabDocUserDetail.ApplicationUser.Email = request.SkyLabDocUserDetailDto.OfficialEmail;
        skylabDocUserDetail.ApplicationUser.IsActive = request.SkyLabDocUserDetailDto.IsActive;
        skylabDocUserDetail.ApplicationUser.IsApproved = request.SkyLabDocUserDetailDto.IsApproved;
        skylabDocUserDetail.MoicaCardNumber = request.SkyLabDocUserDetailDto.MoicaCardNumber;
        skylabDocUserDetail.ApplicationUser.LockoutEnabled = request.SkyLabDocUserDetailDto.LockoutEnabled;
        if (!request.SkyLabDocUserDetailDto.LockoutEnabled)
        {
            skylabDocUserDetail.ApplicationUser.LockoutEnd = null;
            skylabDocUserDetail.ApplicationUser.AccessFailedCount = 0;
        }

    }
    /// <summary>
    /// Update user claims
    /// </summary>
    /// <param name="user"></param>
    /// <param name="functionGroups"></param>
    /// <returns></returns>

    protected virtual async Task UpdateUserClaims(ApplicationUser user, List<FunctionGroupDto> functionGroups)
    {
        foreach (var group in functionGroups)
        {
            foreach (var function in group.Functions)
            {
                var permissionValue = GetPermissionValue(function.Permissions);
                var claimType = $"{function.FunctionID}.Permissions";
                var claimValue = permissionValue.ToString();

                var claim = new Claim(claimType, claimValue);
                await AddOrRemoveClaimAsync(user, claim);
            }
        }
    }
    /// <summary>
    /// Get permission value
    /// </summary>
    /// <param name="permissionSet"></param>
    /// <returns></returns>
    private static int GetPermissionValue(object permissionSet)
    {
        var permissionProps = permissionSet.GetType().GetProperties();
        int permissionValue = 0;

        foreach (var prop in permissionProps)
        {
            var isPermissionTrue = prop.GetValue(permissionSet) as string == "True";
            var permissionBitValue = GetPermissionBitValue(prop.Name);

            if (isPermissionTrue)
            {
                permissionValue |= permissionBitValue;
            }
        }

        return permissionValue;
    }
    /// <summary>
    /// Add or remove claim
    /// </summary>
    /// <param name="user"></param>
    /// <param name="claim"></param>
    /// <returns></returns>
    private async Task AddOrRemoveClaimAsync(ApplicationUser user, Claim claim)
    {
        var userClaims = await _userManager.GetClaimsAsync(user);
        var existingClaim = userClaims.FirstOrDefault(c => c.Type == claim.Type);

        if (existingClaim == null)
        {
            await _userManager.AddClaimAsync(user, claim); // 新增 claim
        }
        else if (existingClaim.Value != claim.Value)
        {
            await _userManager.RemoveClaimAsync(user, existingClaim);
            await _userManager.AddClaimAsync(user, claim); // 更新 claim 值
        }
    }
    /// <summary>
    /// Get permission bit value
    /// </summary>
    /// <param name="permissionName"></param>
    /// <returns></returns>
    private static int GetPermissionBitValue(string permissionName)
    {
        return permissionName switch
        {
            "Read" => (int)Permissions.Read,
            "Search" => (int)Permissions.Search,
            "Create" => (int)Permissions.Create,
            "Update" => (int)Permissions.Update,
            "Delete" => (int)Permissions.Delete,
            "Upload" => (int)Permissions.Upload,
            "Download" => (int)Permissions.Download,
            "Import" => (int)Permissions.Import,
            "Export" => (int)Permissions.Export,
            _ => 0
        };
    }
    /// <summary>
    /// Send rejection email
    /// </summary>
    /// <param name="skylabDocUserDetail"></param>
    /// <param name="reasonsForDisapproval"></param>
    /// <returns></returns>
    protected virtual async Task SendRejectionEmail(SkyLabDocUserDetail skylabDocUserDetail, string reasonsForDisapproval)
    {
        var emailDto = new EmailDto
        {
            To = new List<string> { "skyhsieh@skylab.com.tw",  "candy72@skylab.com.tw", "welcome09210801@skylab.com.tw" },
            From = _configuration["MailSettings:EmailFrom"] ?? string.Empty,
            Subject = "[SkyLab查詢系統]註冊新帳號—審核已退回",
            Body = $"您好，於「SkyLab查詢系統」註冊的新帳號審核已退回，退回原因: {reasonsForDisapproval}，請至「SkyLab查詢系統」登入您申請的帳號及密碼，重新送出申請帳號資料，謝謝。此為系統自動發信，請勿回覆，謝謝您的配合。"
        };

        await _emailService.SendAsync(emailDto);
    }

    /// <summary>
    /// 觸發登入通知郵件（非阻塞）
    /// </summary>
    /// <param name="userName">使用者名稱</param>
    /// <param name="officialEmail">使用者官方郵件（生產環境收件人）</param>
    /// <param name="ipAddress">客戶端 IP 位址</param>
    /// <param name="isSuccess">登入是否成功</param>
    /// <param name="failureReason">失敗原因（可選）</param>
    protected virtual void TriggerLoginNotification(string userName, string? officialEmail, string ipAddress, bool isSuccess, string? failureReason = null)
    {
        // 使用 Task.Run 確保非阻塞處理
        _ = Task.Run(async () =>
        {
            try
            {
                await _loginNotificationService.SendLoginNotificationAsync(
                    tenantId: _currentTenantId,
                    userName: userName,
                    officialEmail: officialEmail,
                    isSuccess: isSuccess,
                    failureReason: failureReason,
                    ipAddress: ipAddress);
            }
            catch (Exception ex)
            {
                // 錯誤隔離：記錄錯誤但不拋出異常，避免影響登入流程
                _logger.LogError(ex, "觸發登入通知時發生未預期的錯誤。TenantId: {TenantId}, UserName: {UserName}, Success: {IsSuccess}", 
                    _currentTenantId, userName, isSuccess);
            }
        });
    }

    // NOTE: 租戶已透過多租戶架構處理（ITenantUserServiceFactory）
    /// <summary>
    /// Handle account query
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<AccountQueryVM> HandleAccountQueryAsync(AccountQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // 解密 UserId（在 service 層處理，repository 接收已解密的值）
            if (!string.IsNullOrEmpty(request.UserId))
            {
                request.UserId = _dataProtectionService.Unprotect(request.UserId);
            }

            // Status filter: 先用 UserManager 查符合狀態條件的 userIds
            IEnumerable<string>? statusFilteredUserIds = null;
            if (request.Status != 0)
            {
                var usersQuery = _userManager.Users.AsQueryable();
                usersQuery = request.Status switch
                {
                    (int)UserInfo.StatusUnApprove => usersQuery.Where(u => !u.IsApproved),
                    (int)UserInfo.StatusIsActive => usersQuery.Where(u => u.IsActive),
                    (int)UserInfo.StatusLockoutEnabled => usersQuery.Where(u => u.LockoutEnabled),
                    (int)UserInfo.StatusUnActive => usersQuery.Where(u => !u.IsActive && u.IsApproved),
                    (int)UserInfo.StatusIsApproved => usersQuery.Where(u => u.IsApproved),
                    _ => usersQuery
                };
                statusFilteredUserIds = usersQuery.Select(u => u.Id).ToList();
            }

            var (items, totalCount) = await _unitOfWork.SkyLabDocUserDetails
                .GetAccountQueryAsync(request, statusFilteredUserIds, cancellationToken);
            var itemList = items.ToList();

            // 若無記錄
            if (totalCount == 0)
            {
                return new AccountQueryVM
                {
                    SkyLabDocUserDetailDtos = new List<SkyLabDocUserDetailDto>(),
                    TotalRecords = 0,
                    CurrentPage = 0,
                    PageSize = 0,
                    operationResult = new OperationResult(false, "查無資料", 404)
                };
            }

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            // 頁碼超出範圍
            if (request.PageNumber > totalPages)
            {
                return new AccountQueryVM
                {
                    SkyLabDocUserDetailDtos = new List<SkyLabDocUserDetailDto>(),
                    TotalRecords = totalCount,
                    CurrentPage = request.PageNumber,
                    PageSize = request.PageSize,
                    operationResult = new OperationResult(false, $"頁碼不正確，大於總頁數:{totalPages}", 500)
                };
            }

            // 批次查詢 Branch、FileUpload、User 狀態資料
            var branchCodes = itemList.Select(i => i.BranchCode).Where(c => !string.IsNullOrEmpty(c)).Distinct();
            var fileIds = itemList.Select(i => i.FileId).Where(f => !string.IsNullOrEmpty(f)).Distinct();
            var userIds = itemList.Select(i => i.UserId).Distinct().ToList();

            var branches = (await _unitOfWork.Branches.GetByCodesAsync(branchCodes, cancellationToken))
                .ToDictionary(b => b.BranchCode);
            var files = (await _unitOfWork.FileUploads.GetByIdsAsync(fileIds, cancellationToken))
                .ToDictionary(f => f.FileId);
            var appUsers = _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionary(u => u.Id);

            // LINQ 組合最終 DTO + 加密敏感欄位
            foreach (var dto in itemList)
            {
                if (branches.TryGetValue(dto.BranchCode, out var branch))
                    dto.BranchName = branch.BranchName;

                if (files.TryGetValue(dto.FileId, out var file))
                {
                    dto.OriginalFileName = file.OriginalFileName;
                    dto.FileExtension = file.FileExtension;
                }

                if (appUsers.TryGetValue(dto.UserId, out var appUser))
                {
                    dto.IsApproved = appUser.IsApproved;
                    dto.IsActive = appUser.IsActive;
                    dto.LockoutEnabled = appUser.LockoutEnabled;
                }

                dto.FileId = _dataProtectionService.Protect(dto.FileId);
                dto.UserId = _dataProtectionService.Protect(dto.UserId);
                dto.SystemRole = _dataProtectionService.Protect(dto.SystemRole);
            }

            return new AccountQueryVM
            {
                SkyLabDocUserDetailDtos = itemList,
                TotalRecords = totalCount,
                CurrentPage = request.PageNumber,
                PageSize = request.PageSize,
                operationResult = new OperationResult(true, "查詢成功", 200)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AccountQueryHandler");
            return new AccountQueryVM
            {
                SkyLabDocUserDetailDtos = new List<SkyLabDocUserDetailDto>(),
                TotalRecords = 0,
                CurrentPage = 0,
                PageSize = 0,
                operationResult = new OperationResult(false, "查詢失敗", 500)
            };
        }
    }
    // NOTE: 租戶已透過多租戶架構處理（ITenantUserServiceFactory）
    /// <summary>
    /// Handle account maintain query
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<AcctMaintainQueryVM> HandleAcctMaintainQueryAsync(AcctMaintainQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var loginUserInfo = await GetLoginUserInfoAsync(request.LoginUserId, cancellationToken);
            if (!loginUserInfo.IsUserEligible)
            {
                return new AcctMaintainQueryVM
                {
                    OperationResult = new OperationResult(false, "操作使用者不存在或無權限", StatusCodes.Status404NotFound)
                };
            }
            var user = await _userManager.FindByIdAsync(_dataProtectionService.Unprotect(request.UserId));

            if (user == null)
            {
                return new AcctMaintainQueryVM
                {
                    OperationResult = new OperationResult(false, UserNotExistsMessage, StatusCodes.Status404NotFound)
                };
            }

            var skylabDocUserDetail = await GetSkyLabDocUserDetailAsync(request);
            var acctMaintainFunctionPermissionResponse = await GetAcctMaintainFunctionPermissionResponse(request, user, cancellationToken);

            return new AcctMaintainQueryVM
            {
                SkyLabDocUserDetailDto = skylabDocUserDetail,
                AcctMaintainFunctionPermissionResponse = acctMaintainFunctionPermissionResponse,
                OperationResult = new OperationResult(true, "取出資料", StatusCodes.Status200OK)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AcctMaintainQueryHandler failed.");
            return new AcctMaintainQueryVM
            {
                OperationResult = new OperationResult(false, "取出資料失敗", StatusCodes.Status500InternalServerError)
            };
        }
    }


    /// <summary>
    /// Get user details
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task<SkyLabDocUserDetailDto> GetSkyLabDocUserDetailAsync(AcctMaintainQuery request)
    {
        try
        {
            var result = await HandleAccountQueryAsync(new AccountQuery { UserId = request.UserId }, CancellationToken.None);
            var userDetail = result.SkyLabDocUserDetailDtos.FirstOrDefault();
            return userDetail ?? new SkyLabDocUserDetailDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user details.");
            return new SkyLabDocUserDetailDto();
        }
    }
    /// <summary>
    /// Get account maintain function permission response
    /// </summary>
    /// <param name="request"></param>
    /// <param name="user"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task<AcctMaintainFunctionPermissionResponse> GetAcctMaintainFunctionPermissionResponse(
        AcctMaintainQuery request,
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        try
        {
            var functionGroupDtos = await GetFunctionGroupsAsync(cancellationToken);
            var claimsDictionary = await GetUserClaimsDictionaryAsync(user);
            ProcessFunctionPermissions(functionGroupDtos, claimsDictionary);

            return new AcctMaintainFunctionPermissionResponse
            {
                UserId = _dataProtectionService.Protect(request.UserId),
                FunctionGroups = functionGroupDtos,
                OperationResult = new OperationResult(true, "取出資料", StatusCodes.Status200OK)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get function permissions.");
            return new AcctMaintainFunctionPermissionResponse
            {
                OperationResult = new OperationResult(false, "取出資料失敗", StatusCodes.Status500InternalServerError)
            };
        }
    }
    /// <summary>
    /// Get function groups
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task<List<FunctionGroupDto>> GetFunctionGroupsAsync(CancellationToken cancellationToken)
    {
        var groups = (await _unitOfWork.FunctionGroups.GetAllAsync(cancellationToken)).ToList();
        var groupIds = groups.Select(g => g.GroupID);
        var functions = (await _unitOfWork.Functions.GetByGroupIdsAsync(groupIds, cancellationToken))
            .GroupBy(f => f.GroupID)
            .ToDictionary(g => g.Key, g => g.ToList());

        return groups.Select(fg => new FunctionGroupDto
        {
            GroupID = fg.GroupID,
            GroupEnglishDescription = fg.GroupEnglishDescription,
            GroupChineseDescription = fg.GroupChineseDescription,
            GroupOrder = fg.GroupOrder,
            Functions = functions.TryGetValue(fg.GroupID, out var fns)
                ? fns.Select(f => new FunctionDto
                {
                    GroupID = f.GroupID,
                    FunctionID = f.FunctionID,
                    FunctionEnglishDescription = f.FunctionEnglishDescription,
                    FunctionChineseDescription = f.FunctionChineseDescription,
                    FunctionOrder = f.FunctionOrder
                }).ToList()
                : []
        }).ToList();
    }
    /// <summary>
    /// Get user claims dictionary
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    protected virtual async Task<Dictionary<string, string>> GetUserClaimsDictionaryAsync(ApplicationUser user)
    {
        var claims = await _userManager.GetClaimsAsync(user);
        return claims.ToDictionary(c => c.Type, c => c.Value);
    }
    /// <summary>
    /// Process function permissions
    /// </summary>
    /// <param name="functionGroupDtos"></param>
    /// <param name="claimsDictionary"></param>
    protected virtual void ProcessFunctionPermissions(List<FunctionGroupDto> functionGroupDtos, Dictionary<string, string> claimsDictionary)
    {
        foreach (var group in functionGroupDtos)
        {
            foreach (var function in group.Functions)
            {
                function.Permissions = ExtractPermissionsFromClaim(function.FunctionID, claimsDictionary);
            }
        }
    }
    /// <summary>
    /// Extract permissions from claim
    /// </summary>
    /// <param name="functionID"></param>
    /// <param name="claimsDictionary"></param>
    /// <returns></returns>
    protected virtual PermissionSet ExtractPermissionsFromClaim(string functionID, Dictionary<string, string> claimsDictionary)
    {
        var claimKey = $"{functionID}.Permissions";
        if (claimsDictionary.TryGetValue(claimKey, out var claimValue) && int.TryParse(claimValue, out int permissionsValue))
        {
            return GeneratePermissionSet(permissionsValue);
        }
        return GeneratePermissionSet(0);
    }
    /// <summary>
    /// Generate permission set
    /// </summary>
    /// <param name="permissionsValue"></param>
    /// <returns></returns>
    private static PermissionSet GeneratePermissionSet(int permissionsValue)
    {
        return new PermissionSet
        {
            Read = HasPermission(permissionsValue, Permissions.Read),
            Search = HasPermission(permissionsValue, Permissions.Search),
            Create = HasPermission(permissionsValue, Permissions.Create),
            Update = HasPermission(permissionsValue, Permissions.Update),
            Delete = HasPermission(permissionsValue, Permissions.Delete),
            Upload = HasPermission(permissionsValue, Permissions.Upload),
            Download = HasPermission(permissionsValue, Permissions.Download),
            Import = HasPermission(permissionsValue, Permissions.Import),
            Export = HasPermission(permissionsValue, Permissions.Export)
        };
    }

    /// <summary>
    /// Has permission
    /// </summary>
    /// <param name="permissionsValue"></param>
    /// <param name="permission"></param>
    /// <returns></returns>
    private static string HasPermission(int permissionsValue, Permissions permission)
    {
        return (permissionsValue & (int)permission) == (int)permission ? "True" : "False";
    }

    public abstract Task CreateExternalUserDetailAsync(string userId, string username, string fullName, string email, string tenantId, CancellationToken cancellationToken);

    /// <summary>
    /// 更新外部用戶註冊資料
    /// </summary>
    public virtual async Task<OperationResult> UpdateExternalUserRegistrationAsync(
        string encryptedUserId,
        ExternalUserRegistrationDto userDetails,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("開始完成外部用戶註冊流程，加密用戶ID: {EncryptedUserId}", encryptedUserId);

            var decryptedUserId = _dataProtectionService.Unprotect(encryptedUserId);
            _logger.LogDebug("已解密用戶ID: {UserId}", decryptedUserId);

            // 尋找用戶
            var user = await _userManager.FindByIdAsync(decryptedUserId);
            if (user == null)
            {
                _logger.LogWarning("完成註冊失敗: 用戶不存在，用戶ID: {UserId}", decryptedUserId);
                // 🔒 統一錯誤訊息，避免洩露帳號存在性 (API8:2023)
                return new OperationResult(false, "操作失敗，請確認您的註冊資訊", StatusCodes.Status400BadRequest);
            }

            _logger.LogDebug("找到用戶: {UserName}, Email: {Email}, 外部提供者: {Provider}",
                user.UserName, user.Email, user.ExternalProvider);

            // 獲取用戶詳情 (由子類實現的租戶特定邏輯)
            var userDetail = await GetUserDetailByTenantAsync(decryptedUserId, cancellationToken);
            if (userDetail == null)
            {
                _logger.LogWarning("完成註冊失敗: 用戶詳情不存在，用戶ID: {UserId}", decryptedUserId);
                // 🔒 統一錯誤訊息，避免洩露系統內部狀態 (API8:2023)
                return new OperationResult(false, "操作失敗，請確認您的註冊資訊", StatusCodes.Status400BadRequest);
            }

            // 更新用戶詳情 (由子類實現的租戶特定邏輯)
            await UpdateUserDetailByTenantAsync(userDetail, user, userDetails, cancellationToken);

            // 標記用戶已完成註冊
            user.HasCompletedRegistration = true;

            // 儲存變更
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                _logger.LogError("更新用戶資料失敗: {Errors}", errors);
                return new OperationResult(false, "更新用戶資料失敗", StatusCodes.Status500InternalServerError);
            }

            _logger.LogInformation("用戶 {UserId} 已完成外部登入註冊流程", user.Id);
            return new OperationResult(true, "資料補充完成", StatusCodes.Status200OK);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "完成註冊流程時出錯: {EncryptedUserId}", encryptedUserId);
            return new OperationResult(false, "完成註冊流程時出錯", StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// 根據租戶獲取用戶詳情
    /// </summary>
    protected abstract Task<object> GetUserDetailByTenantAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// 根據租戶更新用戶詳情
    /// </summary>
    protected abstract Task UpdateUserDetailByTenantAsync(
        object userDetail,
        ApplicationUser user,
        ExternalUserRegistrationDto userDetails,
        CancellationToken cancellationToken);
}

