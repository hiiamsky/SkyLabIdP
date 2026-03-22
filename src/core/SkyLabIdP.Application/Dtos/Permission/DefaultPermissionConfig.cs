using SkyLabIdP.Domain.Enums;

namespace SkyLabIdP.Application.Dtos.Permission;

/// <summary>
/// 預設權限配置
/// </summary>
public class DefaultPermissionConfig
{
    /// <summary>
    /// 功能名稱
    /// </summary>
    public string FunctionName { get; }

    /// <summary>
    /// 權限值 (位元運算)
    /// </summary>
    public int PermissionValue { get; }

    /// <summary>
    /// 建構子
    /// </summary>
    /// <param name="functionName">功能名稱</param>
    /// <param name="permissions">權限列舉</param>
    public DefaultPermissionConfig(string functionName, Permissions permissions)
    {
        FunctionName = functionName;
        PermissionValue = (int)permissions;
    }

    /// <summary>
    /// 建構子 (直接指定權限值)
    /// </summary>
    /// <param name="functionName">功能名稱</param>
    /// <param name="permissionValue">權限值</param>
    public DefaultPermissionConfig(string functionName, int permissionValue)
    {
        FunctionName = functionName;
        PermissionValue = permissionValue;
    }
}
