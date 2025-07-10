-- Script to verify the database schema after migrations
-- Run this script against your database to check if migrations were applied correctly

-- Check if Latitude and Longitude columns exist in ServiceCenters table
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE
FROM 
    INFORMATION_SCHEMA.COLUMNS
WHERE 
    TABLE_NAME = 'ServiceCenters' 
    AND COLUMN_NAME IN ('Latitude', 'Longitude');

-- Verify foreign key constraints for PaymentLogs table
SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTableName,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS ReferencedColumnName,
    fk.delete_referential_action_desc AS DeleteAction
FROM 
    sys.foreign_keys AS fk
INNER JOIN 
    sys.foreign_key_columns AS fkc ON fk.OBJECT_ID = fkc.constraint_object_id
WHERE 
    OBJECT_NAME(fk.parent_object_id) = 'PaymentLogs';

-- Check for any migrations in the __EFMigrationsHistory table
SELECT 
    MigrationId, 
    ProductVersion
FROM 
    __EFMigrationsHistory
ORDER BY 
    MigrationId;
