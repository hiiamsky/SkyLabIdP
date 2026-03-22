using System;
using SkyLabIdP.Application.Common.Exceptions;
using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos.Email;
using SkyLabIdP.Application.Dtos.User.Registration;
using SkyLabIdP.Application.SystemApps.Services;
using SkyLabIdP.Domain.Entities;
using Mediator;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Application.SystemApps.Users.Commands.RegisterUser;

public class RegisterSkyLabDevelopUserCommandHandler(
    IUnitOfWork unitOfWork,
    UserManager<ApplicationUser> userManager,
    IEmailService emailService,
    IMediator mediator,
    IConfiguration configuration,
    IDataProtectionService dataprotectionservice,
    ISaltGenerator saltGenerator,
    ILogger<RegisterSkyLabDevelopUserCommandHandler> logger,
    IUserDetailWriterFactory userDetailWriterFactory,
    IDefaultPermissionServiceFactory defaultPermissionServiceFactory) : IRequestHandler<RegisterSkyLabDevelopUserCommand, SkyLabDevelopUserRegistrationResponse>
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IEmailService _emailService = emailService;
    private readonly IMediator _mediator = mediator;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ISaltGenerator _saltGenerator = saltGenerator;
    private readonly IDataProtectionService _dataprotectionservice = dataprotectionservice;
    private readonly ILogger<RegisterSkyLabDevelopUserCommandHandler> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly IUserDetailWriterFactory _userDetailWriterFactory = userDetailWriterFactory;
    private readonly IDefaultPermissionServiceFactory _defaultPermissionServiceFactory = defaultPermissionServiceFactory;

    public async ValueTask<SkyLabDevelopUserRegistrationResponse> Handle(RegisterSkyLabDevelopUserCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // 取得租戶 ID
            var tenantId = request.TenantId;

            // 使用 Factory 取得對應的 Writer
            var userDetailWriter = _userDetailWriterFactory.GetWriter<SkyLabDevelopUserRegistrationRequest, SkyLabDevelopUserRegistrationResponse>(tenantId);

            // 使用 Writer 處理註冊邏輯
            var userRegistrationResponse = await userDetailWriter.WriteAsync(request.SkyLabDevelopUserRegistrationRequest, cancellationToken);
            
            // 檢查註冊是否成功
            if (!userRegistrationResponse.operationResult.Success)
            {
                _logger.LogWarning("使用者註冊失敗: {Message}", string.Join(", ", userRegistrationResponse.operationResult.Messages));
                await _unitOfWork.RollbackAsync(cancellationToken);
                return userRegistrationResponse;
            }
            
            // 解密並查找使用者
            var user = await _userManager.FindByIdAsync(_dataprotectionservice.Unprotect(userRegistrationResponse.UserId)) 
                ?? throw new NotFoundException(nameof(ApplicationUser), userRegistrationResponse.UserId);
            
            // 設定預設權限
            var defaultPermissionService = _defaultPermissionServiceFactory.GetService(tenantId);
            await defaultPermissionService.SetDefaultPermissionsAsync(user.Id, tenantId, cancellationToken);

            await SavePasswordHistoryAsync(user, request.SkyLabDevelopUserRegistrationRequest.Password, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);

            await SendRegistrationEmailAsync(request);

            return CreateSuccessResponse(user);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Register User Failed");

            throw;
        }
    }


    private async Task SavePasswordHistoryAsync(ApplicationUser user, string newPassword, CancellationToken cancellationToken)
    {
        var salt = _saltGenerator.GenerateSecureSalt();
        var hashedPassword = _userManager.PasswordHasher.HashPassword(user, newPassword + salt);

        var passwordHistory = new PasswordHistory
        {
            UserId = user.Id,
            HashedPassword = hashedPassword,
            PasswordSalt = _dataprotectionservice.Protect(salt),
            PasswordChangeDate = DateTime.Now
        };

        await _unitOfWork.PasswordHistories.AddAsync(passwordHistory, cancellationToken);
    }

    private async Task SendRegistrationEmailAsync(RegisterSkyLabDevelopUserCommand request)
    {
        var emailDto = new EmailDto
        {
            To = [_configuration["AssigneeEmail"] ?? "skyhsieh@skylab.com.tw"],
            From = "",
            Subject = $"「SkyLab查詢系統」註冊新帳號—請協助審核({DateTime.Now.Year - 1911}-{DateTime.Now.Month}-{DateTime.Now.Day}) ",
            Body = $"SkyLab查詢系統有使用者註冊一個新帳號。<br />" +
            $"使用者名稱：{request.SkyLabDevelopUserRegistrationRequest.FullName}。<br />" +
            $"帳號：{request.SkyLabDevelopUserRegistrationRequest.UserName}。<br />" +
            $"請至SkyLab查詢系統＞帳號管理 協助審核，謝謝。此為系統自動發信，請勿回覆，謝謝您的配合。"
        };

        await _emailService.SendAsync(emailDto);
    }
    private SkyLabDevelopUserRegistrationResponse CreateSuccessResponse(ApplicationUser user)
    {
        return new SkyLabDevelopUserRegistrationResponse
        {
            UserId = _dataprotectionservice.Protect(user.Id),
            UserName = user.UserName ?? "",
            operationResult = new Dtos.OperationResult(true, "註冊成功，請確認Email")
        };
    }

    private SkyLabDevelopUserRegistrationResponse CreateFailureResponse()
    {
        return new SkyLabDevelopUserRegistrationResponse
        {
            operationResult = new Dtos.OperationResult(false, ["註冊失敗"], StatusCodes.Status400BadRequest)
        };
    }


}
