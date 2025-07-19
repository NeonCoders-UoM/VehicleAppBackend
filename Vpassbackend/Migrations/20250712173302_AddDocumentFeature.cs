using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vpassbackend.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Vehicles_VehicleId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "UploadDate",
                table: "Documents",
                newName: "UploadedAt");

            migrationBuilder.RenameColumn(
                name: "ExpiryDate",
                table: "Documents",
                newName: "ExpirationDate");

            migrationBuilder.AlterColumn<int>(
                name: "VehicleId",
                table: "Documents",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DocumentType",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Documents",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "Documents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "FileUrl",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CustomerId",
                table: "Documents",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Customers_CustomerId",
                table: "Documents",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Vehicles_VehicleId",
                table: "Documents",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Customers_CustomerId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Vehicles_VehicleId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_CustomerId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "FileUrl",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "UploadedAt",
                table: "Documents",
                newName: "UploadDate");

            migrationBuilder.RenameColumn(
                name: "ExpirationDate",
                table: "Documents",
                newName: "ExpiryDate");

            migrationBuilder.AlterColumn<int>(
                name: "VehicleId",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DocumentType",
                table: "Documents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Documents",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Vehicles_VehicleId",
                table: "Documents",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "VehicleId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
