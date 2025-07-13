using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vpassbackend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNotesFromFuelEfficiency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "FuelEfficiencies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "FuelEfficiencies",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
