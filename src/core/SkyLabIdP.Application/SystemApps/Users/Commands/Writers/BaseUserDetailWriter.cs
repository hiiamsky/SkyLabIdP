using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos;
using SkyLabIdP.Application.Dtos.User.Registration;
using SkyLabIdP.Application.SystemApps.Services.ServiceSettings;
using SkyLabIdP.Domain.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Application.SystemApps.Users.Commands.Writers;

/// <summary>
/// 泛型基礎類別，處理使用者詳細資訊寫入的共同邏輯
/// </summary>
/// <typeparam name="TRequest">請求類型</typeparam>
/// <typeparam name="TResponse">回應類型</typeparam>
/// <typeparam name="TUserDetail">使用者詳細資訊的實體類型</typeparam>
public abstract class BaseUserDetailWriter<TRequest, TResponse, TUserDetail> : IUserDetailWriter<TRequest, TResponse> 
    where TRequest : BaseUserRegistrationRequest
    where TResponse : BaseUserRegistrationResponse, new()
    where TUserDetail : class
{
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly UserManager<ApplicationUser> _userManager;
    protected readonly IDataProtectionService _dataProtectionService;
    protected readonly ILogger _logger;

    protected BaseUserDetailWriter(
        LoginUserInfoServiceSettings loginUserInfoServiceSettings,
        ILogger logger)
    {
        _unitOfWork = loginUserInfoServiceSettings.UnitOfWork;
        _userManager = loginUserInfoServiceSettings.UserManager;
        _dataProtectionService = loginUserInfoServiceSettings.Dataprotectionservice;
        _logger = logger;
    }

    public virtual async Task<TResponse> WriteAsync(TRequest request, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        IdentityResult userCreationResult = IdentityResult.Success;
        ApplicationUser user;

        if (existingUser != null)
        {
            user = existingUser;
        }
        else
        {
            user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                IsActive = true,
                IsApproved = false,
                IsExternalAccount = false,
                LockoutEnd = null,
                AccessFailedCount = 0,
                LockoutEnabled = false
            };
            _logger.LogInformation("Creating new user with UserName: {UserName}, Email: {Email}, LockoutEnabled: {LockoutEnabled}", user.UserName, user.Email, user.LockoutEnabled);
            userCreationResult = await _userManager.CreateAsync(user, request.Password);
            _logger.LogInformation("User creation result: {Result}", userCreationResult.Succeeded ? "Success" : "Failed");
            
            if (userCreationResult.Succeeded)
            {
                // Identity 會自動將 LockoutEnabled 設為 true，手動重設為 false
                user.LockoutEnabled = false;
                await _userManager.UpdateAsync(user);
                _logger.LogInformation("Updated user LockoutEnabled to false for UserName: {UserName}", user.UserName);
            }
            if (!userCreationResult.Succeeded)
            {
                _logger.LogError("帳號建立失敗: {Errors}", string.Join(", ", userCreationResult.Errors.Select(e => e.Description)));
                return new TResponse
                {
                    UserName = request.UserName,
                    Email = request.Email,
                    operationResult = new OperationResult(false, "帳號建立失敗", 400)
                };
            }
        }

        // 檢查該帳號是否已註冊在此租戶中
        var existsInTenant = await _unitOfWork.UserTenants
            .ExistsAsync(user.Id, request.TenantId, cancellationToken);

        if (existsInTenant)
        {
            _logger.LogError("帳號已存在於租戶中: {UserId}, {TenantId}", user.Id, request.TenantId);
            return new TResponse
            {
                UserName = request.UserName,
                Email = request.Email,
                operationResult = new OperationResult(false, "帳號已存在於租戶中", 400)
            };
        }

        // 如果不存在，則新增 UserTenant 記錄
        var tenantGuid = Guid.NewGuid().ToString();
        var userTenant = new UserTenant
        {
            TenantGuid = tenantGuid,
            UserId = user.Id,
            TenantId = request.TenantId,
            CreateDateTime = DateTime.UtcNow,
        };

        await _unitOfWork.UserTenants.AddAsync(userTenant, cancellationToken);

        // 創建使用者詳細資訊
        var userDetail = CreateUserDetail(request, user, tenantGuid);
        await AddUserDetailToContextAsync(userDetail, cancellationToken);

        var response = new TResponse
        {
            UserId = _dataProtectionService.Protect(user.Id),
            UserName = request.UserName,
            Email = request.Email,
            operationResult = new OperationResult(true, "新增成功", 200)
        };

        return response;
    }

   /// <summary>
   /// 創建使用者詳細資訊的實體
   /// </summary>
   /// <param name="request"></param>
   /// <param name="user"></param>
   /// <param name="tenantGuid"></param>
   /// <returns></returns>
    protected abstract TUserDetail CreateUserDetail(TRequest request, ApplicationUser user, string tenantGuid);

    /// <summary>
    /// 新增使用者詳細資訊到資料庫上下文
    /// </summary>
    /// <param name="userDetail">使用者詳細資訊</param>
    /// <param name="cancellationToken">取消權杖</param>
    protected abstract Task AddUserDetailToContextAsync(TUserDetail userDetail, CancellationToken cancellationToken);
}