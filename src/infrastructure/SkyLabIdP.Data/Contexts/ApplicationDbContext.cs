using SkyLabIdP.Application.Common.Interfaces;

using SkyLabIdP.Domain;
using SkyLabIdP.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace SkyLabIdP.Data.Contexts
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRoles, string,
        ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin,
        ApplicationRoleClaim, ApplicationUserToken>, IApplicationDbContext
    {
        private IDbContextTransaction? _currentTransaction;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ── Business tables still accessed via EF (not yet migrated to Dapper) ──
        public DbSet<FileUpload> FileUploads { get; set; }
        public DbSet<PolicyConfiguration> PolicyConfigurations { get; set; }
        public DbSet<SysCode> SysCodes { get; set; }
        public DbSet<BranchArea> BranchAreas { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // 確保 Identity 模型也被配置
            builder.Entity<ApplicationUser>(b =>
            {
                // Each User can have many UserClaims
                b.HasMany(e => e.Claims)
                    .WithOne(e => e.User)
                    .HasForeignKey(uc => uc.UserId)
                    .IsRequired();

                // Each User can have many UserLogins
                b.HasMany(e => e.Logins)
                    .WithOne(e => e.User)
                    .HasForeignKey(ul => ul.UserId)
                    .IsRequired();

                // Each User can have many UserTokens
                b.HasMany(e => e.Tokens)
                    .WithOne(e => e.User)
                    .HasForeignKey(ut => ut.UserId)
                    .IsRequired();

                // Each User can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.User)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();


            });

            builder.Entity<ApplicationRoles>(b =>
            {
                // Each Role can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                // Each Role can have many associated RoleClaims
                b.HasMany(e => e.RoleClaims)
                    .WithOne(e => e.Role)
                    .HasForeignKey(rc => rc.RoleId)
                    .IsRequired();
            });

            builder.Entity<ApplicationUserClaim>(b =>
            {
                // 明確配置 Id 為自動增長主鍵
                b.HasKey(uc => uc.Id);
                b.Property(uc => uc.Id).ValueGeneratedOnAdd();
            });

            builder.Entity<UserTenant>(entity =>
            {
                entity.HasKey(e => e.SerialNo);
                entity.Property(e => e.SerialNo).ValueGeneratedOnAdd();
                entity.HasOne(ut => ut.User)
                    .WithMany(u => u.UserTenants)
                    .HasForeignKey(ut => ut.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<UserTenant>()
                .HasAlternateKey(u => u.TenantGuid); // 配置 TenantGuid 為替代鍵

            builder.Entity<SkyLabDocUserDetail>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            builder.Entity<SkyLabDocUserDetail>()
                .HasIndex(e => e.OfficialEmail)
                .IsUnique();

            builder.Entity<SkyLabDocUserDetail>()
                .HasIndex(e => e.FileId)
                .IsUnique();

            builder.Entity<SkyLabDocUserDetail>()
                .HasOne(e => e.UserTenant)
                .WithOne(u => u.SkyLabDocUserDetail)
                .HasForeignKey<SkyLabDocUserDetail>(e => e.UserTenantGuid)
                .HasPrincipalKey<UserTenant>(u => u.TenantGuid); // 使用 TenantGuid 作為外鍵關聯

            builder.Entity<SkyLabDevelopUserDetail>()
                .HasOne(e => e.UserTenant)
                .WithOne(u => u.SkyLabDevelopUserDetail)
                .HasForeignKey<SkyLabDevelopUserDetail>(e => e.UserTenantGuid)
                .HasPrincipalKey<UserTenant>(u => u.TenantGuid); // 使用 TenantGuid 作為外鍵關聯

            builder.Entity<Branch>().HasKey(b => b.BranchCode);

            builder.Entity<FileUpload>(entity =>
            {
                entity.HasKey(e => e.SerialNo);
                entity.Property(e => e.FileId).ValueGeneratedOnAdd();
            });


            builder.Entity<FunctionGroup>()
                .HasKey(fg => fg.GroupID);

            builder.Entity<Function>()
                .HasKey(f => f.FunctionID);  // 确保 FunctionID 是主键

            builder.Entity<Function>()
                .HasOne(f => f.FunctionGroup)
                .WithMany(fg => fg.Functions)
                .HasForeignKey(f => f.GroupID);

            builder.Entity<PolicyConfiguration>()
            .HasOne(p => p.Function)
            .WithMany(f => f.PolicyConfigurations)
            .HasForeignKey(p => p.FunctionID);


            builder.Entity<PasswordHistory>()
            .HasOne(p => p.User)
            .WithMany(u => u.PasswordHistories)
            .HasForeignKey(p => p.UserId);

            builder.Entity<SysCode>();

            // 額外的模型配置
            builder.Entity<BranchArea>(entity =>
            {
                entity.HasKey(e => e.AreaId);
            });


            // 添加 AuditLog 配置
            builder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Timestamp)
                    .HasDatabaseName("IDX_AuditLog_Timestamp");
                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("IDX_AuditLog_UserId");
                entity.HasIndex(e => e.RequestPath)
                    .HasDatabaseName("IDX_AuditLog_RequestPath");
                entity.HasIndex(e => e.StatusCode)
                    .HasDatabaseName("IDX_AuditLog_StatusCode");

                // 配置 JSON 儲存的欄位
                entity.Property(e => e.AdditionalInfo)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, new System.Text.Json.JsonSerializerOptions()),
                        v => System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, string>>(v, new System.Text.Json.JsonSerializerOptions()) ?? new System.Collections.Generic.Dictionary<string, string>()
                    );

                // 忽略 ExcludedPaths 屬性，不映射到資料庫
                entity.Ignore(e => e.ExcludedPaths);
            });


        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await base.SaveChangesAsync(cancellationToken);
        }

        public IDbContextTransaction GetCurrentTransaction() => _currentTransaction!;

        /// <summary>
        /// IApplicationDbContext.BeginTransactionAsync — starts a new EF transaction.
        /// </summary>
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
                throw new InvalidOperationException("A transaction is already in progress.");

            _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
        }

        /// <summary>
        /// IApplicationDbContext.CommitTransactionAsync — saves changes and commits the current transaction.
        /// </summary>
        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
                throw new InvalidOperationException("No active transaction to commit.");

            try
            {
                await SaveChangesAsync(cancellationToken);
                await _currentTransaction.CommitAsync(cancellationToken);
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        /// <summary>
        /// IApplicationDbContext.RollbackTransactionAsync — rolls back the current transaction.
        /// </summary>
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null) return;

            try
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public Task SaveChangesAsync()
        {
            return SaveChangesAsync(CancellationToken.None);
        }
    }
}