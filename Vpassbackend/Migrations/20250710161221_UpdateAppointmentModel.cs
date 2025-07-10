using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vpassbackend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAppointmentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AppointmentPrice",
                table: "Appointments",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Station_id",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Station_id",
                table: "Appointments",
                column: "Station_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_ServiceCenters_Station_id",
                table: "Appointments",
                column: "Station_id",
                principalTable: "ServiceCenters",
                principalColumn: "Station_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_ServiceCenters_Station_id",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_Station_id",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "AppointmentPrice",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Station_id",
                table: "Appointments");
        }
    }
}
