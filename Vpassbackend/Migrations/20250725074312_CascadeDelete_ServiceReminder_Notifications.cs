using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vpassbackend.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDelete_ServiceReminder_Notifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_ServiceReminders_ServiceReminderId",
                table: "Notifications");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_ServiceReminders_ServiceReminderId",
                table: "Notifications",
                column: "ServiceReminderId",
                principalTable: "ServiceReminders",
                principalColumn: "ServiceReminderId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_ServiceReminders_ServiceReminderId",
                table: "Notifications");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_ServiceReminders_ServiceReminderId",
                table: "Notifications",
                column: "ServiceReminderId",
                principalTable: "ServiceReminders",
                principalColumn: "ServiceReminderId");
        }
    }
}
