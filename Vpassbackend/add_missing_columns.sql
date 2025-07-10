-- Script to add missing Latitude and Longitude columns to ServiceCenters table
-- Run this script directly on your database to fix the issue

-- Check if columns exist first
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ServiceCenters' AND COLUMN_NAME = 'Latitude'
)
BEGIN
    -- Add Latitude column
    ALTER TABLE [ServiceCenters] ADD [Latitude] float NULL;
    PRINT 'Added Latitude column to ServiceCenters table';
END
ELSE
BEGIN
    PRINT 'Latitude column already exists';
END

-- Check if Longitude column exists
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ServiceCenters' AND COLUMN_NAME = 'Longitude'
)
BEGIN
    -- Add Longitude column
    ALTER TABLE [ServiceCenters] ADD [Longitude] float NULL;
    PRINT 'Added Longitude column to ServiceCenters table';
END
ELSE
BEGIN
    PRINT 'Longitude column already exists';
END

-- Verify columns were added
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ServiceCenters'
AND COLUMN_NAME IN ('Latitude', 'Longitude');
