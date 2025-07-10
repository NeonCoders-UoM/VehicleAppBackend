using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vpassbackend.Migrations
{
    /// <inheritdoc />
    public partial class AddCoordinatesToServiceCenter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, drop the problematic constraint if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT * FROM sys.foreign_keys 
                    WHERE name = 'FK_PaymentLogs_Appointments_AppointmentId'
                )
                BEGIN
                    ALTER TABLE [PaymentLogs] DROP CONSTRAINT [FK_PaymentLogs_Appointments_AppointmentId]
                END
            ");

            // Add our new columns
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "ServiceCenters",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "ServiceCenters",
                type: "float",
                nullable: true);

            // Re-add the constraint with NO ACTION
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
            // First remove our foreign key constraint
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT * FROM sys.foreign_keys 
                    WHERE name = 'FK_PaymentLogs_Appointments_AppointmentId'
                )
                BEGIN
                    ALTER TABLE [PaymentLogs] DROP CONSTRAINT [FK_PaymentLogs_Appointments_AppointmentId]
                END
            ");

            // Drop our columns
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "ServiceCenters");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "ServiceCenters");
        }
    }
}
