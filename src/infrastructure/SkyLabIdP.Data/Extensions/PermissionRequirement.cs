using System;
using SkyLabIdP.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace SkyLabIdP.Data.Extensions;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string FunctionName { get; }
    public Permissions RequiredPermission { get; }

    public PermissionRequirement(string functionName, Permissions requiredPermission)
    {
        FunctionName = functionName;
        RequiredPermission = requiredPermission;
    }
}
