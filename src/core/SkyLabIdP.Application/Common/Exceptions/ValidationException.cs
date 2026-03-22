using System.Text;
using FluentValidation.Results;

namespace SkyLabIdP.Application.Common.Exceptions
{ /// <summary>
  /// 驗證例外處理
  /// </summary>
  public class ValidationException : Exception
  {
    /// <summary>
    /// 建立驗證例外
    /// </summary>
    public ValidationException()
      : base("輸入驗證發生一個或多個錯誤。")
    {
      Errors = new Dictionary<string, string[]>();
      Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// 建立驗證例外
    /// </summary>
    /// <param name="failures">驗證失敗集合</param>
    public ValidationException(IEnumerable<ValidationFailure> failures)
      : base(GetValidationErrorMessage(failures))
    {
      Errors = new Dictionary<string, string[]>();
      Timestamp = DateTime.UtcNow;
      
      var failureGroups = failures
        .GroupBy(e => e.PropertyName, e => e.ErrorMessage);

      foreach (var failureGroup in failureGroups)
      {
        var propertyName = failureGroup.Key;
        var propertyFailures = failureGroup.ToArray();

        Errors.Add(propertyName, propertyFailures);
      }

      // 記錄更詳細的驗證失敗資訊
      ValidationDetails = failures.Select(f => new ValidationDetail
      {
        PropertyName = f.PropertyName,
        ErrorMessage = f.ErrorMessage,
        AttemptedValue = f.AttemptedValue,
        Severity = f.Severity.ToString(),
        ErrorCode = f.ErrorCode
      }).ToList();
    }

    /// <summary>
    /// 產生包含驗證錯誤詳情的錯誤訊息
    /// </summary>
    /// <param name="failures">驗證失敗集合</param>
    /// <returns>格式化的錯誤訊息</returns>
    private static string GetValidationErrorMessage(IEnumerable<ValidationFailure> failures)
    {
      if (!failures.Any())
        return "輸入驗證發生一個或多個錯誤。";

      var errorMessages = failures
        .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
        .Select(g => $"[{g.Key}]: {string.Join(", ", g)}")
        .ToList();

      return $"輸入驗證發生錯誤: {string.Join("; ", errorMessages)}";
    }

    /// <summary>
    /// 建立驗證例外
    /// </summary>
    /// <param name="failures">驗證失敗集合</param>
    /// <param name="entityName">實體名稱</param>
    public ValidationException(IEnumerable<ValidationFailure> failures, string entityName)
      : this(failures)
    {
      EntityName = entityName;
    }

    /// <summary>
    /// 建立驗證例外
    /// </summary>
    /// <param name="failures">驗證失敗集合</param>
    /// <param name="entityName">實體名稱</param>
    /// <param name="actionName">動作名稱</param>
    public ValidationException(IEnumerable<ValidationFailure> failures, string entityName, string actionName)
      : this(failures, entityName)
    {
      ActionName = actionName;
    }

    /// <summary>
    /// 所有錯誤
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// 驗證細節集合
    /// </summary>
    public List<ValidationDetail> ValidationDetails { get; } = new List<ValidationDetail>();

    /// <summary>
    /// 驗證發生時間
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// 實體名稱
    /// </summary>
    public string? EntityName { get; }

    /// <summary>
    /// 動作名稱
    /// </summary>
    public string? ActionName { get; }

    /// <summary>
    /// 獲取格式化的詳細錯誤訊息
    /// </summary>
    /// <returns>詳細的錯誤訊息</returns>
    public string GetFormattedValidationErrors()
    {
      var sb = new StringBuilder();

      if (!string.IsNullOrEmpty(EntityName))
      {
        sb.AppendLine($"實體: {EntityName}");
      }

      if (!string.IsNullOrEmpty(ActionName))
      {
        sb.AppendLine($"動作: {ActionName}");
      }

      sb.AppendLine($"時間戳記: {Timestamp:yyyy-MM-dd HH:mm:ss.fff}");
      sb.AppendLine("驗證錯誤:");

      foreach (var error in Errors)
      {
        sb.AppendLine($"- 欄位: {error.Key}");
        foreach (var message in error.Value)
        {
          sb.AppendLine($"  • {message}");
        }
      }

      if (ValidationDetails.Any())
      {
        sb.AppendLine("詳細驗證信息:");
        foreach (var detail in ValidationDetails)
        {
          sb.AppendLine($"- 欄位: {detail.PropertyName}");
          sb.AppendLine($"  • 錯誤: {detail.ErrorMessage}");
          sb.AppendLine($"  • 嘗試值: {detail.AttemptedValue}");
          sb.AppendLine($"  • 嚴重性: {detail.Severity}");
          if (!string.IsNullOrEmpty(detail.ErrorCode))
          {
            sb.AppendLine($"  • 錯誤代碼: {detail.ErrorCode}");
          }
        }
      }

      return sb.ToString();
    }
  }

  /// <summary>
  /// 驗證詳細資訊
  /// </summary>
  public class ValidationDetail
  {
    /// <summary>
    /// 屬性名稱
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// 錯誤訊息
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// 嘗試值
    /// </summary>
    public object? AttemptedValue { get; set; }

    /// <summary>
    /// 嚴重性
    /// </summary>
    public string Severity { get; set; } = string.Empty;

    /// <summary>
    /// 錯誤代碼
    /// </summary>
    public string? ErrorCode { get; set; }
  }
}

