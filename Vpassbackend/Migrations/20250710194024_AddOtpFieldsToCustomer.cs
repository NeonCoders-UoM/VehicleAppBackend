using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vpassbackend.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpFieldsToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEmailVerified",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OtpCode",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OtpExpiry",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ServiceReminders",
                columns: table => new
                {
                    ServiceReminderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    ReminderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IntervalMonths = table.Column<int>(type: "int", nullable: false),
                    NotifyBeforeDays = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceReminders", x => x.ServiceReminderId);
                    table.ForeignKey(
                        name: "FK_ServiceReminders_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "ServiceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceReminders_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceReminders_ServiceId",
                table: "ServiceReminders",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceReminders_VehicleId",
                table: "ServiceReminders",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceReminders");

            migrationBuilder.DropColumn(
                name: "IsEmailVerified",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "OtpCode",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "OtpExpiry",
                table: "Customers");
        }
    }
}
