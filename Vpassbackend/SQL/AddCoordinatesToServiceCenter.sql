-- Add Latitude and Longitude columns to the ServiceCenters table if they don't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ServiceCenters') AND name = 'Latitude')
BEGIN
    ALTER TABLE ServiceCenters ADD Latitude float NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ServiceCenters') AND name = 'Longitude')
BEGIN
    ALTER TABLE ServiceCenters ADD Longitude float NULL;
END

-- You can execute this SQL script directly against your database
-- using SQL Server Management Studio or another SQL client tool
