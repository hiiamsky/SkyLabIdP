using Microsoft.AspNetCore.Identity;
using SkyLabIdP.Domain.Entities;

namespace SkyLabIdP.Data.Identity
{
    /// <summary>
    /// 自定義密碼驗證器 - 防範常見密碼和弱密碼
    /// </summary>
    public class CustomPasswordValidator : IPasswordValidator<ApplicationUser>
    {
        /// <summary>
        /// 常見弱密碼黑名單 (OWASP Top 10,000 Common Passwords subset)
        /// </summary>
        private static readonly HashSet<string> CommonPasswords = new(StringComparer.OrdinalIgnoreCase)
        {
            // Top 100 最常見密碼
            "password", "123456", "123456789", "12345678", "12345", "1234567", "password1",
            "123123", "1234567890", "000000", "abc123", "password123", "1234", "iloveyou",
            "1q2w3e4r", "qwerty", "monkey", "dragon", "111111", "baseball", "letmein",
            "trustno1", "sunshine", "master", "welcome", "shadow", "ashley", "football",
            "jesus", "michael", "ninja", "mustang", "password1", "123321", "bailey",
            
            // 常見中文拼音密碼
            "qwertyuiop", "asdfghjkl", "zxcvbnm", "admin", "administrator", "root",
            "test", "guest", "user", "demo", "sample", "default", "changeme",
            
            // 鍵盤序列
            "qweasd", "qweasdzxc", "1qaz2wsx", "qazwsx", "zaq12wsx", "1q2w3e",
            
            // 常見台灣公司/系統密碼模式
            "skylab123", "skylab2024", "skylab2025", "skylab2026",
            "company123", "company2024", "welcome123", "admin123", "user123",
            
            // 日期模式 (常見出生年)
            "19900101", "19910101", "19920101", "19930101", "19940101", "19950101",
            "20000101", "20010101", "20020101"
        };

        /// <summary>
        /// 驗證密碼是否符合安全要求
        /// </summary>
        public async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string? password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "PasswordRequired",
                    Description = "密碼不可為空"
                });
            }

            var errors = new List<IdentityError>();

            // 1. 檢查是否為常見密碼
            if (CommonPasswords.Contains(password))
            {
                errors.Add(new IdentityError
                {
                    Code = "CommonPassword",
                    Description = "此密碼過於常見，請使用更安全的密碼"
                });
            }

            // 2. 檢查密碼是否包含使用者名稱 (防止 username = password 模式)
            if (!string.IsNullOrEmpty(user.UserName) && 
                password.Contains(user.UserName, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordContainsUserName",
                    Description = "密碼不可包含使用者名稱"
                });
            }

            // 3. 檢查密碼是否包含 Email 前綴
            if (!string.IsNullOrEmpty(user.Email))
            {
                var emailPrefix = user.Email.Split('@')[0];
                if (password.Contains(emailPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(new IdentityError
                    {
                        Code = "PasswordContainsEmail",
                        Description = "密碼不可包含電子郵件地址"
                    });
                }
            }

            // 4. 檢查連續字符 (例如: 111111, aaaaaa, 123456)
            if (HasConsecutiveCharacters(password, 4))
            {
                errors.Add(new IdentityError
                {
                    Code = "ConsecutiveCharacters",
                    Description = "密碼不可包含超過 3 個連續的相同字符"
                });
            }

            // 5. 檢查純數字或純字母 (需要混合)
            if (password.All(char.IsDigit) || password.All(char.IsLetter))
            {
                errors.Add(new IdentityError
                {
                    Code = "WeakPasswordPattern",
                    Description = "密碼必須包含數字和字母的組合"
                });
            }

            // 6. 檢查是否為鍵盤序列模式
            if (IsKeyboardPattern(password))
            {
                errors.Add(new IdentityError
                {
                    Code = "KeyboardPattern",
                    Description = "密碼不可為鍵盤連續字符 (例如: qwerty, asdfgh)"
                });
            }

            return errors.Count == 0 
                ? IdentityResult.Success 
                : IdentityResult.Failed(errors.ToArray());
        }

        /// <summary>
        /// 檢查是否有連續重複字符
        /// </summary>
        private static bool HasConsecutiveCharacters(string password, int maxConsecutive)
        {
            for (int i = 0; i < password.Length - maxConsecutive; i++)
            {
                if (password.Skip(i).Take(maxConsecutive).Distinct().Count() == 1)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 檢查是否為鍵盤序列模式
        /// </summary>
        private static bool IsKeyboardPattern(string password)
        {
            var keyboardRows = new[]
            {
                "qwertyuiop",
                "asdfghjkl",
                "zxcvbnm",
                "1234567890"
            };

            var lowerPassword = password.ToLowerInvariant();
            
            foreach (var row in keyboardRows)
            {
                for (int i = 0; i <= row.Length - 4; i++)
                {
                    var pattern = row.Substring(i, 4);
                    if (lowerPassword.Contains(pattern))
                    {
                        return true;
                    }
                    
                    // 反向檢查
                    var reversePattern = new string(pattern.Reverse().ToArray());
                    if (lowerPassword.Contains(reversePattern))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
