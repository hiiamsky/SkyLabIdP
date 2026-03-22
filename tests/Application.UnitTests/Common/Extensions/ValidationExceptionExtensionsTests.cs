using SkyLabIdP.Application.Common.Exceptions;
using SkyLabIdP.Application.Common.Extensions;
using SkyLabIdP.Application.Dtos.User.Registration;
using FluentValidation.Results;
using System.Text.Json;
using Xunit;

namespace Application.UnitTests.Common.Extensions
{
    /// <summary>
    /// 驗證錯誤處理擴展方法的單元測試
    /// 測試 ValidationException 的改進和新的錯誤處理機制
    /// </summary>
    public class ValidationExceptionExtensionsTests
    {
        /// <summary>
        /// 測試 ValidationException 包含詳細的錯誤訊息
        /// </summary>
        [Fact]
        public void ValidationException_Should_Include_Field_Names_In_Message()
        {
            // Arrange
            var failures = new List<ValidationFailure>
            {
                new ValidationFailure("UserRegistrationRequest.UserName", "使用者帳號是必填的。"),
                new ValidationFailure("UserRegistrationRequest.Password", "密碼至少需要12個字符。"),
                new ValidationFailure("UserRegistrationRequest.Password", "密碼必須包含至少一個大寫字母。"),
                new ValidationFailure("UserRegistrationRequest.Email", "需要有效的信箱。")
            };

            // Act
            var exception = new ValidationException(failures);

            // Assert
            var message = exception.Message;
            Assert.Contains("UserRegistrationRequest.UserName", message);
            Assert.Contains("使用者帳號是必填的", message);
            Assert.Contains("UserRegistrationRequest.Password", message);
            Assert.Contains("密碼至少需要12個字符", message);
            Assert.Contains("密碼必須包含至少一個大寫字母", message);
            Assert.Contains("UserRegistrationRequest.Email", message);
            Assert.Contains("需要有效的信箱", message);
        }

        /// <summary>
        /// 測試 ToOperationResult 擴展方法產生正確的 OperationResult
        /// </summary>
        [Fact]
        public void ToOperationResult_Should_Convert_ValidationException_To_OperationResult()
        {
            // Arrange
            var failures = new List<ValidationFailure>
            {
                new ValidationFailure("UserRegistrationRequest.UserName", "使用者帳號是必填的。"),
                new ValidationFailure("UserRegistrationRequest.Password", "密碼至少需要12個字符。"),
                new ValidationFailure("UserRegistrationRequest.Email", "需要有效的信箱。")
            };
            var exception = new ValidationException(failures, "RegisterSkyLabMgmUserCommand", "使用者註冊");

            // Act
            var operationResult = exception.ToOperationResult();

            // Assert
            Assert.False(operationResult.Success);
            Assert.Equal(400, operationResult.StatusCode);
            Assert.Equal(3, operationResult.Messages.Count);
            
            // 檢查中文欄位名稱轉換
            Assert.Contains("使用者帳號: 使用者帳號是必填的。", operationResult.Messages);
            Assert.Contains("密碼: 密碼至少需要12個字符。", operationResult.Messages);
            Assert.Contains("電子郵件: 需要有效的信箱。", operationResult.Messages);

            // 檢查 Data 包含詳細資訊
            Assert.NotNull(operationResult.Data);
            var validationDetails = operationResult.Data as ValidationErrorDetails;
            Assert.NotNull(validationDetails);
            Assert.Equal("RegisterSkyLabMgmUserCommand", validationDetails.EntityName);
            Assert.Equal("使用者註冊", validationDetails.ActionName);
            Assert.Equal(3, validationDetails.FieldErrors.Count);
        }

        /// <summary>
        /// 測試中文欄位名稱轉換功能
        /// </summary>
        [Theory]
        [InlineData("UserRegistrationRequest.UserName", "使用者帳號")]
        [InlineData("UserRegistrationRequest.Password", "密碼")]
        [InlineData("UserRegistrationRequest.Email", "電子郵件")]
        [InlineData("SkyLabDevelopUserRegistrationRequest.UserName", "開發者帳號")]
        [InlineData("SkyLabDevelopUserRegistrationRequest.Password", "開發者密碼")]
        [InlineData("UnknownField", "UnknownField")] // 未知欄位應該返回原始名稱
        public void Chinese_Field_Name_Mapping_Should_Work_Correctly(string originalFieldName, string expectedChineseName)
        {
            // Arrange
            var failures = new List<ValidationFailure>
            {
                new ValidationFailure(originalFieldName, "測試錯誤訊息")
            };
            var exception = new ValidationException(failures);

            // Act
            var operationResult = exception.ToOperationResult();

            // Assert
            var expectedMessage = $"{expectedChineseName}: 測試錯誤訊息";
            Assert.Contains(expectedMessage, operationResult.Messages);
        }

        /// <summary>
        /// 測試 ValidationException 包含完整的驗證詳細資訊
        /// </summary>
        [Fact]
        public void ValidationException_Should_Include_Complete_ValidationDetails()
        {
            // Arrange
            var failures = new List<ValidationFailure>
            {
                new ValidationFailure("UserName", "使用者帳號是必填的。")
                {
                    AttemptedValue = "",
                    ErrorCode = "NotEmptyValidator"
                },
                new ValidationFailure("Password", "密碼至少需要12個字符。")
                {
                    AttemptedValue = "123",
                    ErrorCode = "MinimumLengthValidator"
                }
            };

            // Act
            var exception = new ValidationException(failures);

            // Assert
            Assert.Equal(2, exception.ValidationDetails.Count);
            
            var userNameDetail = exception.ValidationDetails.First(d => d.PropertyName == "UserName");
            Assert.Equal("使用者帳號是必填的。", userNameDetail.ErrorMessage);
            Assert.Equal("", userNameDetail.AttemptedValue);
            Assert.Equal("NotEmptyValidator", userNameDetail.ErrorCode);

            var passwordDetail = exception.ValidationDetails.First(d => d.PropertyName == "Password");
            Assert.Equal("密碼至少需要12個字符。", passwordDetail.ErrorMessage);
            Assert.Equal("123", passwordDetail.AttemptedValue);
            Assert.Equal("MinimumLengthValidator", passwordDetail.ErrorCode);
        }

        /// <summary>
        /// 測試空的驗證錯誤集合處理
        /// </summary>
        [Fact]
        public void ValidationException_Should_Handle_Empty_Failures_Gracefully()
        {
            // Arrange
            var failures = new List<ValidationFailure>();

            // Act
            var exception = new ValidationException(failures);

            // Assert
            Assert.Equal("輸入驗證發生一個或多個錯誤。", exception.Message);
            Assert.Empty(exception.Errors);
            Assert.Empty(exception.ValidationDetails);

            var operationResult = exception.ToOperationResult();
            Assert.False(operationResult.Success);
            Assert.Equal(400, operationResult.StatusCode);
            Assert.Empty(operationResult.Messages);
        }
    }
}
