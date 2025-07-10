using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vpassbackend.Migrations
{
    /// <inheritdoc />
    public partial class CreateServiceCenterServicesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_ServiceCenters_Station_id",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_Station_id",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Station_id",
                table: "Services");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Services",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ServiceCenterServices",
                columns: table => new
                {
                    ServiceCenterServiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Station_id = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    CustomPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCenterServices", x => x.ServiceCenterServiceId);
                    table.ForeignKey(
                        name: "FK_ServiceCenterServices_ServiceCenters_Station_id",
                        column: x => x.Station_id,
                        principalTable: "ServiceCenters",
                        principalColumn: "Station_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceCenterServices_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCenterServices_ServiceId_Station_id",
                table: "ServiceCenterServices",
                columns: new[] { "ServiceId", "Station_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCenterServices_Station_id",
                table: "ServiceCenterServices",
                column: "Station_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceCenterServices");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Services");

            migrationBuilder.AddColumn<int>(
                name: "Station_id",
                table: "Services",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Services_Station_id",
                table: "Services",
                column: "Station_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_ServiceCenters_Station_id",
                table: "Services",
                column: "Station_id",
                principalTable: "ServiceCenters",
                principalColumn: "Station_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
