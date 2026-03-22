using SkyLabIdP.Application.Common.Exceptions;
using SkyLabIdP.Application.Dtos;

namespace SkyLabIdP.Application.Common.Extensions;

/// <summary>
/// ValidationException 的擴展方法，提供更好的錯誤處理和響應格式化
/// </summary>
public static class ValidationExceptionExtensions
{
    /// <summary>
    /// 將 ValidationException 轉換為 OperationResult
    /// </summary>
    /// <param name="exception">驗證例外</param>
    /// <returns>包含詳細驗證錯誤的 OperationResult</returns>
    public static OperationResult ToOperationResult(this ValidationException exception)
    {
        var errorMessages = new List<string>();
        
        foreach (var error in exception.Errors)
        {
            var fieldName = GetChineseFieldName(error.Key);
            var fieldErrors = error.Value.Select(msg => $"{fieldName}: {msg}");
            errorMessages.AddRange(fieldErrors);
        }

        return new OperationResult(
            success: false,
            messages: errorMessages,
            statusCode: 400
        )
        {
            Data = new ValidationErrorDetails
            {
                Timestamp = exception.Timestamp,
                EntityName = exception.EntityName,
                ActionName = exception.ActionName,
                FieldErrors = exception.Errors,
                ValidationDetails = exception.ValidationDetails
            }
        };
    }

    /// <summary>
    /// 取得欄位的中文名稱（用於更好的使用者體驗）
    /// </summary>
    /// <param name="fieldName">英文欄位名稱</param>
    /// <returns>中文欄位名稱</returns>
    private static string GetChineseFieldName(string fieldName)
    {
        return fieldName switch
        {
            // UserRegistrationRequest 相關欄位
            "UserRegistrationRequest.UserName" => "使用者帳號",
            "UserRegistrationRequest.Password" => "密碼",
            "UserRegistrationRequest.ConfirmPassword" => "確認密碼",
            "UserRegistrationRequest.Email" => "電子郵件",
            "UserRegistrationRequest.FullName" => "姓名",
            "UserRegistrationRequest.ServiceAgency" => "服務機構",
            "UserRegistrationRequest.SubordinateUnit" => "隸屬單位",
            "UserRegistrationRequest.JobTitle" => "職稱",
            "UserRegistrationRequest.OfficialPhone" => "公務電話",
            "UserRegistrationRequest.FileId" => "申請書",
            
            // SkyLabDevelopUserRegistrationRequest 相關欄位
            "SkyLabDevelopUserRegistrationRequest.UserName" => "開發者帳號",
            "SkyLabDevelopUserRegistrationRequest.Password" => "開發者密碼",
            "SkyLabDevelopUserRegistrationRequest.Email" => "開發者電子郵件",
            "SkyLabDevelopUserRegistrationRequest.OfficialEmail" => "官方電子郵件",
            
            // 通用欄位處理
            _ when fieldName.EndsWith(".UserName") => "使用者帳號",
            _ when fieldName.EndsWith(".Password") => "密碼", 
            _ when fieldName.EndsWith(".ConfirmPassword") => "確認密碼",
            _ when fieldName.EndsWith(".Email") => "電子郵件",
            _ when fieldName.EndsWith(".FullName") => "姓名",
            _ when fieldName.EndsWith(".ServiceAgency") => "服務機構",
            _ when fieldName.EndsWith(".FileId") => "申請書",
            
            // 如果沒有對應的中文名稱，返回原始欄位名稱
            _ => fieldName
        };
    }
}

/// <summary>
/// 驗證錯誤詳細資訊（用於 API 響應）
/// </summary>
public class ValidationErrorDetails
{
    /// <summary>
    /// 驗證發生時間
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// 實體名稱
    /// </summary>
    public string? EntityName { get; set; }
    
    /// <summary>
    /// 動作名稱
    /// </summary>
    public string? ActionName { get; set; }
    
    /// <summary>
    /// 欄位錯誤字典（欄位名稱 -> 錯誤訊息列表）
    /// </summary>
    public IDictionary<string, string[]> FieldErrors { get; set; } = new Dictionary<string, string[]>();
    
    /// <summary>
    /// 詳細驗證資訊列表
    /// </summary>
    public List<ValidationDetail> ValidationDetails { get; set; } = new();
}
