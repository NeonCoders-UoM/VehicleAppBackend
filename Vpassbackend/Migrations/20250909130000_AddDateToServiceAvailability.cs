using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vpassbackend.Migrations
{
    /// <inheritdoc />
    public partial class AddDateToServiceAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if Day column exists before dropping
            migrationBuilder.Sql(@"
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
            ");

            // Check if WeekNumber column exists before dropping
            migrationBuilder.Sql(@"
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
            ");

            // Check if Date column exists before adding
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                               WHERE TABLE_NAME = 'ServiceAvailabilities' AND COLUMN_NAME = 'Date')
                BEGIN
                    ALTER TABLE [ServiceAvailabilities] ADD [Date] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "ServiceAvailabilities");

            migrationBuilder.AddColumn<string>(
                name: "Day",
                table: "ServiceAvailabilities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WeekNumber",
                table: "ServiceAvailabilities",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
