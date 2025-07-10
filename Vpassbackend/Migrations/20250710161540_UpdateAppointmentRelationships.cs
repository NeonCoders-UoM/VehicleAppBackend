using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vpassbackend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAppointmentRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_ServiceCenters_Station_id",
                table: "Appointments");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_ServiceCenters_Station_id",
                table: "Appointments",
                column: "Station_id",
                principalTable: "ServiceCenters",
                principalColumn: "Station_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_ServiceCenters_Station_id",
                table: "Appointments");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_ServiceCenters_Station_id",
                table: "Appointments",
                column: "Station_id",
                principalTable: "ServiceCenters",
                principalColumn: "Station_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
