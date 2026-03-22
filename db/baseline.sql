IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FirstName] nvarchar(max) NULL,
        [LastName] nvarchar(max) NULL,
        [RefreshToken] nvarchar(max) NULL,
        [RefreshTokenExpiryTime] datetime2 NULL,
        [IsApproved] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [IsMigrated] bit NOT NULL,
        [IsMigratedAndReSetPWed] bit NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    CREATE TABLE [DISTRICT] (
        [DSTCODE] nvarchar(450) NOT NULL,
        [CITYCODE] nvarchar(max) NULL,
        [DSTNAME] nvarchar(max) NULL,
        [ENVTIT] nvarchar(max) NULL,
        [ENVTITB] nvarchar(max) NULL,
        [ENVADD] nvarchar(max) NULL,
        [ENVTEL] nvarchar(max) NULL,
        [ENVFAX] nvarchar(max) NULL,
        [MEMO1] nvarchar(max) NULL,
        [MEMO2] nvarchar(max) NULL,
        [MEMO3] nvarchar(max) NULL,
        [MEMO4] nvarchar(max) NULL,
        [MEMO5] nvarchar(max) NULL,
        [DORDER] nvarchar(max) NULL,
        CONSTRAINT [PK_DISTRICT] PRIMARY KEY ([DSTCODE])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    CREATE TABLE [FileUploads] (
        [SerialNo] int NOT NULL IDENTITY,
        [FileId] nvarchar(max) NOT NULL,
        [OriginalFileName] nvarchar(max) NOT NULL,
        [FileExtension] nvarchar(max) NOT NULL,
        [FileSystemType] nvarchar(max) NOT NULL,
        [FileDescription] nvarchar(max) NULL,
        [ApacheTikaContent] nvarchar(max) NOT NULL,
        [IsDisabled] bit NOT NULL,
        [Comments] nvarchar(max) NOT NULL,
        [CreatorId] nvarchar(max) NOT NULL,
        [CreatedTime] datetime2 NOT NULL,
        CONSTRAINT [PK_FileUploads] PRIMARY KEY ([SerialNo])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    CREATE TABLE [FunctionGroups] (
        [GroupID] nvarchar(450) NOT NULL,
        [GroupIcon] nvarchar(100) NOT NULL,
        [GroupTitle] nvarchar(100) NOT NULL,
        [GroupEnglishDescription] nvarchar(100) NOT NULL,
        [GroupChineseDescription] nvarchar(100) NOT NULL,
        [TargetRoute] nvarchar(200) NOT NULL,
        [IsDisabled] bit NOT NULL,
        [IsOpenFunctionList] bit NOT NULL,
        [GroupOrder] int NOT NULL,
        CONSTRAINT [PK_FunctionGroups] PRIMARY KEY ([GroupID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    CREATE TABLE [SkyLabDocUserDetails] (
        [SerialNo] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [SystemRole] nvarchar(255) NOT NULL,
        [FileId] nvarchar(450) NOT NULL,
        [UserName] nvarchar(256) NOT NULL,
        [FullName] nvarchar(255) NOT NULL,
        [ServiceAgency] nvarchar(10) NOT NULL,
        [SubordinateUnit] nvarchar(255) NOT NULL,
        [JobTitle] nvarchar(255) NOT NULL,
        [OfficialEmail] nvarchar(256) NOT NULL,
        [OfficialPhone] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_SkyLabDocUserDetails] PRIMARY KEY ([SerialNo]),
        CONSTRAINT [FK_SkyLabDocUserDetails_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    CREATE TABLE [PasswordHistory] (
        [SerialNo] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [HashedPassword] nvarchar(max) NOT NULL,
        [PasswordSalt] nvarchar(max) NOT NULL,
        [PasswordChangeDate] datetime2 NOT NULL,
        CONSTRAINT [PK_PasswordHistory] PRIMARY KEY ([SerialNo]),
        CONSTRAINT [FK_PasswordHistory_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    CREATE TABLE [Functions] (
        [FunctionID] nvarchar(450) NOT NULL,
        [GroupID] nvarchar(450) NOT NULL,
        [FunctionIcon] nvarchar(100) NOT NULL,
        [FunctionEnglishDescription] nvarchar(255) NOT NULL,
        [FunctionChineseDescription] nvarchar(255) NOT NULL,
        [TargetRoute] nvarchar(200) NOT NULL,
        [IsDisabled] bit NOT NULL,
        [IsDisplayInMenu] bit NOT NULL,
        [FunctionOrder] int NOT NULL,
        CONSTRAINT [PK_Functions] PRIMARY KEY ([FunctionID]),
        CONSTRAINT [FK_Functions_FunctionGroups_GroupID] FOREIGN KEY ([GroupID]) REFERENCES [FunctionGroups] ([GroupID]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    CREATE TABLE [PolicyConfigurations] (
        [SerialNo] int NOT NULL IDENTITY,
        [PolicyId] nvarchar(max) NOT NULL,
        [PolicyDescription] nvarchar(max) NOT NULL,
        [ClaimType] nvarchar(max) NOT NULL,
        [ClaimValue] nvarchar(max) NOT NULL,
        [FunctionID] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_PolicyConfigurations] PRIMARY KEY ([SerialNo]),
        CONSTRAINT [FK_PolicyConfigurations_Functions_FunctionID] FOREIGN KEY ([FunctionID]) REFERENCES [Functions] ([FunctionID]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SkyLabDocUserDetails_FileId] ON [SkyLabDocUserDetails] ([FileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SkyLabDocUserDetails_OfficialEmail] ON [SkyLabDocUserDetails] ([OfficialEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SkyLabDocUserDetails_UserId] ON [SkyLabDocUserDetails] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SkyLabDocUserDetails_UserName] ON [SkyLabDocUserDetails] ([UserName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Functions_GroupID] ON [Functions] ([GroupID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PasswordHistory_UserId] ON [PasswordHistory] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PolicyConfigurations_FunctionID] ON [PolicyConfigurations] ([FunctionID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240503004313_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240503004313_InitialCreate', N'8.0.21');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240507093707_AddNewFieldsToSkyLabDocUserDetail'
)
BEGIN
    ALTER TABLE [SkyLabDocUserDetails] ADD [CreateBy] nvarchar(450) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240507093707_AddNewFieldsToSkyLabDocUserDetail'
)
BEGIN
    ALTER TABLE [SkyLabDocUserDetails] ADD [CreateDatetime] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240507093707_AddNewFieldsToSkyLabDocUserDetail'
)
BEGIN
    ALTER TABLE [SkyLabDocUserDetails] ADD [LastLoginDatetime] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240507093707_AddNewFieldsToSkyLabDocUserDetail'
)
BEGIN
    ALTER TABLE [SkyLabDocUserDetails] ADD [LastUpdateDatetime] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240507093707_AddNewFieldsToSkyLabDocUserDetail'
)
BEGIN
    ALTER TABLE [SkyLabDocUserDetails] ADD [LastUpdatedBy] nvarchar(450) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240507093707_AddNewFieldsToSkyLabDocUserDetail'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240507093707_AddNewFieldsToSkyLabDocUserDetail', N'8.0.21');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240509082528_AddSysCode'
)
BEGIN
    CREATE TABLE [SysCode] (
        [SerialNo] bigint NOT NULL IDENTITY,
        [Type] nvarchar(100) NOT NULL,
        [Code] nvarchar(100) NOT NULL,
        [Desc] nvarchar(100) NOT NULL,
        [Item1] nvarchar(100) NOT NULL,
        [Item2] nvarchar(100) NOT NULL,
        [Item3] nvarchar(100) NOT NULL,
        [Item4] nvarchar(100) NOT NULL,
        [Item5] nvarchar(100) NOT NULL,
        [Item6] nvarchar(100) NOT NULL,
        [Item7] nvarchar(100) NOT NULL,
        [Item8] nvarchar(100) NOT NULL,
        [Item9] nvarchar(100) NOT NULL,
        [Item10] nvarchar(100) NOT NULL,
        [Item11] nvarchar(100) NOT NULL,
        [Item12] nvarchar(100) NOT NULL,
        [Item13] nvarchar(100) NOT NULL,
        [Item14] nvarchar(100) NOT NULL,
        [Item15] nvarchar(100) NOT NULL,
        [Item16] nvarchar(100) NOT NULL,
        [Item17] nvarchar(100) NOT NULL,
        [Item18] nvarchar(100) NOT NULL,
        [Item19] nvarchar(100) NOT NULL,
        [Item20] nvarchar(100) NOT NULL,
        [StopTag] bit NOT NULL,
        [Ord] nvarchar(50) NOT NULL,
        [createBy] nvarchar(450) NOT NULL,
        [createDate] datetime2 NULL,
        [LastUpdateBy] nvarchar(450) NOT NULL,
        [LastUpdateDate] datetime2 NULL,
        CONSTRAINT [PK_SysCode] PRIMARY KEY ([SerialNo])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240509082528_AddSysCode'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240509082528_AddSysCode', N'8.0.21');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510015508_updateCaseEntities'
)
BEGIN
    ALTER TABLE [SysCode] DROP CONSTRAINT [PK_SysCode];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510015508_updateCaseEntities'
)
BEGIN
    EXEC sp_rename N'[SysCode]', N'SysCodes';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510015508_updateCaseEntities'
)
BEGIN
    ALTER TABLE [SysCodes] ADD CONSTRAINT [PK_SysCodes] PRIMARY KEY ([SerialNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510015508_updateCaseEntities'
)
BEGIN
    CREATE TABLE [ABSN] (
        [HCODE] nvarchar(10) NOT NULL,
        [TAKER] nvarchar(20) NOT NULL,
        [TRIA1] nvarchar(7) NOT NULL,
        [TRIA2] nvarchar(7) NOT NULL,
        [TRIA3] nvarchar(7) NOT NULL,
        [TRIA4] nvarchar(7) NOT NULL,
        [TRIA5] nvarchar(7) NOT NULL,
        [COMIT1] nvarchar(7) NOT NULL,
        [COMIT2] nvarchar(7) NOT NULL,
        [COMIT3] nvarchar(7) NOT NULL,
        [PORCS] nvarchar(5) NOT NULL,
        [TRIA6] nvarchar(7) NOT NULL,
        [TRIA7] nvarchar(7) NOT NULL,
        [TRIA8] nvarchar(7) NOT NULL,
        [TRIA9] nvarchar(7) NOT NULL,
        [TRIA10] nvarchar(7) NOT NULL,
        [TRIA11] nvarchar(7) NOT NULL,
        [TRIA12] nvarchar(7) NOT NULL,
        [TRIA13] nvarchar(7) NOT NULL,
        [TRIA14] nvarchar(7) NOT NULL,
        [TRIA15] nvarchar(7) NOT NULL,
        [CONSULTI] nvarchar(100) NOT NULL,
        [PORCS_1] nvarchar(50) NOT NULL,
        [ForumM] nvarchar(100) NOT NULL,
        [CreateBy] nvarchar(450) NOT NULL,
        [CreateDatetime] datetime2 NOT NULL,
        [LastUpdatedBy] nvarchar(450) NOT NULL,
        [LastUpdateDatetime] datetime2 NOT NULL,
        CONSTRAINT [PK_ABSN] PRIMARY KEY ([HCODE])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510015508_updateCaseEntities'
)
BEGIN
    CREATE TABLE [ABSRL] (
        [HCODE] nvarchar(10) NOT NULL,
        [HCODLIST] nvarchar(500) NOT NULL,
        [DEPN] nvarchar(100) NOT NULL,
        [EDN] nvarchar(150) NOT NULL,
        [CreateBy] nvarchar(450) NOT NULL,
        [CreateDatetime] datetime2 NOT NULL,
        [LastUpdatedBy] nvarchar(450) NOT NULL,
        [LastUpdateDatetime] datetime2 NOT NULL,
        CONSTRAINT [PK_ABSRL] PRIMARY KEY ([HCODE])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510015508_updateCaseEntities'
)
BEGIN
    CREATE TABLE [ABST4] (
        [HCODE] nvarchar(10) NOT NULL,
        [A01] nvarchar(max) NOT NULL,
        [B01] nvarchar(max) NOT NULL,
        [C01] nvarchar(max) NOT NULL,
        [C02] nvarchar(max) NOT NULL,
        [C03] datetime2 NULL,
        [C04] nvarchar(max) NOT NULL,
        [C0501] float NULL,
        [C0502] float NULL,
        [C0503] float NULL,
        [C0504] nvarchar(max) NOT NULL,
        [D01] nvarchar(max) NOT NULL,
        [D02] nvarchar(max) NOT NULL,
        [D03C201] nvarchar(50) NOT NULL,
        [D03C202] float NULL,
        [D03C203] float NULL,
        [D03C204] float NULL,
        [D03WA01] nvarchar(50) NOT NULL,
        [D03WA02] float NULL,
        [D03WA03] nvarchar(max) NOT NULL,
        [D03WA04] float NULL,
        [D03WA05] float NULL,
        [D03WA06] float NULL,
        [D03WA07] nvarchar(max) NOT NULL,
        [D03WS01] nvarchar(50) NOT NULL,
        [D03WS02] float NULL,
        [D03WS03] nvarchar(max) NOT NULL,
        [D03WS04] float NULL,
        [D03WS05] nvarchar(max) NOT NULL,
        [E01] nvarchar(max) NOT NULL,
        [CreateBy] nvarchar(450) NOT NULL,
        [CreateDatetime] datetime2 NOT NULL,
        [LastUpdatedBy] nvarchar(450) NOT NULL,
        [LastUpdateDatetime] datetime2 NOT NULL,
        CONSTRAINT [PK_ABST4] PRIMARY KEY ([HCODE])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510015508_updateCaseEntities'
)
BEGIN
    CREATE TABLE [ABST4AR] (
        [UID] int NOT NULL IDENTITY,
        [HCODE] nvarchar(10) NOT NULL,
        [ID] nvarchar(50) NULL,
        [C01] nvarchar(50) NOT NULL,
        [N01] real NULL,
        [N02] real NULL,
        [N03] real NULL,
        [N04] real NULL,
        [U01] nvarchar(50) NOT NULL,
        [C02] nvarchar(max) NOT NULL,
        [CreateBy] nvarchar(450) NOT NULL,
        [CreateDatetime] datetime2 NOT NULL,
        [LastUpdatedBy] nvarchar(450) NOT NULL,
        [LastUpdateDatetime] datetime2 NOT NULL,
        CONSTRAINT [PK_ABST4AR] PRIMARY KEY ([UID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510015508_updateCaseEntities'
)
BEGIN
    CREATE TABLE [ABST4TX] (
        [UID] int NOT NULL IDENTITY,
        [HCODE] nvarchar(10) NOT NULL,
        [ID] nvarchar(50) NULL,
        [C01] nvarchar(50) NOT NULL,
        [N01] real NULL,
        [U01] nvarchar(50) NOT NULL,
        [C02] nvarchar(max) NOT NULL,
        [CreateBy] nvarchar(450) NOT NULL,
        [CreateDatetime] datetime2 NOT NULL,
        [LastUpdatedBy] nvarchar(450) NOT NULL,
        [LastUpdateDatetime] datetime2 NOT NULL,
        CONSTRAINT [PK_ABST4TX] PRIMARY KEY ([UID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510015508_updateCaseEntities'
)
BEGIN
    CREATE TABLE [ABST4WQ] (
        [Uid] int NOT NULL IDENTITY,
        [HCode] nvarchar(10) NOT NULL,
        [Id] nvarchar(50) NULL,
        [C01] nvarchar(50) NOT NULL,
        [N01] real NULL,
        [N02] real NULL,
        [U01] nvarchar(50) NOT NULL,
        [N03] real NULL,
        [U02] nvarchar(50) NOT NULL,
        [C02] nvarchar(MAX) NOT NULL,
        [CreateBy] nvarchar(450) NOT NULL,
        [CreateDatetime] datetime2 NOT NULL,
        [LastUpdatedBy] nvarchar(450) NOT NULL,
        [LastUpdateDatetime] datetime2 NOT NULL,
        CONSTRAINT [PK_ABST4WQ] PRIMARY KEY ([Uid])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510015508_updateCaseEntities'
)
BEGIN
    CREATE TABLE [ABSTRACT] (
        [HCode] nvarchar(10) NOT NULL,
        [Depn] nvarchar(100) NOT NULL,
        [Edn] nvarchar(150) NOT NULL,
        [Dst] nvarchar(50) NOT NULL,
        [Decal] nvarchar(10) NOT NULL,
        [Exdat] nvarchar(7) NOT NULL,
        [Exnu] nvarchar(50) NOT NULL,
        [Extp] nvarchar(2) NOT NULL,
        [Exct] text NOT NULL,
        [Sedat] nvarchar(7) NOT NULL,
        [Enddat] nvarchar(7) NOT NULL,
        [Darea] real NULL,
        [Dsize] real NULL,
        [Dsunt] nvarchar(1) NOT NULL,
        [Notes] nvarchar(255) NOT NULL,
        [Dirorg] nvarchar(100) NOT NULL,
        [Mailceo] nvarchar(255) NOT NULL,
        [Mailmgr] nvarchar(255) NOT NULL,
        [Booking] bit NULL,
        [Hcodext] nvarchar(50) NOT NULL,
        [Townlist] nvarchar(200) NOT NULL,
        [Examperd] int NULL,
        [Kml] bit NULL,
        [Releagey] nvarchar(max) NOT NULL,
        [Lsupdate] datetime2 NULL,
        [Formemo] nvarchar(1) NOT NULL,
        [Openprj] nvarchar(1) NOT NULL,
        [Mgrman] nvarchar(20) NOT NULL,
        [Mgretel] nvarchar(20) NOT NULL,
        [Soilman] nvarchar(100) NOT NULL,
        [Euic] nvarchar(50) NOT NULL,
        [Plansize] nvarchar(1) NOT NULL,
        [SkyLabEtcFilesCount] int NULL,
        [SkyLabEtcPagesCount] int NULL,
        [SkyLabEtcFilesChkTime] datetime2 NULL,
        [SkyLabPdfFilesCount] int NULL,
        [SkyLabPdfPagesCount] int NULL,
        [SkyLabPdfFilesChkTime] datetime2 NULL,
        [SkyLabPdfEncryptPass] nvarchar(255) NOT NULL,
        [MbrListExcl] nvarchar(500) NOT NULL,
        [FullTextCustom] nvarchar(1000) NOT NULL,
        CONSTRAINT [PK_ABSTRACT] PRIMARY KEY ([HCode])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510015508_updateCaseEntities'
)
BEGIN
    CREATE TABLE [SENSI] (
        [UID] int NOT NULL IDENTITY,
        [HCode] nvarchar(10) NOT NULL,
        [ItemNo] nvarchar(10) NOT NULL,
        [Known] nvarchar(1) NOT NULL,
        [Manifest] nvarchar(1000) NOT NULL,
        [Note] nvarchar(1000) NOT NULL,
        [Etc] nvarchar(1000) NOT NULL,
        CONSTRAINT [PK_SENSI] PRIMARY KEY ([UID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510015508_updateCaseEntities'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240510015508_updateCaseEntities', N'8.0.21');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510021041_AddAbsrlWithoneAbsn'
)
BEGIN
    ALTER TABLE [ABSRL] ADD CONSTRAINT [FK_ABSRL_ABSN_HCODE] FOREIGN KEY ([HCODE]) REFERENCES [ABSN] ([HCODE]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510021041_AddAbsrlWithoneAbsn'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240510021041_AddAbsrlWithoneAbsn', N'8.0.21');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510021745_AddAbsrlWithoneAbst4'
)
BEGIN
    ALTER TABLE [ABSRL] ADD CONSTRAINT [FK_ABSRL_ABST4_HCODE] FOREIGN KEY ([HCODE]) REFERENCES [ABST4] ([HCODE]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510021745_AddAbsrlWithoneAbst4'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240510021745_AddAbsrlWithoneAbst4', N'8.0.21');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510021928_AddAbsrlWithoneAbstract'
)
BEGIN
    ALTER TABLE [ABSRL] ADD CONSTRAINT [FK_ABSRL_ABSTRACT_HCODE] FOREIGN KEY ([HCODE]) REFERENCES [ABSTRACT] ([HCode]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510021928_AddAbsrlWithoneAbstract'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240510021928_AddAbsrlWithoneAbstract', N'8.0.21');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510022216_AddAbsrlWithoneAbst4Ar'
)
BEGIN
    ALTER TABLE [ABST4AR] DROP CONSTRAINT [PK_ABST4AR];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510022216_AddAbsrlWithoneAbst4Ar'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ABST4AR]') AND [c].[name] = N'ID');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [ABST4AR] DROP CONSTRAINT [' + @var0 + '];');
    EXEC(N'UPDATE [ABST4AR] SET [ID] = N'''' WHERE [ID] IS NULL');
    ALTER TABLE [ABST4AR] ALTER COLUMN [ID] nvarchar(50) NOT NULL;
    ALTER TABLE [ABST4AR] ADD DEFAULT N'' FOR [ID];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510022216_AddAbsrlWithoneAbst4Ar'
)
BEGIN
    ALTER TABLE [ABST4AR] ADD CONSTRAINT [PK_ABST4AR] PRIMARY KEY ([HCODE]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510022216_AddAbsrlWithoneAbst4Ar'
)
BEGIN
    ALTER TABLE [ABSRL] ADD CONSTRAINT [FK_ABSRL_ABST4AR_HCODE] FOREIGN KEY ([HCODE]) REFERENCES [ABST4AR] ([HCODE]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510022216_AddAbsrlWithoneAbst4Ar'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240510022216_AddAbsrlWithoneAbst4Ar', N'8.0.21');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510033447_AddAbsrlWithone'
)
BEGIN
    ALTER TABLE [ABST4WQ] DROP CONSTRAINT [PK_ABST4WQ];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510033447_AddAbsrlWithone'
)
BEGIN
    ALTER TABLE [ABST4TX] DROP CONSTRAINT [PK_ABST4TX];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510033447_AddAbsrlWithone'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ABST4WQ]') AND [c].[name] = N'Id');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [ABST4WQ] DROP CONSTRAINT [' + @var1 + '];');
    EXEC(N'UPDATE [ABST4WQ] SET [Id] = N'''' WHERE [Id] IS NULL');
    ALTER TABLE [ABST4WQ] ALTER COLUMN [Id] nvarchar(50) NOT NULL;
    ALTER TABLE [ABST4WQ] ADD DEFAULT N'' FOR [Id];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510033447_AddAbsrlWithone'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ABST4TX]') AND [c].[name] = N'ID');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [ABST4TX] DROP CONSTRAINT [' + @var2 + '];');
    EXEC(N'UPDATE [ABST4TX] SET [ID] = N'''' WHERE [ID] IS NULL');
    ALTER TABLE [ABST4TX] ALTER COLUMN [ID] nvarchar(50) NOT NULL;
    ALTER TABLE [ABST4TX] ADD DEFAULT N'' FOR [ID];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510033447_AddAbsrlWithone'
)
BEGIN
    ALTER TABLE [ABST4WQ] ADD CONSTRAINT [PK_ABST4WQ] PRIMARY KEY ([HCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510033447_AddAbsrlWithone'
)
BEGIN
    ALTER TABLE [ABST4TX] ADD CONSTRAINT [PK_ABST4TX] PRIMARY KEY ([HCODE]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510033447_AddAbsrlWithone'
)
BEGIN
    ALTER TABLE [ABSRL] ADD CONSTRAINT [FK_ABSRL_ABST4TX_HCODE] FOREIGN KEY ([HCODE]) REFERENCES [ABST4TX] ([HCODE]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510033447_AddAbsrlWithone'
)
BEGIN
    ALTER TABLE [ABSRL] ADD CONSTRAINT [FK_ABSRL_ABST4WQ_HCODE] FOREIGN KEY ([HCODE]) REFERENCES [ABST4WQ] ([HCode]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510033447_AddAbsrlWithone'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240510033447_AddAbsrlWithone', N'8.0.21');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510050459_chengeAbst4ArKey'
)
BEGIN
    ALTER TABLE [ABSRL] DROP CONSTRAINT [FK_ABSRL_ABST4AR_HCODE];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510050459_chengeAbst4ArKey'
)
BEGIN
    ALTER TABLE [SENSI] DROP CONSTRAINT [PK_SENSI];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510050459_chengeAbst4ArKey'
)
BEGIN
    ALTER TABLE [ABST4AR] DROP CONSTRAINT [PK_ABST4AR];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510050459_chengeAbst4ArKey'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ABST4AR]') AND [c].[name] = N'ID');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [ABST4AR] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [ABST4AR] ALTER COLUMN [ID] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510050459_chengeAbst4ArKey'
)
BEGIN
    ALTER TABLE [SENSI] ADD CONSTRAINT [PK_SENSI] PRIMARY KEY ([HCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510050459_chengeAbst4ArKey'
)
BEGIN
    ALTER TABLE [ABST4AR] ADD CONSTRAINT [PK_ABST4AR] PRIMARY KEY ([UID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510050459_chengeAbst4ArKey'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ABST4AR_HCODE] ON [ABST4AR] ([HCODE]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510050459_chengeAbst4ArKey'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ABSRL_HCODE] ON [ABSRL] ([HCODE]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510050459_chengeAbst4ArKey'
)
BEGIN
    ALTER TABLE [ABST4AR] ADD CONSTRAINT [FK_ABST4AR_ABSRL_HCODE] FOREIGN KEY ([HCODE]) REFERENCES [ABSRL] ([HCODE]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510050459_chengeAbst4ArKey'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240510050459_chengeAbst4ArKey', N'8.0.21');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510050727_chengeKey'
)
BEGIN
    ALTER TABLE [ABSRL] DROP CONSTRAINT [FK_ABSRL_ABST4TX_HCODE];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510050727_chengeKey'
)
BEGIN
    ALTER TABLE [ABSRL] DROP CONSTRAINT [FK_ABSRL_ABST4WQ_HCODE];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510050727_chengeKey'
)
BEGIN
    ALTER TABLE [ABST4WQ] DROP CONSTRAINT [PK_ABST4WQ];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510050727_chengeKey'
)
BEGIN
    ALTER TABLE [ABST4TX] DROP CONSTRAINT [PK_ABST4TX];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510050727_chengeKey'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ABST4WQ]') AND [c].[name] = N'Id');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [ABST4WQ] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [ABST4WQ] ALTER COLUMN [Id] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510050727_chengeKey'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ABST4TX]') AND [c].[name] = N'ID');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [ABST4TX] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [ABST4TX] ALTER COLUMN [ID] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510050727_chengeKey'
)
BEGIN
    ALTER TABLE [ABST4WQ] ADD CONSTRAINT [PK_ABST4WQ] PRIMARY KEY ([Uid]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510050727_chengeKey'
)
BEGIN
    ALTER TABLE [ABST4TX] ADD CONSTRAINT [PK_ABST4TX] PRIMARY KEY ([UID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510050727_chengeKey'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ABST4WQ_HCode] ON [ABST4WQ] ([HCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510050727_chengeKey'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ABST4TX_HCODE] ON [ABST4TX] ([HCODE]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510050727_chengeKey'
)
BEGIN
    ALTER TABLE [ABST4TX] ADD CONSTRAINT [FK_ABST4TX_ABSRL_HCODE] FOREIGN KEY ([HCODE]) REFERENCES [ABSRL] ([HCODE]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510050727_chengeKey'
)
BEGIN
    ALTER TABLE [ABST4WQ] ADD CONSTRAINT [FK_ABST4WQ_ABSRL_HCode] FOREIGN KEY ([HCode]) REFERENCES [ABSRL] ([HCODE]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240510050727_chengeKey'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240510050727_chengeKey', N'8.0.21');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511030358_updateDb'
)
BEGIN
    ALTER TABLE [ABSRL] DROP CONSTRAINT [FK_ABSRL_ABSN_HCODE];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511030358_updateDb'
)
BEGIN
    ALTER TABLE [ABSRL] DROP CONSTRAINT [FK_ABSRL_ABST4_HCODE];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511030358_updateDb'
)
BEGIN
    ALTER TABLE [ABSRL] DROP CONSTRAINT [FK_ABSRL_ABSTRACT_HCODE];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511030358_updateDb'
)
BEGIN
    DROP INDEX [IX_ABST4WQ_HCode] ON [ABST4WQ];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511030358_updateDb'
)
BEGIN
    DROP INDEX [IX_ABST4TX_HCODE] ON [ABST4TX];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511030358_updateDb'
)
BEGIN
    DROP INDEX [IX_ABST4AR_HCODE] ON [ABST4AR];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511030358_updateDb'
)
BEGIN
    ALTER TABLE [ABSTRACT] ADD [CreateBy] nvarchar(450) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511030358_updateDb'
)
BEGIN
    ALTER TABLE [ABSTRACT] ADD [CreateDatetime] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511030358_updateDb'
)
BEGIN
    ALTER TABLE [ABSTRACT] ADD [LastUpdateDatetime] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511030358_updateDb'
)
BEGIN
    ALTER TABLE [ABSTRACT] ADD [LastUpdatedBy] nvarchar(450) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511030358_updateDb'
)
BEGIN
    ALTER TABLE [ABSRL] ADD [CaseType] nvarchar(2) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511030358_updateDb'
)
BEGIN
    ALTER TABLE [ABSRL] ADD [MulitCaseType] nvarchar(20) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511030358_updateDb'
)
BEGIN
    ALTER TABLE [ABSRL] ADD [SourceHCode] nvarchar(10) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511030358_updateDb'
)
BEGIN
    ALTER TABLE [ABSRL] ADD [YEAR] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511030358_updateDb'
)
BEGIN
    CREATE INDEX [IX_ABST4WQ_HCode] ON [ABST4WQ] ([HCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511030358_updateDb'
)
BEGIN
    CREATE INDEX [IX_ABST4TX_HCODE] ON [ABST4TX] ([HCODE]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511030358_updateDb'
)
BEGIN
    CREATE INDEX [IX_ABST4AR_HCODE] ON [ABST4AR] ([HCODE]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511030358_updateDb'
)
BEGIN
    ALTER TABLE [ABSN] ADD CONSTRAINT [FK_ABSN_ABSRL_HCODE] FOREIGN KEY ([HCODE]) REFERENCES [ABSRL] ([HCODE]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511030358_updateDb'
)
BEGIN
    ALTER TABLE [ABST4] ADD CONSTRAINT [FK_ABST4_ABSRL_HCODE] FOREIGN KEY ([HCODE]) REFERENCES [ABSRL] ([HCODE]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511030358_updateDb'
)
BEGIN
    ALTER TABLE [ABSTRACT] ADD CONSTRAINT [FK_ABSTRACT_ABSRL_HCode] FOREIGN KEY ([HCode]) REFERENCES [ABSRL] ([HCODE]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511030358_updateDb'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240511030358_updateDb', N'8.0.21');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511071625_updateDb202405111512'
)
BEGIN
    ALTER TABLE [ABSN] DROP CONSTRAINT [FK_ABSN_ABSRL_HCODE];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511071625_updateDb202405111512'
)
BEGIN
    ALTER TABLE [ABST4] DROP CONSTRAINT [FK_ABST4_ABSRL_HCODE];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511071625_updateDb202405111512'
)
BEGIN
    ALTER TABLE [ABST4AR] DROP CONSTRAINT [FK_ABST4AR_ABSRL_HCODE];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511071625_updateDb202405111512'
)
BEGIN
    ALTER TABLE [ABST4TX] DROP CONSTRAINT [FK_ABST4TX_ABSRL_HCODE];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511071625_updateDb202405111512'
)
BEGIN
    ALTER TABLE [ABST4WQ] DROP CONSTRAINT [FK_ABST4WQ_ABSRL_HCode];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511071625_updateDb202405111512'
)
BEGIN
    ALTER TABLE [ABSTRACT] DROP CONSTRAINT [FK_ABSTRACT_ABSRL_HCode];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511071625_updateDb202405111512'
)
BEGIN
    DROP INDEX [IX_ABST4WQ_HCode] ON [ABST4WQ];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511071625_updateDb202405111512'
)
BEGIN
    DROP INDEX [IX_ABST4TX_HCODE] ON [ABST4TX];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511071625_updateDb202405111512'
)
BEGIN
    DROP INDEX [IX_ABST4AR_HCODE] ON [ABST4AR];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511071625_updateDb202405111512'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ABSRL]') AND [c].[name] = N'MulitCaseType');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [ABSRL] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [ABSRL] ALTER COLUMN [MulitCaseType] nvarchar(2) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511071625_updateDb202405111512'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240511071625_updateDb202405111512', N'8.0.21');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511072139_updateDb202405111521'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ABST4WQ_HCode] ON [ABST4WQ] ([HCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511072139_updateDb202405111521'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ABST4TX_HCODE] ON [ABST4TX] ([HCODE]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511072139_updateDb202405111521'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ABST4AR_HCODE] ON [ABST4AR] ([HCODE]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511072139_updateDb202405111521'
)
BEGIN
    ALTER TABLE [ABSN] ADD CONSTRAINT [FK_ABSN_ABSRL_HCODE] FOREIGN KEY ([HCODE]) REFERENCES [ABSRL] ([HCODE]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511072139_updateDb202405111521'
)
BEGIN
    ALTER TABLE [ABST4] ADD CONSTRAINT [FK_ABST4_ABSRL_HCODE] FOREIGN KEY ([HCODE]) REFERENCES [ABSRL] ([HCODE]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511072139_updateDb202405111521'
)
BEGIN
    ALTER TABLE [ABST4AR] ADD CONSTRAINT [FK_ABST4AR_ABSRL_HCODE] FOREIGN KEY ([HCODE]) REFERENCES [ABSRL] ([HCODE]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511072139_updateDb202405111521'
)
BEGIN
    ALTER TABLE [ABST4TX] ADD CONSTRAINT [FK_ABST4TX_ABSRL_HCODE] FOREIGN KEY ([HCODE]) REFERENCES [ABSRL] ([HCODE]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511072139_updateDb202405111521'
)
BEGIN
    ALTER TABLE [ABST4WQ] ADD CONSTRAINT [FK_ABST4WQ_ABSRL_HCode] FOREIGN KEY ([HCode]) REFERENCES [ABSRL] ([HCODE]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511072139_updateDb202405111521'
)
BEGIN
    ALTER TABLE [ABSTRACT] ADD CONSTRAINT [FK_ABSTRACT_ABSRL_HCode] FOREIGN KEY ([HCode]) REFERENCES [ABSRL] ([HCODE]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240511072139_updateDb202405111521'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240511072139_updateDb202405111521', N'8.0.21');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    ALTER TABLE [SkyLabDocUserDetails] DROP CONSTRAINT [FK_SkyLabDocUserDetails_AspNetUsers_UserId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    DROP TABLE [ABSN];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    DROP TABLE [ABST4];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    DROP TABLE [ABST4AR];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    DROP TABLE [ABST4TX];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    DROP TABLE [ABST4WQ];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    DROP TABLE [ABSTRACT];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    DROP TABLE [SENSI];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    DROP TABLE [ABSRL];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    ALTER TABLE [SkyLabDocUserDetails] DROP CONSTRAINT [PK_SkyLabDocUserDetails];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    DROP INDEX [IX_SkyLabDocUserDetails_UserId] ON [SkyLabDocUserDetails];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    EXEC sp_rename N'[SkyLabDocUserDetails]', N'SkyLabDocUserDetail';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    EXEC sp_rename N'[SkyLabDocUserDetail].[IX_SkyLabDocUserDetails_UserName]', N'IX_SkyLabDocUserDetail_UserName', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    EXEC sp_rename N'[SkyLabDocUserDetail].[IX_SkyLabDocUserDetails_OfficialEmail]', N'IX_SkyLabDocUserDetail_OfficialEmail', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    EXEC sp_rename N'[SkyLabDocUserDetail].[IX_SkyLabDocUserDetails_FileId]', N'IX_SkyLabDocUserDetail_FileId', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    ALTER TABLE [DISTRICT] ADD [ISDISPLAYED] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [ExternalId] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [ExternalProvider] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [HasCompletedRegistration] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [IsExternalAccount] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    ALTER TABLE [SkyLabDocUserDetail] ADD [MoicaCardNumber] nvarchar(450) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    ALTER TABLE [SkyLabDocUserDetail] ADD [ReasonsForDisapproval] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    ALTER TABLE [SkyLabDocUserDetail] ADD [UserTenantGuid] nvarchar(450) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SkyLabDocUserDetail]') AND [c].[name] = N'SerialNo');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [SkyLabDocUserDetail] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [SkyLabDocUserDetail] DROP COLUMN [SerialNo];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    ALTER TABLE [SkyLabDocUserDetail] ADD [SerialNo] int NOT NULL IDENTITY;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    ALTER TABLE [SkyLabDocUserDetail] ADD CONSTRAINT [PK_SkyLabDocUserDetail] PRIMARY KEY ([SerialNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    CREATE TABLE [AREA] (
        [AREAID] nvarchar(4) NOT NULL,
        [AREANA] nvarchar(10) NOT NULL,
        [AREAID2] nvarchar(4) NOT NULL,
        [DstCode] nvarchar(2) NOT NULL,
        [ISDISPLAYED] bit NULL,
        [RELDSTCODE] nvarchar(2) NOT NULL,
        [CITYCODE] nvarchar(1) NOT NULL,
        CONSTRAINT [PK_AREA] PRIMARY KEY ([AREAID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    CREATE TABLE [AuditLog] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] nvarchar(450) NULL,
        [UserName] nvarchar(max) NULL,
        [TraceId] nvarchar(max) NULL,
        [Timestamp] datetime2 NOT NULL,
        [RequestMethod] nvarchar(max) NULL,
        [RequestPath] nvarchar(450) NULL,
        [RequestQueryString] nvarchar(max) NULL,
        [RequestBody] nvarchar(MAX) NULL,
        [StatusCode] int NOT NULL,
        [ResponseBody] nvarchar(MAX) NULL,
        [ExecutionTime] bigint NOT NULL,
        [IPAddress] nvarchar(max) NULL,
        [UserAgent] nvarchar(max) NULL,
        [AdditionalInfo] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_AuditLog] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    CREATE TABLE [DECALNAME] (
        [DECAL] nvarchar(5) NOT NULL,
        [DECOLD] nvarchar(100) NOT NULL,
        [DECALNAME] nvarchar(100) NOT NULL,
        [TYPE] nvarchar(10) NOT NULL,
        [DecalGroup] nvarchar(10) NOT NULL,
        [UseStartDate] datetime2 NOT NULL,
        [UseEndDate] datetime2 NOT NULL,
        CONSTRAINT [PK_DECALNAME] PRIMARY KEY ([DECAL])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    CREATE TABLE [DSTNAME] (
        [DST] nvarchar(1) NOT NULL,
        [DSTNAME] nvarchar(10) NOT NULL,
        [DSTCODE] nvarchar(2) NOT NULL,
        [ISDISPLAYED] bit NOT NULL,
        [CITYCODE] nvarchar(1) NOT NULL,
        [RELDSTCODE] nvarchar(2) NOT NULL,
        CONSTRAINT [PK_DSTNAME] PRIMARY KEY ([DST]),
        CONSTRAINT [FK_DSTNAME_DISTRICT_DSTCODE] FOREIGN KEY ([DSTCODE]) REFERENCES [DISTRICT] ([DSTCODE]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    CREATE TABLE [UserTenant] (
        [SerialNo] int NOT NULL IDENTITY,
        [TenantGuid] nvarchar(450) NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [TenantId] nvarchar(50) NOT NULL,
        [CreateDateTime] datetime2 NOT NULL,
        CONSTRAINT [PK_UserTenant] PRIMARY KEY ([SerialNo]),
        CONSTRAINT [AK_UserTenant_TenantGuid] UNIQUE ([TenantGuid]),
        CONSTRAINT [FK_UserTenant_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    CREATE TABLE [SkyLabCaedpUserDetail] (
        [SerialNo] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [FullName] nvarchar(255) NOT NULL,
        [ServiceAgency] nvarchar(2) NOT NULL,
        [SubordinateUnit] nvarchar(255) NOT NULL,
        [JobTitle] nvarchar(255) NOT NULL,
        [OfficialEmail] nvarchar(256) NOT NULL,
        [OfficialPhone] nvarchar(50) NOT NULL,
        [CreateBy] nvarchar(450) NOT NULL,
        [CreateDatetime] datetime2 NOT NULL,
        [LastLoginDatetime] datetime2 NOT NULL,
        [LastUpdateDatetime] datetime2 NOT NULL,
        [LastUpdatedBy] nvarchar(450) NOT NULL,
        [UserTenantGuid] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_SkyLabCaedpUserDetail] PRIMARY KEY ([SerialNo]),
        CONSTRAINT [FK_SkyLabCaedpUserDetail_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SkyLabCaedpUserDetail_UserTenant_UserTenantGuid] FOREIGN KEY ([UserTenantGuid]) REFERENCES [UserTenant] ([TenantGuid]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    CREATE TABLE [SkyLabCommitteeUserDetail] (
        [SerialNo] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [MemberID] nvarchar(50) NOT NULL,
        [FullName] nvarchar(255) NOT NULL,
        [Gender] nvarchar(1) NOT NULL,
        [MemberType] nvarchar(50) NOT NULL,
        [CompanyName] nvarchar(255) NOT NULL,
        [DepartmentName] nvarchar(255) NOT NULL,
        [JobTitle] nvarchar(255) NOT NULL,
        [Active] bit NOT NULL,
        [IsPersonal] bit NOT NULL,
        [GroupSort] int NOT NULL,
        [MainEmail] nvarchar(50) NOT NULL,
        [SpareEmail] nvarchar(50) NOT NULL,
        [MainTel] nvarchar(50) NOT NULL,
        [SpareTel] nvarchar(50) NOT NULL,
        [ZipCode] nvarchar(10) NOT NULL,
        [Address] nvarchar(300) NOT NULL,
        [Specialty] nvarchar(50) NOT NULL,
        [LastLoginDatetime] datetime2 NOT NULL,
        [CreateBy] nvarchar(450) NOT NULL,
        [CreateDatetime] datetime2 NOT NULL,
        [LastUpdatedBy] nvarchar(450) NOT NULL,
        [LastUpdateDatetime] datetime2 NOT NULL,
        [UserTenantGuid] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_SkyLabCommitteeUserDetail] PRIMARY KEY ([SerialNo]),
        CONSTRAINT [FK_SkyLabCommitteeUserDetail_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SkyLabCommitteeUserDetail_UserTenant_UserTenantGuid] FOREIGN KEY ([UserTenantGuid]) REFERENCES [UserTenant] ([TenantGuid]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    CREATE TABLE [SkyLabDevelopUserDetail] (
        [SerialNo] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [FullName] nvarchar(255) NOT NULL,
        [ServiceAgency] nvarchar(2) NOT NULL,
        [SubordinateUnit] nvarchar(255) NOT NULL,
        [JobTitle] nvarchar(255) NOT NULL,
        [OfficialEmail] nvarchar(256) NOT NULL,
        [OfficialPhone] nvarchar(50) NOT NULL,
        [CreateBy] nvarchar(450) NOT NULL,
        [CreateDatetime] datetime2 NOT NULL,
        [LastLoginDatetime] datetime2 NOT NULL,
        [LastUpdateDatetime] datetime2 NOT NULL,
        [LastUpdatedBy] nvarchar(450) NOT NULL,
        [UserTenantGuid] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_SkyLabDevelopUserDetail] PRIMARY KEY ([SerialNo]),
        CONSTRAINT [FK_SkyLabDevelopUserDetail_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_SkyLabDevelopUserDetail_UserTenant_UserTenantGuid] FOREIGN KEY ([UserTenantGuid]) REFERENCES [UserTenant] ([TenantGuid]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SkyLabDocUserDetail_ServiceAgency] ON [SkyLabDocUserDetail] ([ServiceAgency]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    CREATE INDEX [IX_SkyLabDocUserDetail_UserId] ON [SkyLabDocUserDetail] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SkyLabDocUserDetail_UserTenantGuid] ON [SkyLabDocUserDetail] ([UserTenantGuid]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    CREATE INDEX [IDX_AuditLog_RequestPath] ON [AuditLog] ([RequestPath]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    CREATE INDEX [IDX_AuditLog_StatusCode] ON [AuditLog] ([StatusCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    CREATE INDEX [IDX_AuditLog_Timestamp] ON [AuditLog] ([Timestamp]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    CREATE INDEX [IDX_AuditLog_UserId] ON [AuditLog] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DSTNAME_DSTCODE] ON [DSTNAME] ([DSTCODE]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    CREATE INDEX [IX_SkyLabCaedpUserDetail_UserId] ON [SkyLabCaedpUserDetail] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SkyLabCaedpUserDetail_UserTenantGuid] ON [SkyLabCaedpUserDetail] ([UserTenantGuid]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    CREATE INDEX [IX_SkyLabCommitteeUserDetail_UserId] ON [SkyLabCommitteeUserDetail] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SkyLabCommitteeUserDetail_UserTenantGuid] ON [SkyLabCommitteeUserDetail] ([UserTenantGuid]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    CREATE INDEX [IX_SkyLabDevelopUserDetail_UserId] ON [SkyLabDevelopUserDetail] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SkyLabDevelopUserDetail_UserTenantGuid] ON [SkyLabDevelopUserDetail] ([UserTenantGuid]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    CREATE INDEX [IX_UserTenant_UserId] ON [UserTenant] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    ALTER TABLE [SkyLabDocUserDetail] ADD CONSTRAINT [FK_SkyLabDocUserDetail_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    ALTER TABLE [SkyLabDocUserDetail] ADD CONSTRAINT [FK_SkyLabDocUserDetail_DISTRICT_ServiceAgency] FOREIGN KEY ([ServiceAgency]) REFERENCES [DISTRICT] ([DSTCODE]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    ALTER TABLE [SkyLabDocUserDetail] ADD CONSTRAINT [FK_SkyLabDocUserDetail_UserTenant_UserTenantGuid] FOREIGN KEY ([UserTenantGuid]) REFERENCES [UserTenant] ([TenantGuid]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250530032952_AddUserTenantGuidToExistingTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250530032952_AddUserTenantGuidToExistingTables', N'8.0.21');
END;
GO

COMMIT;
GO

