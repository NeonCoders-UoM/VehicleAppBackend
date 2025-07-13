using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vpassbackend.Migrations
{
    /// <inheritdoc />
    public partial class AddCostToFuelEfficiency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Cost",
                table: "FuelEfficiencies",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cost",
                table: "FuelEfficiencies");
        }
    }
}
