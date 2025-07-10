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
    CONSTRAINT [PK_Customers] PRIMARY KEY ([CustomerId])
);
GO

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
    CONSTRAINT [PK_ServiceCenters] PRIMARY KEY ([Station_id])
);
GO

CREATE TABLE [UserRoles] (
    [UserRoleId] int NOT NULL IDENTITY,
    [UserRoleName] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserRoleId])
);
GO

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
GO

CREATE TABLE [ServiceCenterCheckInPoints] (
    [StationId] int NOT NULL IDENTITY,
    [Station_id] int NOT NULL,
    [Name] nvarchar(50) NULL,
    CONSTRAINT [PK_ServiceCenterCheckInPoints] PRIMARY KEY ([StationId]),
    CONSTRAINT [FK_ServiceCenterCheckInPoints_ServiceCenters_Station_id] FOREIGN KEY ([Station_id]) REFERENCES [ServiceCenters] ([Station_id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Services] (
    [ServiceId] int NOT NULL IDENTITY,
    [ServiceName] nvarchar(100) NOT NULL,
    [Description] nvarchar(255) NULL,
    [BasePrice] decimal(10,2) NULL,
    [LoyaltyPoints] int NULL,
    [Station_id] int NOT NULL,
    CONSTRAINT [PK_Services] PRIMARY KEY ([ServiceId]),
    CONSTRAINT [FK_Services_ServiceCenters_Station_id] FOREIGN KEY ([Station_id]) REFERENCES [ServiceCenters] ([Station_id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Users] (
    [UserId] int NOT NULL IDENTITY,
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NOT NULL,
    [Email] nvarchar(150) NOT NULL,
    [Password] nvarchar(max) NOT NULL,
    [UserRoleId] int NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserId]),
    CONSTRAINT [FK_Users_UserRoles_UserRoleId] FOREIGN KEY ([UserRoleId]) REFERENCES [UserRoles] ([UserRoleId]) ON DELETE CASCADE
);
GO

CREATE TABLE [BorderPoints] (
    [PointId] int NOT NULL IDENTITY,
    [VehicleId] int NOT NULL,
    [CheckPoint] nvarchar(100) NULL,
    [CheckDate] datetime2 NULL,
    [EntryPoint] nvarchar(20) NULL,
    CONSTRAINT [PK_BorderPoints] PRIMARY KEY ([PointId]),
    CONSTRAINT [FK_BorderPoints_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE CASCADE
);
GO

CREATE TABLE [Documents] (
    [DocumentId] int NOT NULL IDENTITY,
    [VehicleId] int NOT NULL,
    [DocumentType] nvarchar(100) NULL,
    [ExpiryDate] datetime2 NULL,
    [UploadDate] datetime2 NOT NULL,
    [FilePath] nvarchar(255) NULL,
    CONSTRAINT [PK_Documents] PRIMARY KEY ([DocumentId]),
    CONSTRAINT [FK_Documents_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE CASCADE
);
GO

CREATE TABLE [Invoices] (
    [InvoiceId] int NOT NULL IDENTITY,
    [VehicleId] int NOT NULL,
    [TotalCost] decimal(10,2) NULL,
    [InvoiceDate] datetime2 NULL,
    CONSTRAINT [PK_Invoices] PRIMARY KEY ([InvoiceId]),
    CONSTRAINT [FK_Invoices_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE CASCADE
);
GO

CREATE TABLE [Appointments] (
    [AppointmentId] int NOT NULL IDENTITY,
    [VehicleId] int NOT NULL,
    [ServiceId] int NOT NULL,
    [CustomerId] int NOT NULL,
    [AppointmentDate] datetime2 NULL,
    [Status] nvarchar(20) NULL,
    [Type] nvarchar(20) NULL,
    [Description] nvarchar(255) NULL,
    CONSTRAINT [PK_Appointments] PRIMARY KEY ([AppointmentId]),
    CONSTRAINT [FK_Appointments_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Appointments_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([ServiceId]),
    CONSTRAINT [FK_Appointments_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId])
);
GO

CREATE TABLE [PaymentLogs] (
    [LogId] int NOT NULL IDENTITY,
    [InvoiceId] int NOT NULL,
    [PaymentDate] datetime2 NULL,
    [Status] nvarchar(20) NULL,
    CONSTRAINT [PK_PaymentLogs] PRIMARY KEY ([LogId]),
    CONSTRAINT [FK_PaymentLogs_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([InvoiceId]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Appointments_CustomerId] ON [Appointments] ([CustomerId]);
GO

CREATE INDEX [IX_Appointments_ServiceId] ON [Appointments] ([ServiceId]);
GO

CREATE INDEX [IX_Appointments_VehicleId] ON [Appointments] ([VehicleId]);
GO

CREATE INDEX [IX_BorderPoints_VehicleId] ON [BorderPoints] ([VehicleId]);
GO

CREATE INDEX [IX_Documents_VehicleId] ON [Documents] ([VehicleId]);
GO

CREATE INDEX [IX_Invoices_VehicleId] ON [Invoices] ([VehicleId]);
GO

CREATE INDEX [IX_PaymentLogs_InvoiceId] ON [PaymentLogs] ([InvoiceId]);
GO

CREATE INDEX [IX_ServiceCenterCheckInPoints_Station_id] ON [ServiceCenterCheckInPoints] ([Station_id]);
GO

CREATE INDEX [IX_Services_Station_id] ON [Services] ([Station_id]);
GO

CREATE INDEX [IX_Users_UserRoleId] ON [Users] ([UserRoleId]);
GO

CREATE INDEX [IX_Vehicles_CustomerId] ON [Vehicles] ([CustomerId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250709065437_InitialCreate', N'8.0.18');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [VehicleServiceHistory] (
    [ServiceHistoryId] int NOT NULL IDENTITY,
    [VehicleId] int NOT NULL,
    [ServiceDate] datetime2 NOT NULL,
    [ServiceType] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [Cost] decimal(18,2) NOT NULL,
    [ServiceCenterId] int NOT NULL,
    [ServicedByUserId] int NULL,
    CONSTRAINT [PK_VehicleServiceHistory] PRIMARY KEY ([ServiceHistoryId]),
    CONSTRAINT [FK_VehicleServiceHistory_ServiceCenters_ServiceCenterId] FOREIGN KEY ([ServiceCenterId]) REFERENCES [ServiceCenters] ([Station_id]) ON DELETE CASCADE,
    CONSTRAINT [FK_VehicleServiceHistory_Users_ServicedByUserId] FOREIGN KEY ([ServicedByUserId]) REFERENCES [Users] ([UserId]),
    CONSTRAINT [FK_VehicleServiceHistory_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_VehicleServiceHistory_ServiceCenterId] ON [VehicleServiceHistory] ([ServiceCenterId]);
GO

CREATE INDEX [IX_VehicleServiceHistory_ServicedByUserId] ON [VehicleServiceHistory] ([ServicedByUserId]);
GO

CREATE INDEX [IX_VehicleServiceHistory_VehicleId] ON [VehicleServiceHistory] ([VehicleId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250709092044_AddVehicleServiceHistory', N'8.0.18');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [ServiceCenters] ADD [Latitude] float NULL;
GO

ALTER TABLE [ServiceCenters] ADD [Longitude] float NULL;
GO

DROP INDEX [IX_Appointments_ServiceId] ON [Appointments];
GO

ALTER TABLE [Appointments] DROP CONSTRAINT [FK_Appointments_Services_ServiceId];
GO

CREATE TABLE [AppointmentServices] (
    [AppointmentServiceId] int NOT NULL IDENTITY,
    [AppointmentId] int NOT NULL,
    [ServiceId] int NOT NULL,
    [ServicePrice] decimal(10, 2) NOT NULL,
    CONSTRAINT [PK_AppointmentServices] PRIMARY KEY ([AppointmentServiceId]),
    CONSTRAINT [FK_AppointmentServices_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([AppointmentId]) ON DELETE CASCADE,
    CONSTRAINT [FK_AppointmentServices_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([ServiceId])
);
GO

ALTER TABLE [Appointments] ADD [ServiceCenterId] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Appointments] ADD [EstimatedTotalCost] decimal(10, 2) NOT NULL DEFAULT 0.0;
GO

ALTER TABLE [Appointments] ADD [AdvancePaymentAmount] decimal(10, 2) NOT NULL DEFAULT 0.0;
GO

ALTER TABLE [Appointments] ADD [ActualTotalCost] decimal(10, 2) NULL;
GO

ALTER TABLE [Appointments] ADD [IsAdvancePaymentCompleted] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Appointments]') AND [c].[name] = N'Status');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Appointments] DROP CONSTRAINT [' + @var0 + '];');
UPDATE [Appointments] SET [Status] = N'Pending' WHERE [Status] IS NULL;
ALTER TABLE [Appointments] ALTER COLUMN [Status] nvarchar(20) NOT NULL;
ALTER TABLE [Appointments] ADD DEFAULT N'Pending' FOR [Status];
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Appointments]') AND [c].[name] = N'AppointmentDate');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Appointments] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Appointments] ALTER COLUMN [AppointmentDate] datetime2 NOT NULL;
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Appointments]') AND [c].[name] = N'ServiceId');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Appointments] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Appointments] DROP COLUMN [ServiceId];
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Appointments]') AND [c].[name] = N'Type');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Appointments] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Appointments] DROP COLUMN [Type];
GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaymentLogs]') AND [c].[name] = N'InvoiceId');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [PaymentLogs] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [PaymentLogs] ALTER COLUMN [InvoiceId] int NULL;
GO

ALTER TABLE [PaymentLogs] ADD [AppointmentId] int NULL;
GO

ALTER TABLE [PaymentLogs] ADD [Amount] decimal(10, 2) NOT NULL DEFAULT 0.0;
GO

ALTER TABLE [PaymentLogs] ADD [PaymentMethod] nvarchar(50) NULL;
GO

ALTER TABLE [PaymentLogs] ADD [PaymentType] nvarchar(50) NULL;
GO

ALTER TABLE [PaymentLogs] ADD [TransactionReference] nvarchar(100) NULL;
GO

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaymentLogs]') AND [c].[name] = N'Status');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [PaymentLogs] DROP CONSTRAINT [' + @var5 + '];');
UPDATE [PaymentLogs] SET [Status] = N'Pending' WHERE [Status] IS NULL;
ALTER TABLE [PaymentLogs] ALTER COLUMN [Status] nvarchar(20) NOT NULL;
ALTER TABLE [PaymentLogs] ADD DEFAULT N'Pending' FOR [Status];
GO

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaymentLogs]') AND [c].[name] = N'PaymentDate');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [PaymentLogs] DROP CONSTRAINT [' + @var6 + '];');
UPDATE [PaymentLogs] SET [PaymentDate] = GETDATE() WHERE [PaymentDate] IS NULL;
ALTER TABLE [PaymentLogs] ALTER COLUMN [PaymentDate] datetime2 NOT NULL;
ALTER TABLE [PaymentLogs] ADD DEFAULT (GETDATE()) FOR [PaymentDate];
GO

ALTER TABLE [Appointments] ADD CONSTRAINT [FK_Appointments_ServiceCenters_ServiceCenterId] FOREIGN KEY ([ServiceCenterId]) REFERENCES [ServiceCenters] ([Station_id]);
GO

ALTER TABLE [PaymentLogs] ADD CONSTRAINT [FK_PaymentLogs_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([AppointmentId]) ON DELETE SET NULL;
GO

CREATE INDEX [IX_Appointments_ServiceCenterId] ON [Appointments] ([ServiceCenterId]);
GO

CREATE INDEX [IX_AppointmentServices_AppointmentId] ON [AppointmentServices] ([AppointmentId]);
GO

CREATE INDEX [IX_AppointmentServices_ServiceId] ON [AppointmentServices] ([ServiceId]);
GO

CREATE INDEX [IX_PaymentLogs_AppointmentId] ON [PaymentLogs] ([AppointmentId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250709104530_AddAppointmentBookingSystem', N'8.0.18');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

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
    CONSTRAINT [PK_Customers] PRIMARY KEY ([CustomerId])
);
GO

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
    [Latitude] float NULL,
    [Longitude] float NULL,
    CONSTRAINT [PK_ServiceCenters] PRIMARY KEY ([Station_id])
);
GO

CREATE TABLE [UserRoles] (
    [UserRoleId] int NOT NULL IDENTITY,
    [UserRoleName] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserRoleId])
);
GO

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
GO

CREATE TABLE [ServiceCenterCheckInPoints] (
    [StationId] int NOT NULL IDENTITY,
    [Station_id] int NOT NULL,
    [Name] nvarchar(50) NULL,
    CONSTRAINT [PK_ServiceCenterCheckInPoints] PRIMARY KEY ([StationId]),
    CONSTRAINT [FK_ServiceCenterCheckInPoints_ServiceCenters_Station_id] FOREIGN KEY ([Station_id]) REFERENCES [ServiceCenters] ([Station_id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Services] (
    [ServiceId] int NOT NULL IDENTITY,
    [ServiceName] nvarchar(100) NOT NULL,
    [Description] nvarchar(255) NULL,
    [BasePrice] decimal(10,2) NULL,
    [LoyaltyPoints] int NULL,
    [Station_id] int NOT NULL,
    CONSTRAINT [PK_Services] PRIMARY KEY ([ServiceId]),
    CONSTRAINT [FK_Services_ServiceCenters_Station_id] FOREIGN KEY ([Station_id]) REFERENCES [ServiceCenters] ([Station_id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Users] (
    [UserId] int NOT NULL IDENTITY,
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NOT NULL,
    [Email] nvarchar(150) NOT NULL,
    [Password] nvarchar(max) NOT NULL,
    [UserRoleId] int NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserId]),
    CONSTRAINT [FK_Users_UserRoles_UserRoleId] FOREIGN KEY ([UserRoleId]) REFERENCES [UserRoles] ([UserRoleId]) ON DELETE CASCADE
);
GO

CREATE TABLE [BorderPoints] (
    [PointId] int NOT NULL IDENTITY,
    [VehicleId] int NOT NULL,
    [CheckPoint] nvarchar(100) NULL,
    [CheckDate] datetime2 NULL,
    [EntryPoint] nvarchar(20) NULL,
    CONSTRAINT [PK_BorderPoints] PRIMARY KEY ([PointId]),
    CONSTRAINT [FK_BorderPoints_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE CASCADE
);
GO

CREATE TABLE [Documents] (
    [DocumentId] int NOT NULL IDENTITY,
    [VehicleId] int NOT NULL,
    [DocumentType] nvarchar(100) NULL,
    [ExpiryDate] datetime2 NULL,
    [UploadDate] datetime2 NOT NULL,
    [FilePath] nvarchar(255) NULL,
    CONSTRAINT [PK_Documents] PRIMARY KEY ([DocumentId]),
    CONSTRAINT [FK_Documents_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE CASCADE
);
GO

CREATE TABLE [Invoices] (
    [InvoiceId] int NOT NULL IDENTITY,
    [VehicleId] int NOT NULL,
    [TotalCost] decimal(10,2) NULL,
    [InvoiceDate] datetime2 NULL,
    CONSTRAINT [PK_Invoices] PRIMARY KEY ([InvoiceId]),
    CONSTRAINT [FK_Invoices_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE CASCADE
);
GO

CREATE TABLE [Appointments] (
    [AppointmentId] int NOT NULL IDENTITY,
    [VehicleId] int NOT NULL,
    [CustomerId] int NOT NULL,
    [ServiceCenterId] int NOT NULL,
    [AppointmentDate] datetime2 NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [Description] nvarchar(255) NULL,
    [EstimatedTotalCost] decimal(10,2) NOT NULL,
    [AdvancePaymentAmount] decimal(10,2) NOT NULL,
    [ActualTotalCost] decimal(10,2) NULL,
    [IsAdvancePaymentCompleted] bit NOT NULL,
    [ServiceId] int NULL,
    CONSTRAINT [PK_Appointments] PRIMARY KEY ([AppointmentId]),
    CONSTRAINT [FK_Appointments_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([CustomerId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Appointments_ServiceCenters_ServiceCenterId] FOREIGN KEY ([ServiceCenterId]) REFERENCES [ServiceCenters] ([Station_id]),
    CONSTRAINT [FK_Appointments_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([ServiceId]),
    CONSTRAINT [FK_Appointments_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId])
);
GO

CREATE TABLE [VehicleServiceHistory] (
    [ServiceHistoryId] int NOT NULL IDENTITY,
    [VehicleId] int NOT NULL,
    [ServiceDate] datetime2 NOT NULL,
    [ServiceType] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [Cost] decimal(10,2) NOT NULL,
    [ServiceCenterId] int NOT NULL,
    [ServicedByUserId] int NULL,
    CONSTRAINT [PK_VehicleServiceHistory] PRIMARY KEY ([ServiceHistoryId]),
    CONSTRAINT [FK_VehicleServiceHistory_ServiceCenters_ServiceCenterId] FOREIGN KEY ([ServiceCenterId]) REFERENCES [ServiceCenters] ([Station_id]) ON DELETE CASCADE,
    CONSTRAINT [FK_VehicleServiceHistory_Users_ServicedByUserId] FOREIGN KEY ([ServicedByUserId]) REFERENCES [Users] ([UserId]),
    CONSTRAINT [FK_VehicleServiceHistory_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [Vehicles] ([VehicleId]) ON DELETE CASCADE
);
GO

CREATE TABLE [AppointmentServices] (
    [AppointmentServiceId] int NOT NULL IDENTITY,
    [AppointmentId] int NOT NULL,
    [ServiceId] int NOT NULL,
    [ServicePrice] decimal(10,2) NOT NULL,
    CONSTRAINT [PK_AppointmentServices] PRIMARY KEY ([AppointmentServiceId]),
    CONSTRAINT [FK_AppointmentServices_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([AppointmentId]) ON DELETE CASCADE,
    CONSTRAINT [FK_AppointmentServices_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([ServiceId])
);
GO

CREATE TABLE [PaymentLogs] (
    [LogId] int NOT NULL IDENTITY,
    [InvoiceId] int NULL,
    [AppointmentId] int NULL,
    [PaymentDate] datetime2 NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [Amount] decimal(10,2) NOT NULL,
    [PaymentMethod] nvarchar(50) NULL,
    [TransactionReference] nvarchar(100) NULL,
    [PaymentType] nvarchar(50) NULL,
    CONSTRAINT [PK_PaymentLogs] PRIMARY KEY ([LogId]),
    CONSTRAINT [FK_PaymentLogs_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([AppointmentId]) ON DELETE SET NULL,
    CONSTRAINT [FK_PaymentLogs_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([InvoiceId])
);
GO

CREATE INDEX [IX_Appointments_CustomerId] ON [Appointments] ([CustomerId]);
GO

CREATE INDEX [IX_Appointments_ServiceCenterId] ON [Appointments] ([ServiceCenterId]);
GO

CREATE INDEX [IX_Appointments_ServiceId] ON [Appointments] ([ServiceId]);
GO

CREATE INDEX [IX_Appointments_VehicleId] ON [Appointments] ([VehicleId]);
GO

CREATE INDEX [IX_AppointmentServices_AppointmentId] ON [AppointmentServices] ([AppointmentId]);
GO

CREATE INDEX [IX_AppointmentServices_ServiceId] ON [AppointmentServices] ([ServiceId]);
GO

CREATE INDEX [IX_BorderPoints_VehicleId] ON [BorderPoints] ([VehicleId]);
GO

CREATE INDEX [IX_Documents_VehicleId] ON [Documents] ([VehicleId]);
GO

CREATE INDEX [IX_Invoices_VehicleId] ON [Invoices] ([VehicleId]);
GO

CREATE INDEX [IX_PaymentLogs_AppointmentId] ON [PaymentLogs] ([AppointmentId]);
GO

CREATE INDEX [IX_PaymentLogs_InvoiceId] ON [PaymentLogs] ([InvoiceId]);
GO

CREATE INDEX [IX_ServiceCenterCheckInPoints_Station_id] ON [ServiceCenterCheckInPoints] ([Station_id]);
GO

CREATE INDEX [IX_Services_Station_id] ON [Services] ([Station_id]);
GO

CREATE INDEX [IX_Users_UserRoleId] ON [Users] ([UserRoleId]);
GO

CREATE INDEX [IX_Vehicles_CustomerId] ON [Vehicles] ([CustomerId]);
GO

CREATE INDEX [IX_VehicleServiceHistory_ServiceCenterId] ON [VehicleServiceHistory] ([ServiceCenterId]);
GO

CREATE INDEX [IX_VehicleServiceHistory_ServicedByUserId] ON [VehicleServiceHistory] ([ServicedByUserId]);
GO

CREATE INDEX [IX_VehicleServiceHistory_VehicleId] ON [VehicleServiceHistory] ([VehicleId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250709131940_AddAppointmentControllers', N'8.0.18');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [PaymentLogs] DROP CONSTRAINT [FK_PaymentLogs_Appointments_AppointmentId];
GO

ALTER TABLE [PaymentLogs] ADD CONSTRAINT [FK_PaymentLogs_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([AppointmentId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250709132101_FixForeignKeyConstraint', N'8.0.18');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO


                IF EXISTS (
                    SELECT * FROM sys.foreign_keys 
                    WHERE name = 'FK_PaymentLogs_Appointments_AppointmentId'
                )
                BEGIN
                    ALTER TABLE [PaymentLogs] DROP CONSTRAINT [FK_PaymentLogs_Appointments_AppointmentId]
                END
            
GO

ALTER TABLE [ServiceCenters] ADD [Latitude] float NULL;
GO

ALTER TABLE [ServiceCenters] ADD [Longitude] float NULL;
GO


                IF EXISTS (
                    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'PaymentLogs' AND COLUMN_NAME = 'AppointmentId'
                )
                BEGIN
                    ALTER TABLE [PaymentLogs] ADD CONSTRAINT [FK_PaymentLogs_Appointments_AppointmentId] 
                    FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([AppointmentId]) ON DELETE NO ACTION;
                END
            
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250710000001_AddCoordinatesToServiceCenter', N'8.0.18');
GO

COMMIT;
GO

