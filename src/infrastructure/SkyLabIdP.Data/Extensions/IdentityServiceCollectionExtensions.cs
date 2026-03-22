
using SkyLabIdP.Data.Contexts;
using SkyLabIdP.Data.Identity;
using SkyLabIdP.Domain.Entities;
using SkyLabIdP.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace SkyLabIdP.Data.Extensions
{
    public static class IdentityServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services)
        {

            // 註冊 IHttpContextAccessor
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddIdentityCore<ApplicationUser>(options =>
            {
                // Configure password policy
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 12;

                // Configure lockout policy
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                options.Lockout.AllowedForNewUsers = false;  // 新使用者預設不啟用鎖定

                // Configure user settings
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;

                // Configure token settings
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
            })
            .AddRoles<ApplicationRoles>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddPasswordValidator<CustomPasswordValidator>(); // 🔐 註冊自定義密碼驗證器

            services.Configure<DataProtectionTokenProviderOptions>(opt =>
            opt.TokenLifespan = TimeSpan.FromHours(2));

            services.AddScoped<SignInManager<ApplicationUser>>();
            var functions = new List<string>
            {
               "AcctMgmt"
            };
            services.AddAuthorization(options =>
            {

                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                                    .RequireAuthenticatedUser()
                                    .Build();
                var permissions = Enum.GetValues(typeof(Permissions))
                                      .Cast<Permissions>()
                                      .Where(p => p != Permissions.None)
                                      .ToList();

                foreach (var function in functions)
                {
                    foreach (var permission in permissions)
                    {
                        var policyName = $"Can{function}{permission}";
                        options.AddPolicy(policyName, policy =>
                            policy.Requirements.Add(new PermissionRequirement(function, permission)));
                    }
                }
            });
            return services;
        }
    }
}


