using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vpassbackend.Migrations
{
    /// <inheritdoc />
    public partial class FixForeignKeyConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentLogs_Appointments_AppointmentId",
                table: "PaymentLogs");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentLogs_Appointments_AppointmentId",
                table: "PaymentLogs",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "AppointmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentLogs_Appointments_AppointmentId",
                table: "PaymentLogs");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentLogs_Appointments_AppointmentId",
                table: "PaymentLogs",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "AppointmentId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
