using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vpassbackend.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadeDeletePaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the problematic foreign key if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT * FROM sys.foreign_keys 
                    WHERE name = 'FK_PaymentLogs_Appointments_AppointmentId'
                )
                BEGIN
                    ALTER TABLE [PaymentLogs] DROP CONSTRAINT [FK_PaymentLogs_Appointments_AppointmentId]
                END
            ");

            // Add the foreign key with NO ACTION
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'PaymentLogs' AND COLUMN_NAME = 'AppointmentId'
                )
                BEGIN
                    ALTER TABLE [PaymentLogs] ADD CONSTRAINT [FK_PaymentLogs_Appointments_AppointmentId] 
                    FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([AppointmentId]) ON DELETE NO ACTION;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop our NO ACTION foreign key
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT * FROM sys.foreign_keys 
                    WHERE name = 'FK_PaymentLogs_Appointments_AppointmentId'
                )
                BEGIN
                    ALTER TABLE [PaymentLogs] DROP CONSTRAINT [FK_PaymentLogs_Appointments_AppointmentId]
                END
            ");

            // Restore the original foreign key with SET NULL
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'PaymentLogs' AND COLUMN_NAME = 'AppointmentId'
                )
                BEGIN
                    ALTER TABLE [PaymentLogs] ADD CONSTRAINT [FK_PaymentLogs_Appointments_AppointmentId] 
                    FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([AppointmentId]) ON DELETE SET NULL;
                END
            ");
        }
    }
}
