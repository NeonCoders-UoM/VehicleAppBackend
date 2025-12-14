-- Script to update ClosureSchedules table schema
-- This manually applies the migration that replaces Day/WeekNumber with ClosureDate

USE VehiclePassportAppNew91;
GO

-- Step 1: Check current table structure
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ClosureSchedules';
GO

-- Step 2: Drop old columns if they exist (Day and WeekNumber)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'ClosureSchedules' AND COLUMN_NAME = 'Day')
BEGIN
    ALTER TABLE ClosureSchedules DROP COLUMN Day;
    PRINT 'Dropped column: Day';
END
ELSE
BEGIN
    PRINT 'Column Day does not exist';
END
GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
           WHERE TABLE_NAME = 'ClosureSchedules' AND COLUMN_NAME = 'WeekNumber')
BEGIN
    ALTER TABLE ClosureSchedules DROP COLUMN WeekNumber;
    PRINT 'Dropped column: WeekNumber';
END
ELSE
BEGIN
    PRINT 'Column WeekNumber does not exist';
END
GO

-- Step 3: Add ClosureDate column if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'ClosureSchedules' AND COLUMN_NAME = 'ClosureDate')
BEGIN
    ALTER TABLE ClosureSchedules 
    ADD ClosureDate datetime2 NOT NULL DEFAULT '2025-01-01';
    PRINT 'Added column: ClosureDate';
END
ELSE
BEGIN
    PRINT 'Column ClosureDate already exists';
END
GO

-- Step 4: Verify the final table structure
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ClosureSchedules';
GO

PRINT 'Schema update complete!';
