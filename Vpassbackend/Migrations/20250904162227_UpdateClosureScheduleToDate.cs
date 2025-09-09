using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vpassbackend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateClosureScheduleToDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Day",
                table: "ClosureSchedules");

            migrationBuilder.DropColumn(
                name: "WeekNumber",
                table: "ClosureSchedules");

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosureDate",
                table: "ClosureSchedules",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosureDate",
                table: "ClosureSchedules");

            migrationBuilder.AddColumn<string>(
                name: "Day",
                table: "ClosureSchedules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WeekNumber",
                table: "ClosureSchedules",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
