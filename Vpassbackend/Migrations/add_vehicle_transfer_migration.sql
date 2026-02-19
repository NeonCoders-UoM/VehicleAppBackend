-- Add VehicleTransfer migration
-- Run this script to add vehicle transfer functionality

-- Step 1: Add Status column to Vehicles table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Vehicles') AND name = 'Status')
BEGIN
    ALTER TABLE Vehicles
    ADD Status NVARCHAR(20) NOT NULL DEFAULT 'Active';
END
GO

-- Step 2: Create VehicleTransfers table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'VehicleTransfers')
BEGIN
    CREATE TABLE VehicleTransfers (
        TransferId INT PRIMARY KEY IDENTITY(1,1),
        VehicleId INT NOT NULL,
        FromOwnerId INT NOT NULL,
        ToOwnerId INT NOT NULL,
        InitiatedAt DATETIME2 NOT NULL,
        CompletedAt DATETIME2 NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        MileageAtTransfer INT NULL,
        SalePrice DECIMAL(10, 2) NULL,
        Notes NVARCHAR(500) NULL,
        ExpiresAt DATETIME2 NOT NULL,
        
        CONSTRAINT FK_VehicleTransfers_Vehicles FOREIGN KEY (VehicleId) 
            REFERENCES Vehicles(VehicleId) ON DELETE CASCADE,
        CONSTRAINT FK_VehicleTransfers_FromOwner FOREIGN KEY (FromOwnerId) 
            REFERENCES Customers(CustomerId) ON DELETE NO ACTION,
        CONSTRAINT FK_VehicleTransfers_ToOwner FOREIGN KEY (ToOwnerId) 
            REFERENCES Customers(CustomerId) ON DELETE NO ACTION
    );
    
    -- Create indexes for better query performance
    CREATE INDEX IX_VehicleTransfers_VehicleId ON VehicleTransfers(VehicleId);
    CREATE INDEX IX_VehicleTransfers_FromOwnerId ON VehicleTransfers(FromOwnerId);
    CREATE INDEX IX_VehicleTransfers_ToOwnerId ON VehicleTransfers(ToOwnerId);
    CREATE INDEX IX_VehicleTransfers_Status ON VehicleTransfers(Status);
    CREATE INDEX IX_VehicleTransfers_ExpiresAt ON VehicleTransfers(ExpiresAt);
    
    PRINT 'VehicleTransfers table created successfully';
END
ELSE
BEGIN
    PRINT 'VehicleTransfers table already exists';
END
GO
