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
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE TABLE [ClosureSchedules] (
        [Id] int NOT NULL IDENTITY,
        [ServiceCenterId] int NOT NULL,
        [WeekNumber] int NOT NULL,
        [Day] nvarchar(max) NULL,
        CONSTRAINT [PK_ClosureSchedules] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE TABLE [Customers] (
        [CustomerId] int NOT NULL IDENTITY,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [Email] nvarchar(150) NOT NULL,
        [Address] nvarchar(200) NOT NULL,
        [PhoneNumber] nvarchar(20) NOT NULL,
        [Password] nvarchar(max) NOT NULL,
        [NIC] nvarchar(20) NOT NULL,
        [LoyaltyPoints] int NOT NULL,
        [IsEmailVerified] bit NOT NULL,
        [OtpCode] nvarchar(max) NULL,
        [OtpExpiry] datetime2 NULL,
        [ForgotPasswordOtp] nvarchar(max) NULL,
        [ForgotPasswordOtpExpiry] datetime2 NULL,
        [DeviceToken] nvarchar(300) NULL,
        [LastTokenUpdate] datetime2 NULL,
        [Platform] nvarchar(50) NULL,
        [PushNotificationsEnabled] bit NOT NULL,
        CONSTRAINT [PK_Customers] PRIMARY KEY ([CustomerId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE TABLE [EmergencyCallCenters] (
        [CenterId] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Address] nvarchar(300) NOT NULL,
        [RegistrationNumber] nvarchar(100) NOT NULL,
        [PhoneNumber] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_EmergencyCallCenters] PRIMARY KEY ([CenterId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE TABLE [Packages] (
        [PackageId] int NOT NULL IDENTITY,
        [PackageName] nvarchar(50) NOT NULL,
        [Percentage] decimal(5,2) NOT NULL,
        [Description] nvarchar(255) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Packages] PRIMARY KEY ([PackageId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE TABLE [ServiceAvailabilities] (
        [Id] int NOT NULL IDENTITY,
        [ServiceCenterId] int NOT NULL,
        [ServiceId] int NOT NULL,
        [WeekNumber] int NOT NULL,
        [Day] nvarchar(max) NULL,
        [IsAvailable] bit NOT NULL,
        CONSTRAINT [PK_ServiceAvailabilities] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE TABLE [ServiceCenters] (
        [Station_id] int NOT NULL IDENTITY,
        [OwnerName] nvarchar(100) NOT NULL,
        [VATNumber] nvarchar(15) NULL,
        [RegisterationNumber] nvarchar(100) NULL,
        [Station_name] nvarchar(150) NULL,
        [Email] nvarchar(100) NULL,
        [Telephone] nvarchar(20) NULL,
        [Address] nvarchar(255) NULL,
        [Station_status] nvarchar(20) NULL,
        [Latitude] float NOT NULL,
        [Longitude] float NOT NULL,
        [DefaultDailyAppointmentLimit] int NOT NULL,
        CONSTRAINT [PK_ServiceCenters] PRIMARY KEY ([Station_id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE TABLE [Services] (
        [ServiceId] int NOT NULL IDENTITY,
        [ServiceName] nvarchar(100) NOT NULL,
        [Description] nvarchar(255) NULL,
        [BasePrice] decimal(10,2) NULL,
        [Category] nvarchar(50) NULL,
        CONSTRAINT [PK_Services] PRIMARY KEY ([ServiceId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE TABLE [UserRoles] (
        [UserRoleId] int NOT NULL IDENTITY,
        [UserRoleName] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserRoleId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE TABLE [Vehicles] (
        [VehicleId] int NOT NULL IDENTITY,
        [RegistrationNumber] nvarchar(50) NOT NULL,
        [CustomerId] int NOT NULL,
        [Brand] nvarchar(100) NULL,
        [Model] nvarchar(100) NULL,
        [ChassisNumber] nvarchar(20) NULL,
        [Mileage] int NULL,
        [Fuel] nvarchar(50) NULL,
        [Year] int NULL,
        CONSTRAINT [PK_Vehicles] PRIMARY KEY ([VehicleId]),
        CONSTRAINT [FK_Vehicles_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE TABLE [ServiceCenterCheckInPoints] (
        [StationId] int NOT NULL IDENTITY,
        [Station_id] int NOT NULL,
        [Name] nvarchar(50) NULL,
        CONSTRAINT [PK_ServiceCenterCheckInPoints] PRIMARY KEY ([StationId]),
        CONSTRAINT [FK_ServiceCenterCheckInPoints_ServiceCenters_Station_id] FOREIGN KEY ([Station_id]) REFERENCES [ServiceCenters] ([Station_id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE TABLE [ServiceCenterDailyLimits] (
        [Id] int NOT NULL IDENTITY,
        [Station_id] int NOT NULL,
        [Date] date NOT NULL,
        [MaxAppointments] int NOT NULL,
        [CurrentAppointments] int NOT NULL,
        CONSTRAINT [PK_ServiceCenterDailyLimits] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ServiceCenterDailyLimits_ServiceCenters_Station_id] FOREIGN KEY ([Station_id]) REFERENCES [ServiceCenters] ([Station_id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE TABLE [ServiceCenterServices] (
        [ServiceCenterServiceId] int NOT NULL IDENTITY,
        [Station_id] int NOT NULL,
        [ServiceId] int NOT NULL,
        [PackageId] int NULL,
        [CustomPrice] decimal(10,2) NULL,
        [BasePrice] decimal(10,2) NULL,
        [LoyaltyPoints] int NULL,
        [IsAvailable] bit NOT NULL,
        [Notes] nvarchar(255) NULL,
        CONSTRAINT [PK_ServiceCenterServices] PRIMARY KEY ([ServiceCenterServiceId]),
        CONSTRAINT [FK_ServiceCenterServices_Packages_PackageId] FOREIGN KEY ([PackageId]) REFERENCES [Packages] ([PackageId]) ON DELETE SET NULL,
        CONSTRAINT [FK_ServiceCenterServices_ServiceCenters_Station_id] FOREIGN KEY ([Station_id]) REFERENCES [ServiceCenters] ([Station_id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ServiceCenterServices_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([ServiceId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [UserId] int NOT NULL IDENTITY,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [Email] nvarchar(150) NOT NULL,
        [Password] nvarchar(max) NOT NULL,
        [UserRoleId] int NOT NULL,
        [Station_id] int NULL,
        [ForgotPasswordOtp] nvarchar(max) NULL,
        [ForgotPasswordOtpExpiry] datetime2 NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([UserId]),
        CONSTRAINT [FK_Users_ServiceCenters_Station_id] FOREIGN KEY ([Station_id]) REFERENCES [ServiceCenters] ([Station_id]),
        CONSTRAINT [FK_Users_UserRoles_UserRoleId] FOREIGN KEY ([UserRoleId]) REFERENCES [UserRoles] ([UserRoleId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE TABLE [Appointments] (
        [AppointmentId] int NOT NULL IDENTITY,
        [VehicleId] int NOT NULL,
        [Station_id] int NOT NULL,
        [CustomerId] int NOT NULL,
        [AppointmentDate] datetime2 NULL,
        [Status] nvarchar(20) NULL,
        [Type] nvarchar(20) NULL,
        [Description] nvarchar(255) NULL,
        [AppointmentPrice] decimal(10,2) NULL,
        [ServiceId] int NULL,
        CONSTRAINT [PK_Appointments] PRIMARY KEY ([AppointmentId]),
        CONSTRAINT [FK_Appointments_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Appointments_ServiceCenters_Station_id] FOREIGN KEY ([Station_id]) REFERENCES [ServiceCenters] ([Station_id]),
        CONSTRAINT [FK_Appointments_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([ServiceId]),
        CONSTRAINT [FK_Appointments_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE TABLE [BorderPoints] (
        [PointId] int NOT NULL IDENTITY,
        [VehicleId] int NOT NULL,
        [CheckPoint] nvarchar(100) NULL,
        [CheckDate] datetime2 NULL,
        [EntryPoint] nvarchar(20) NULL,
        CONSTRAINT [PK_BorderPoints] PRIMARY KEY ([PointId]),
        CONSTRAINT [FK_BorderPoints_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE TABLE [Documents] (
        [DocumentId] int NOT NULL IDENTITY,
        [FileName] nvarchar(255) NOT NULL,
        [FileUrl] nvarchar(max) NOT NULL,
        [DocumentType] int NOT NULL,
        [ContentType] nvarchar(max) NOT NULL,
        [DisplayName] nvarchar(max) NULL,
        [FileSize] bigint NOT NULL,
        [UploadedAt] datetime2 NOT NULL,
        [ExpirationDate] datetime2 NULL,
        [CustomerId] int NOT NULL,
        [VehicleId] int NULL,
        CONSTRAINT [PK_Documents] PRIMARY KEY ([DocumentId]),
        CONSTRAINT [FK_Documents_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Documents_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE TABLE [Feedbacks] (
        [FeedbackId] int NOT NULL IDENTITY,
        [CustomerId] int NOT NULL,
        [ServiceCenterId] int NOT NULL,
        [VehicleId] int NOT NULL,
        [Rating] int NOT NULL,
        [Comments] nvarchar(1000) NULL,
        [FeedbackDate] datetime2 NOT NULL,
        [ServiceDate] nvarchar(50) NULL,
        CONSTRAINT [PK_Feedbacks] PRIMARY KEY ([FeedbackId]),
        CONSTRAINT [FK_Feedbacks_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Feedbacks_ServiceCenters_ServiceCenterId] FOREIGN KEY ([ServiceCenterId]) REFERENCES [ServiceCenters] ([Station_id]),
        CONSTRAINT [FK_Feedbacks_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE TABLE [FuelEfficiencies] (
        [FuelEfficiencyId] int NOT NULL IDENTITY,
        [VehicleId] int NOT NULL,
        [FuelAmount] decimal(10,2) NOT NULL,
        [Date] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_FuelEfficiencies] PRIMARY KEY ([FuelEfficiencyId]),
        CONSTRAINT [FK_FuelEfficiencies_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE TABLE [Invoices] (
        [InvoiceId] int NOT NULL IDENTITY,
        [VehicleId] int NOT NULL,
        [TotalCost] decimal(10,2) NULL,
        [InvoiceDate] datetime2 NULL,
        CONSTRAINT [PK_Invoices] PRIMARY KEY ([InvoiceId]),
        CONSTRAINT [FK_Invoices_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE TABLE [ServiceReminders] (
        [ServiceReminderId] int NOT NULL IDENTITY,
        [VehicleId] int NOT NULL,
        [ServiceId] int NOT NULL,
        [ReminderDate] datetime2 NOT NULL,
        [IntervalMonths] int NOT NULL,
        [NotifyBeforeDays] int NOT NULL,
        [Notes] nvarchar(255) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ServiceReminders] PRIMARY KEY ([ServiceReminderId]),
        CONSTRAINT [FK_ServiceReminders_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([ServiceId]) ON DELETE CASCADE,
        CONSTRAINT [FK_ServiceReminders_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE TABLE [VehicleServiceHistories] (
        [ServiceHistoryId] int NOT NULL IDENTITY,
        [VehicleId] int NOT NULL,
        [ServiceType] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [Cost] decimal(10,2) NOT NULL,
        [ServiceCenterId] int NULL,
        [ServicedByUserId] int NULL,
        [ServiceDate] datetime2 NOT NULL,
        [Mileage] int NULL,
        [IsVerified] bit NOT NULL,
        [ExternalServiceCenterName] nvarchar(150) NULL,
        [ReceiptDocumentPath] nvarchar(255) NULL,
        CONSTRAINT [PK_VehicleServiceHistories] PRIMARY KEY ([ServiceHistoryId]),
        CONSTRAINT [FK_VehicleServiceHistories_ServiceCenters_ServiceCenterId] FOREIGN KEY ([ServiceCenterId]) REFERENCES [ServiceCenters] ([Station_id]),
        CONSTRAINT [FK_VehicleServiceHistories_Users_ServicedByUserId] FOREIGN KEY ([ServicedByUserId]) REFERENCES [Users] ([UserId]),
        CONSTRAINT [FK_VehicleServiceHistories_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE TABLE [AppointmentServices] (
        [AppointmentId] int NOT NULL,
        [ServiceId] int NOT NULL,
        [ServicePrice] decimal(18,2) NULL,
        CONSTRAINT [PK_AppointmentServices] PRIMARY KEY ([AppointmentId], [ServiceId]),
        CONSTRAINT [FK_AppointmentServices_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([AppointmentId]) ON DELETE CASCADE,
        CONSTRAINT [FK_AppointmentServices_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([ServiceId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE TABLE [PaymentLogs] (
        [LogId] int NOT NULL IDENTITY,
        [InvoiceId] int NOT NULL,
        [PaymentDate] datetime2 NULL,
        [Status] nvarchar(20) NULL,
        CONSTRAINT [PK_PaymentLogs] PRIMARY KEY ([LogId]),
        CONSTRAINT [FK_PaymentLogs_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([InvoiceId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE TABLE [Notifications] (
        [NotificationId] int NOT NULL IDENTITY,
        [CustomerId] int NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Message] nvarchar(1000) NOT NULL,
        [Type] nvarchar(50) NOT NULL,
        [Priority] nvarchar(20) NOT NULL,
        [PriorityColor] nvarchar(7) NOT NULL,
        [IsRead] bit NOT NULL,
        [ReadAt] datetime2 NULL,
        [SentAt] datetime2 NULL,
        [ScheduledFor] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [ServiceReminderId] int NULL,
        [VehicleId] int NULL,
        [AppointmentId] int NULL,
        [VehicleRegistrationNumber] nvarchar(20) NULL,
        [VehicleBrand] nvarchar(100) NULL,
        [VehicleModel] nvarchar(100) NULL,
        [ServiceName] nvarchar(200) NULL,
        [CustomerName] nvarchar(150) NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([NotificationId]),
        CONSTRAINT [FK_Notifications_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([AppointmentId]),
        CONSTRAINT [FK_Notifications_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]),
        CONSTRAINT [FK_Notifications_ServiceReminders_ServiceReminderId] FOREIGN KEY ([ServiceReminderId]) REFERENCES [ServiceReminders] ([ServiceReminderId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Notifications_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_Appointments_CustomerId] ON [Appointments] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_Appointments_ServiceId] ON [Appointments] ([ServiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_Appointments_Station_id] ON [Appointments] ([Station_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_Appointments_VehicleId] ON [Appointments] ([VehicleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_AppointmentServices_ServiceId] ON [AppointmentServices] ([ServiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_BorderPoints_VehicleId] ON [BorderPoints] ([VehicleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_Documents_CustomerId] ON [Documents] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_Documents_VehicleId] ON [Documents] ([VehicleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_Feedbacks_CustomerId] ON [Feedbacks] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_Feedbacks_ServiceCenterId] ON [Feedbacks] ([ServiceCenterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_Feedbacks_VehicleId] ON [Feedbacks] ([VehicleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_FuelEfficiencies_VehicleId] ON [FuelEfficiencies] ([VehicleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_Invoices_VehicleId] ON [Invoices] ([VehicleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_AppointmentId] ON [Notifications] ([AppointmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_CustomerId] ON [Notifications] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_ServiceReminderId] ON [Notifications] ([ServiceReminderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_Notifications_VehicleId] ON [Notifications] ([VehicleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_PaymentLogs_InvoiceId] ON [PaymentLogs] ([InvoiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_ServiceCenterCheckInPoints_Station_id] ON [ServiceCenterCheckInPoints] ([Station_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_ServiceCenterDailyLimits_Station_id] ON [ServiceCenterDailyLimits] ([Station_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_ServiceCenterServices_PackageId] ON [ServiceCenterServices] ([PackageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ServiceCenterServices_ServiceId_Station_id] ON [ServiceCenterServices] ([ServiceId], [Station_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_ServiceCenterServices_Station_id] ON [ServiceCenterServices] ([Station_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_ServiceReminders_ServiceId] ON [ServiceReminders] ([ServiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_ServiceReminders_VehicleId] ON [ServiceReminders] ([VehicleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_Users_Station_id] ON [Users] ([Station_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_Users_UserRoleId] ON [Users] ([UserRoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_Vehicles_CustomerId] ON [Vehicles] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_VehicleServiceHistories_ServiceCenterId] ON [VehicleServiceHistories] ([ServiceCenterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_VehicleServiceHistories_ServicedByUserId] ON [VehicleServiceHistories] ([ServicedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    CREATE INDEX [IX_VehicleServiceHistories_VehicleId] ON [VehicleServiceHistories] ([VehicleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250727153500_InitailCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250727153500_InitailCreate', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250904162227_UpdateClosureScheduleToDate'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ClosureSchedules]') AND [c].[name] = N'Day');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [ClosureSchedules] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [ClosureSchedules] DROP COLUMN [Day];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250904162227_UpdateClosureScheduleToDate'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ClosureSchedules]') AND [c].[name] = N'WeekNumber');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [ClosureSchedules] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [ClosureSchedules] DROP COLUMN [WeekNumber];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250904162227_UpdateClosureScheduleToDate'
)
BEGIN
    ALTER TABLE [ClosureSchedules] ADD [ClosureDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250904162227_UpdateClosureScheduleToDate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250904162227_UpdateClosureScheduleToDate', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909053722_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250909053722_InitialCreate', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909065639_RemoveDefaultClosureDate'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ClosureSchedules]') AND [c].[name] = N'ClosureDate');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [ClosureSchedules] DROP CONSTRAINT [' + @var2 + '];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909065639_RemoveDefaultClosureDate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250909065639_RemoveDefaultClosureDate', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909074657_UpdateServiceAvailabilityToDateBased'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceAvailabilities]') AND [c].[name] = N'Day');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [ServiceAvailabilities] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [ServiceAvailabilities] DROP COLUMN [Day];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909074657_UpdateServiceAvailabilityToDateBased'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceAvailabilities]') AND [c].[name] = N'WeekNumber');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [ServiceAvailabilities] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [ServiceAvailabilities] DROP COLUMN [WeekNumber];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909074657_UpdateServiceAvailabilityToDateBased'
)
BEGIN
    ALTER TABLE [ServiceAvailabilities] ADD [Date] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909074657_UpdateServiceAvailabilityToDateBased'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250909074657_UpdateServiceAvailabilityToDateBased', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909130000_AddDateToServiceAvailability'
)
BEGIN

                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                               WHERE TABLE_NAME = 'ServiceAvailabilities' AND COLUMN_NAME = 'Day')
                    BEGIN
                        DECLARE @var0 sysname;
                        SELECT @var0 = [d].[name]
                        FROM [sys].[default_constraints] [d]
                        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceAvailabilities]') AND [c].[name] = N'Day');
                        IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [ServiceAvailabilities] DROP CONSTRAINT [' + @var0 + '];');
                        ALTER TABLE [ServiceAvailabilities] DROP COLUMN [Day];
                    END
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909130000_AddDateToServiceAvailability'
)
BEGIN

                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                               WHERE TABLE_NAME = 'ServiceAvailabilities' AND COLUMN_NAME = 'WeekNumber')
                    BEGIN
                        DECLARE @var1 sysname;
                        SELECT @var1 = [d].[name]
                        FROM [sys].[default_constraints] [d]
                        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ServiceAvailabilities]') AND [c].[name] = N'WeekNumber');
                        IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [ServiceAvailabilities] DROP CONSTRAINT [' + @var1 + '];');
                        ALTER TABLE [ServiceAvailabilities] DROP COLUMN [WeekNumber];
                    END
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909130000_AddDateToServiceAvailability'
)
BEGIN

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                                   WHERE TABLE_NAME = 'ServiceAvailabilities' AND COLUMN_NAME = 'Date')
                    BEGIN
                        ALTER TABLE [ServiceAvailabilities] ADD [Date] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
                    END
                
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250909130000_AddDateToServiceAvailability'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250909130000_AddDateToServiceAvailability', N'8.0.24');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE TABLE [ClosureSchedules] (
        [Id] int NOT NULL IDENTITY,
        [ServiceCenterId] int NOT NULL,
        [ClosureDate] datetime2 NOT NULL,
        CONSTRAINT [PK_ClosureSchedules] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE TABLE [Customers] (
        [CustomerId] int NOT NULL IDENTITY,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [Email] nvarchar(150) NOT NULL,
        [Address] nvarchar(200) NOT NULL,
        [PhoneNumber] nvarchar(20) NOT NULL,
        [Password] nvarchar(max) NOT NULL,
        [NIC] nvarchar(20) NOT NULL,
        [LoyaltyPoints] int NOT NULL,
        [IsEmailVerified] bit NOT NULL,
        [OtpCode] nvarchar(max) NULL,
        [OtpExpiry] datetime2 NULL,
        [ForgotPasswordOtp] nvarchar(max) NULL,
        [ForgotPasswordOtpExpiry] datetime2 NULL,
        [DeviceToken] nvarchar(300) NULL,
        [LastTokenUpdate] datetime2 NULL,
        [Platform] nvarchar(50) NULL,
        [PushNotificationsEnabled] bit NOT NULL,
        [AuthProvider] nvarchar(50) NOT NULL,
        [GoogleId] nvarchar(200) NULL,
        CONSTRAINT [PK_Customers] PRIMARY KEY ([CustomerId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE TABLE [EmergencyCallCenters] (
        [CenterId] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Address] nvarchar(300) NOT NULL,
        [RegistrationNumber] nvarchar(100) NOT NULL,
        [PhoneNumber] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_EmergencyCallCenters] PRIMARY KEY ([CenterId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE TABLE [KnowledgeDocuments] (
        [DocumentId] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [Category] nvarchar(max) NOT NULL,
        [QdrantId] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_KnowledgeDocuments] PRIMARY KEY ([DocumentId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE TABLE [Packages] (
        [PackageId] int NOT NULL IDENTITY,
        [PackageName] nvarchar(50) NOT NULL,
        [Percentage] decimal(5,2) NOT NULL,
        [Description] nvarchar(255) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Packages] PRIMARY KEY ([PackageId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE TABLE [ServiceCenters] (
        [Station_id] int NOT NULL IDENTITY,
        [OwnerName] nvarchar(100) NOT NULL,
        [VATNumber] nvarchar(15) NULL,
        [RegisterationNumber] nvarchar(100) NULL,
        [Station_name] nvarchar(150) NULL,
        [Email] nvarchar(100) NULL,
        [Telephone] nvarchar(20) NULL,
        [Address] nvarchar(255) NULL,
        [Station_status] nvarchar(20) NULL,
        [Latitude] float NOT NULL,
        [Longitude] float NOT NULL,
        [DefaultDailyAppointmentLimit] int NOT NULL,
        CONSTRAINT [PK_ServiceCenters] PRIMARY KEY ([Station_id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE TABLE [Services] (
        [ServiceId] int NOT NULL IDENTITY,
        [ServiceName] nvarchar(100) NOT NULL,
        [Description] nvarchar(255) NULL,
        [BasePrice] decimal(10,2) NULL,
        [Category] nvarchar(50) NULL,
        CONSTRAINT [PK_Services] PRIMARY KEY ([ServiceId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE TABLE [UserRoles] (
        [UserRoleId] int NOT NULL IDENTITY,
        [UserRoleName] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserRoleId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE TABLE [Vehicles] (
        [VehicleId] int NOT NULL IDENTITY,
        [RegistrationNumber] nvarchar(50) NOT NULL,
        [CustomerId] int NOT NULL,
        [Brand] nvarchar(100) NULL,
        [Model] nvarchar(100) NULL,
        [ChassisNumber] nvarchar(20) NULL,
        [Mileage] int NULL,
        [Fuel] nvarchar(50) NULL,
        [Year] int NULL,
        CONSTRAINT [PK_Vehicles] PRIMARY KEY ([VehicleId]),
        CONSTRAINT [FK_Vehicles_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE TABLE [ServiceCenterCheckInPoints] (
        [StationId] int NOT NULL IDENTITY,
        [Station_id] int NOT NULL,
        [Name] nvarchar(50) NULL,
        CONSTRAINT [PK_ServiceCenterCheckInPoints] PRIMARY KEY ([StationId]),
        CONSTRAINT [FK_ServiceCenterCheckInPoints_ServiceCenters_Station_id] FOREIGN KEY ([Station_id]) REFERENCES [ServiceCenters] ([Station_id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE TABLE [ServiceCenterDailyLimits] (
        [Id] int NOT NULL IDENTITY,
        [Station_id] int NOT NULL,
        [Date] date NOT NULL,
        [MaxAppointments] int NOT NULL,
        [CurrentAppointments] int NOT NULL,
        CONSTRAINT [PK_ServiceCenterDailyLimits] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ServiceCenterDailyLimits_ServiceCenters_Station_id] FOREIGN KEY ([Station_id]) REFERENCES [ServiceCenters] ([Station_id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE TABLE [ServiceCenterServices] (
        [ServiceCenterServiceId] int NOT NULL IDENTITY,
        [Station_id] int NOT NULL,
        [ServiceId] int NOT NULL,
        [PackageId] int NULL,
        [CustomPrice] decimal(10,2) NULL,
        [BasePrice] decimal(10,2) NULL,
        [LoyaltyPoints] int NULL,
        [IsAvailable] bit NOT NULL,
        [Notes] nvarchar(255) NULL,
        CONSTRAINT [PK_ServiceCenterServices] PRIMARY KEY ([ServiceCenterServiceId]),
        CONSTRAINT [FK_ServiceCenterServices_Packages_PackageId] FOREIGN KEY ([PackageId]) REFERENCES [Packages] ([PackageId]) ON DELETE SET NULL,
        CONSTRAINT [FK_ServiceCenterServices_ServiceCenters_Station_id] FOREIGN KEY ([Station_id]) REFERENCES [ServiceCenters] ([Station_id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ServiceCenterServices_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([ServiceId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE TABLE [Users] (
        [UserId] int NOT NULL IDENTITY,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [Email] nvarchar(150) NOT NULL,
        [Password] nvarchar(max) NOT NULL,
        [UserRoleId] int NOT NULL,
        [Station_id] int NULL,
        [ForgotPasswordOtp] nvarchar(max) NULL,
        [ForgotPasswordOtpExpiry] datetime2 NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([UserId]),
        CONSTRAINT [FK_Users_ServiceCenters_Station_id] FOREIGN KEY ([Station_id]) REFERENCES [ServiceCenters] ([Station_id]),
        CONSTRAINT [FK_Users_UserRoles_UserRoleId] FOREIGN KEY ([UserRoleId]) REFERENCES [UserRoles] ([UserRoleId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE TABLE [Appointments] (
        [AppointmentId] int NOT NULL IDENTITY,
        [VehicleId] int NOT NULL,
        [Station_id] int NOT NULL,
        [CustomerId] int NOT NULL,
        [AppointmentDate] datetime2 NULL,
        [Status] nvarchar(20) NULL,
        [Type] nvarchar(20) NULL,
        [Description] nvarchar(255) NULL,
        [AppointmentPrice] decimal(10,2) NULL,
        [ServiceId] int NULL,
        CONSTRAINT [PK_Appointments] PRIMARY KEY ([AppointmentId]),
        CONSTRAINT [FK_Appointments_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Appointments_ServiceCenters_Station_id] FOREIGN KEY ([Station_id]) REFERENCES [ServiceCenters] ([Station_id]),
        CONSTRAINT [FK_Appointments_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([ServiceId]),
        CONSTRAINT [FK_Appointments_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE TABLE [BorderPoints] (
        [PointId] int NOT NULL IDENTITY,
        [VehicleId] int NOT NULL,
        [CheckPoint] nvarchar(100) NULL,
        [CheckDate] datetime2 NULL,
        [EntryPoint] nvarchar(20) NULL,
        CONSTRAINT [PK_BorderPoints] PRIMARY KEY ([PointId]),
        CONSTRAINT [FK_BorderPoints_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE TABLE [Documents] (
        [DocumentId] int NOT NULL IDENTITY,
        [FileName] nvarchar(255) NOT NULL,
        [FileUrl] nvarchar(max) NOT NULL,
        [DocumentType] int NOT NULL,
        [ContentType] nvarchar(max) NOT NULL,
        [DisplayName] nvarchar(max) NULL,
        [FileSize] bigint NOT NULL,
        [UploadedAt] datetime2 NOT NULL,
        [ExpirationDate] datetime2 NULL,
        [CustomerId] int NOT NULL,
        [VehicleId] int NULL,
        CONSTRAINT [PK_Documents] PRIMARY KEY ([DocumentId]),
        CONSTRAINT [FK_Documents_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Documents_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE TABLE [Feedbacks] (
        [FeedbackId] int NOT NULL IDENTITY,
        [CustomerId] int NOT NULL,
        [ServiceCenterId] int NOT NULL,
        [VehicleId] int NOT NULL,
        [Rating] int NOT NULL,
        [Comments] nvarchar(1000) NULL,
        [FeedbackDate] datetime2 NOT NULL,
        [ServiceDate] nvarchar(50) NULL,
        CONSTRAINT [PK_Feedbacks] PRIMARY KEY ([FeedbackId]),
        CONSTRAINT [FK_Feedbacks_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Feedbacks_ServiceCenters_ServiceCenterId] FOREIGN KEY ([ServiceCenterId]) REFERENCES [ServiceCenters] ([Station_id]),
        CONSTRAINT [FK_Feedbacks_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE TABLE [FuelEfficiencies] (
        [FuelEfficiencyId] int NOT NULL IDENTITY,
        [VehicleId] int NOT NULL,
        [FuelAmount] decimal(10,2) NOT NULL,
        [Date] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_FuelEfficiencies] PRIMARY KEY ([FuelEfficiencyId]),
        CONSTRAINT [FK_FuelEfficiencies_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE TABLE [Invoices] (
        [InvoiceId] int NOT NULL IDENTITY,
        [VehicleId] int NOT NULL,
        [TotalCost] decimal(10,2) NULL,
        [InvoiceDate] datetime2 NULL,
        CONSTRAINT [PK_Invoices] PRIMARY KEY ([InvoiceId]),
        CONSTRAINT [FK_Invoices_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE TABLE [ServiceReminders] (
        [ServiceReminderId] int NOT NULL IDENTITY,
        [VehicleId] int NOT NULL,
        [ServiceId] int NOT NULL,
        [ReminderDate] datetime2 NOT NULL,
        [IntervalMonths] int NOT NULL,
        [NotifyBeforeDays] int NOT NULL,
        [Notes] nvarchar(255) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_ServiceReminders] PRIMARY KEY ([ServiceReminderId]),
        CONSTRAINT [FK_ServiceReminders_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([ServiceId]) ON DELETE CASCADE,
        CONSTRAINT [FK_ServiceReminders_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE TABLE [ChatConversations] (
        [ConversationId] int NOT NULL IDENTITY,
        [CustomerId] int NULL,
        [UserId] int NULL,
        [SessionId] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [LastMessageAt] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_ChatConversations] PRIMARY KEY ([ConversationId]),
        CONSTRAINT [FK_ChatConversations_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]),
        CONSTRAINT [FK_ChatConversations_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE TABLE [VehicleServiceHistories] (
        [ServiceHistoryId] int NOT NULL IDENTITY,
        [VehicleId] int NOT NULL,
        [ServiceType] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [Cost] decimal(10,2) NOT NULL,
        [ServiceCenterId] int NULL,
        [ServicedByUserId] int NULL,
        [ServiceDate] datetime2 NOT NULL,
        [Mileage] int NULL,
        [IsVerified] bit NOT NULL,
        [ExternalServiceCenterName] nvarchar(150) NULL,
        [ReceiptDocumentPath] nvarchar(255) NULL,
        CONSTRAINT [PK_VehicleServiceHistories] PRIMARY KEY ([ServiceHistoryId]),
        CONSTRAINT [FK_VehicleServiceHistories_ServiceCenters_ServiceCenterId] FOREIGN KEY ([ServiceCenterId]) REFERENCES [ServiceCenters] ([Station_id]),
        CONSTRAINT [FK_VehicleServiceHistories_Users_ServicedByUserId] FOREIGN KEY ([ServicedByUserId]) REFERENCES [Users] ([UserId]),
        CONSTRAINT [FK_VehicleServiceHistories_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE TABLE [AppointmentServices] (
        [AppointmentId] int NOT NULL,
        [ServiceId] int NOT NULL,
        [ServicePrice] decimal(18,2) NULL,
        CONSTRAINT [PK_AppointmentServices] PRIMARY KEY ([AppointmentId], [ServiceId]),
        CONSTRAINT [FK_AppointmentServices_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([AppointmentId]) ON DELETE CASCADE,
        CONSTRAINT [FK_AppointmentServices_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([ServiceId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE TABLE [PaymentLogs] (
        [LogId] int NOT NULL IDENTITY,
        [InvoiceId] int NOT NULL,
        [PaymentDate] datetime2 NULL,
        [Status] nvarchar(20) NULL,
        CONSTRAINT [PK_PaymentLogs] PRIMARY KEY ([LogId]),
        CONSTRAINT [FK_PaymentLogs_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([InvoiceId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE TABLE [Notifications] (
        [NotificationId] int NOT NULL IDENTITY,
        [CustomerId] int NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Message] nvarchar(1000) NOT NULL,
        [Type] nvarchar(50) NOT NULL,
        [Priority] nvarchar(20) NOT NULL,
        [PriorityColor] nvarchar(7) NOT NULL,
        [IsRead] bit NOT NULL,
        [ReadAt] datetime2 NULL,
        [SentAt] datetime2 NULL,
        [ScheduledFor] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [ServiceReminderId] int NULL,
        [VehicleId] int NULL,
        [AppointmentId] int NULL,
        [VehicleRegistrationNumber] nvarchar(20) NULL,
        [VehicleBrand] nvarchar(100) NULL,
        [VehicleModel] nvarchar(100) NULL,
        [ServiceName] nvarchar(200) NULL,
        [CustomerName] nvarchar(150) NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([NotificationId]),
        CONSTRAINT [FK_Notifications_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([AppointmentId]),
        CONSTRAINT [FK_Notifications_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]),
        CONSTRAINT [FK_Notifications_ServiceReminders_ServiceReminderId] FOREIGN KEY ([ServiceReminderId]) REFERENCES [ServiceReminders] ([ServiceReminderId]) ON DELETE CASCADE,
        CONSTRAINT [FK_Notifications_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE TABLE [ChatMessages] (
        [MessageId] int NOT NULL IDENTITY,
        [ConversationId] int NOT NULL,
        [Role] nvarchar(max) NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ChatMessages] PRIMARY KEY ([MessageId]),
        CONSTRAINT [FK_ChatMessages_ChatConversations_ConversationId] FOREIGN KEY ([ConversationId]) REFERENCES [ChatConversations] ([ConversationId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_Appointments_CustomerId] ON [Appointments] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_Appointments_ServiceId] ON [Appointments] ([ServiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_Appointments_Station_id] ON [Appointments] ([Station_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_Appointments_VehicleId] ON [Appointments] ([VehicleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_AppointmentServices_ServiceId] ON [AppointmentServices] ([ServiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_BorderPoints_VehicleId] ON [BorderPoints] ([VehicleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_ChatConversations_CustomerId] ON [ChatConversations] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_ChatConversations_UserId] ON [ChatConversations] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_ChatMessages_ConversationId] ON [ChatMessages] ([ConversationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_Documents_CustomerId] ON [Documents] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_Documents_VehicleId] ON [Documents] ([VehicleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_Feedbacks_CustomerId] ON [Feedbacks] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_Feedbacks_ServiceCenterId] ON [Feedbacks] ([ServiceCenterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_Feedbacks_VehicleId] ON [Feedbacks] ([VehicleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_FuelEfficiencies_VehicleId] ON [FuelEfficiencies] ([VehicleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_Invoices_VehicleId] ON [Invoices] ([VehicleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_Notifications_AppointmentId] ON [Notifications] ([AppointmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_Notifications_CustomerId] ON [Notifications] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_Notifications_ServiceReminderId] ON [Notifications] ([ServiceReminderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_Notifications_VehicleId] ON [Notifications] ([VehicleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_PaymentLogs_InvoiceId] ON [PaymentLogs] ([InvoiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_ServiceCenterCheckInPoints_Station_id] ON [ServiceCenterCheckInPoints] ([Station_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_ServiceCenterDailyLimits_Station_id] ON [ServiceCenterDailyLimits] ([Station_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_ServiceCenterServices_PackageId] ON [ServiceCenterServices] ([PackageId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ServiceCenterServices_ServiceId_Station_id] ON [ServiceCenterServices] ([ServiceId], [Station_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_ServiceCenterServices_Station_id] ON [ServiceCenterServices] ([Station_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_ServiceReminders_ServiceId] ON [ServiceReminders] ([ServiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_ServiceReminders_VehicleId] ON [ServiceReminders] ([VehicleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_Users_Station_id] ON [Users] ([Station_id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_Users_UserRoleId] ON [Users] ([UserRoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_Vehicles_CustomerId] ON [Vehicles] ([CustomerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_VehicleServiceHistories_ServiceCenterId] ON [VehicleServiceHistories] ([ServiceCenterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_VehicleServiceHistories_ServicedByUserId] ON [VehicleServiceHistories] ([ServicedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    CREATE INDEX [IX_VehicleServiceHistories_VehicleId] ON [VehicleServiceHistories] ([VehicleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260212202402_AddGoogleAuth'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260212202402_AddGoogleAuth', N'8.0.24');
END;
GO

COMMIT;
GO

