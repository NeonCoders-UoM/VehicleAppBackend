using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vpassbackend.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentBookingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add location coordinates to ServiceCenter
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "ServiceCenters",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "ServiceCenters",
                nullable: true);

            // Drop the index on ServiceId first
            migrationBuilder.DropIndex(
                name: "IX_Appointments_ServiceId",
                table: "Appointments");

            // Then drop the foreign key constraint
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Services_ServiceId",
                table: "Appointments");

            // Create AppointmentService table
            migrationBuilder.CreateTable(
                name: "AppointmentServices",
                columns: table => new
                {
                    AppointmentServiceId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppointmentId = table.Column<int>(nullable: false),
                    ServiceId = table.Column<int>(nullable: false),
                    ServicePrice = table.Column<decimal>(type: "decimal(10, 2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentServices", x => x.AppointmentServiceId);
                    table.ForeignKey(
                        name: "FK_AppointmentServices_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "AppointmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppointmentServices_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.NoAction);
                });

            // Now update Appointments table
            migrationBuilder.AddColumn<int>(
                name: "ServiceCenterId",
                table: "Appointments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedTotalCost",
                table: "Appointments",
                type: "decimal(10, 2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AdvancePaymentAmount",
                table: "Appointments",
                type: "decimal(10, 2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualTotalCost",
                table: "Appointments",
                type: "decimal(10, 2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdvancePaymentCompleted",
                table: "Appointments",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Appointments",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "AppointmentDate",
                table: "Appointments",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            // Now drop the column after index and constraint are removed
            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Appointments");

            // Update PaymentLog table
            migrationBuilder.AlterColumn<int>(
                name: "InvoiceId",
                table: "PaymentLogs",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "AppointmentId",
                table: "PaymentLogs",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "PaymentLogs",
                type: "decimal(10, 2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "PaymentLogs",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentType",
                table: "PaymentLogs",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionReference",
                table: "PaymentLogs",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PaymentLogs",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "PaymentLogs",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldNullable: true);

            // Create foreign keys
            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_ServiceCenters_ServiceCenterId",
                table: "Appointments",
                column: "ServiceCenterId",
                principalTable: "ServiceCenters",
                principalColumn: "Station_id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentLogs_Appointments_AppointmentId",
                table: "PaymentLogs",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "AppointmentId",
                onDelete: ReferentialAction.SetNull);

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ServiceCenterId",
                table: "Appointments",
                column: "ServiceCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentServices_AppointmentId",
                table: "AppointmentServices",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentServices_ServiceId",
                table: "AppointmentServices",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentLogs_AppointmentId",
                table: "PaymentLogs",
                column: "AppointmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign keys and indexes
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_ServiceCenters_ServiceCenterId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentLogs_Appointments_AppointmentId",
                table: "PaymentLogs");

            migrationBuilder.DropTable(
                name: "AppointmentServices");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ServiceCenterId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_PaymentLogs_AppointmentId",
                table: "PaymentLogs");

            // Revert changes to ServiceCenter
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "ServiceCenters");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "ServiceCenters");

            // Add back ServiceId column
            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Revert changes to Appointments
            migrationBuilder.DropColumn(
                name: "ServiceCenterId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "EstimatedTotalCost",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "AdvancePaymentAmount",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ActualTotalCost",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "IsAdvancePaymentCompleted",
                table: "Appointments");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Appointments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Appointments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 20,
                oldDefaultValue: "Pending");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AppointmentDate",
                table: "Appointments",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime));

            // Revert changes to PaymentLog
            migrationBuilder.DropColumn(
                name: "AppointmentId",
                table: "PaymentLogs");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "PaymentLogs");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "PaymentLogs");

            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "PaymentLogs");

            migrationBuilder.DropColumn(
                name: "TransactionReference",
                table: "PaymentLogs");

            migrationBuilder.AlterColumn<int>(
                name: "InvoiceId",
                table: "PaymentLogs",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PaymentLogs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 20,
                oldDefaultValue: "Pending");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "PaymentLogs",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldDefaultValueSql: "GETDATE()");

            // Restore foreign keys
            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ServiceId",
                table: "Appointments",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Services_ServiceId",
                table: "Appointments",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "ServiceId",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
