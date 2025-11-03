-- Script to remove default value from ClosureDate column
-- Run this in SQL Server Management Studio or similar tool if the migration doesn't apply automatically

USE VehiclePassportAppNew91;

-- First, check if there are any existing records with the default date
SELECT * FROM ClosureSchedules WHERE ClosureDate = '0001-01-01 00:00:00.0000000';

-- If there are records with the default date, you may want to update them first
-- UPDATE ClosureSchedules SET ClosureDate = GETDATE() WHERE ClosureDate = '0001-01-01 00:00:00.0000000';

-- Then alter the column to remove the default constraint
ALTER TABLE ClosureSchedules
ALTER COLUMN ClosureDate datetime2 NOT NULL;

-- Note: This will fail if there are existing records with the default date
-- In that case, update those records first with a proper date
