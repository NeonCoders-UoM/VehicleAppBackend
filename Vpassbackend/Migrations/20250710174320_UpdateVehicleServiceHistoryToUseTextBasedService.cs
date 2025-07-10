using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vpassbackend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVehicleServiceHistoryToUseTextBasedService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleServiceHistories_Services_ServiceId",
                table: "VehicleServiceHistories");

            migrationBuilder.DropIndex(
                name: "IX_VehicleServiceHistories_ServiceId",
                table: "VehicleServiceHistories");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "VehicleServiceHistories");

            migrationBuilder.RenameColumn(
                name: "ServiceCost",
                table: "VehicleServiceHistories",
                newName: "Cost");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "VehicleServiceHistories",
                newName: "Description");

            migrationBuilder.AddColumn<string>(
                name: "ServiceType",
                table: "VehicleServiceHistories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ServicedByUserId",
                table: "VehicleServiceHistories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleServiceHistories_ServicedByUserId",
                table: "VehicleServiceHistories",
                column: "ServicedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleServiceHistories_Users_ServicedByUserId",
                table: "VehicleServiceHistories",
                column: "ServicedByUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleServiceHistories_Users_ServicedByUserId",
                table: "VehicleServiceHistories");

            migrationBuilder.DropIndex(
                name: "IX_VehicleServiceHistories_ServicedByUserId",
                table: "VehicleServiceHistories");

            migrationBuilder.DropColumn(
                name: "ServiceType",
                table: "VehicleServiceHistories");

            migrationBuilder.DropColumn(
                name: "ServicedByUserId",
                table: "VehicleServiceHistories");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "VehicleServiceHistories",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "Cost",
                table: "VehicleServiceHistories",
                newName: "ServiceCost");

            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "VehicleServiceHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleServiceHistories_ServiceId",
                table: "VehicleServiceHistories",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleServiceHistories_Services_ServiceId",
                table: "VehicleServiceHistories",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "ServiceId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
