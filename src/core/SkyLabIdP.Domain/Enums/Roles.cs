namespace SkyLabIdP.Domain.Enums;

/// <summary>
/// 系統角色列舉
/// 定義所有可用的系統角色名稱
/// </summary>
public enum Roles
{
    /// <summary>
    /// SkyLab管理系統 - 一般管理者
    /// SkyLab
    /// </summary>
    SkyLabSystemMgmt,

    /// <summary>
    /// SkyLab管理系統 - 系統管理者
    /// </summary>
    SkyLabSystemAdmin,

    /// <summary>
    /// 開發單位
    /// </summary>
    SkyLabDeveloper,

    /// <summary>
    /// 外部機關使用者
    /// </summary>
    SkyLabExternalAgencyUser
}

/// <summary>
/// 角色列舉的擴展方法
/// </summary>
public static class RolesExtensions
{
    /// <summary>
    /// 取得角色的字串名稱
    /// </summary>
    /// <param name="role">角色列舉</param>
    /// <returns>角色名稱字串</returns>
    public static string GetName(this Roles role)
    {
        return role.ToString();
    }

    /// <summary>
    /// 從字串解析角色
    /// </summary>
    /// <param name="roleName">角色名稱</param>
    /// <returns>角色列舉，如果解析失敗則回傳 null</returns>
    public static Roles? ParseRole(string roleName)
    {
        if (Enum.TryParse<Roles>(roleName, true, out var role))
        {
            return role;
        }
        return null;
    }

    /// <summary>
    /// 取得角色的顯示名稱（中文）
    /// </summary>
    /// <param name="role">角色列舉</param>
    /// <returns>角色的中文顯示名稱</returns>
    public static string GetDisplayName(this Roles role)
    {
        return role switch
        {
            Roles.SkyLabSystemMgmt => "SkyLab系統一般管理者",
            Roles.SkyLabSystemAdmin => "SkyLab系統管理者",
            Roles.SkyLabDeveloper => "SkyLab系統開發者",
            Roles.SkyLabExternalAgencyUser => "外部機關使用者",
            _ => role.ToString()
        };
    }
}
