using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vpassbackend.Migrations
{
    /// <inheritdoc />
    public partial class AddDateToServiceAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Day",
                table: "ServiceAvailabilities");

            migrationBuilder.DropColumn(
                name: "WeekNumber",
                table: "ServiceAvailabilities");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "ServiceAvailabilities",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "ServiceAvailabilities");

            migrationBuilder.AddColumn<string>(
                name: "Day",
                table: "ServiceAvailabilities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WeekNumber",
                table: "ServiceAvailabilities",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
