using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vpassbackend.Migrations
{
    /// <inheritdoc />
    public partial class AddLoyaltyPointsToServiceCenterService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoyaltyPoints",
                table: "Services");

            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "ServiceCenterServices",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LoyaltyPoints",
                table: "ServiceCenterServices",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "ServiceCenterServices");

            migrationBuilder.DropColumn(
                name: "LoyaltyPoints",
                table: "ServiceCenterServices");

            migrationBuilder.AddColumn<int>(
                name: "LoyaltyPoints",
                table: "Services",
                type: "int",
                nullable: true);
        }
    }
}
