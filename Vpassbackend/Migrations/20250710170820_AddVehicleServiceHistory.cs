using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vpassbackend.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleServiceHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VehicleServiceHistories",
                columns: table => new
                {
                    ServiceHistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    ServiceCenterId = table.Column<int>(type: "int", nullable: true),
                    ServiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ServiceCost = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Mileage = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    ExternalServiceCenterName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ReceiptDocumentPath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleServiceHistories", x => x.ServiceHistoryId);
                    table.ForeignKey(
                        name: "FK_VehicleServiceHistories_ServiceCenters_ServiceCenterId",
                        column: x => x.ServiceCenterId,
                        principalTable: "ServiceCenters",
                        principalColumn: "Station_id");
                    table.ForeignKey(
                        name: "FK_VehicleServiceHistories_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehicleServiceHistories_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleServiceHistories_ServiceCenterId",
                table: "VehicleServiceHistories",
                column: "ServiceCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleServiceHistories_ServiceId",
                table: "VehicleServiceHistories",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleServiceHistories_VehicleId",
                table: "VehicleServiceHistories",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleServiceHistories");
        }
    }
}
