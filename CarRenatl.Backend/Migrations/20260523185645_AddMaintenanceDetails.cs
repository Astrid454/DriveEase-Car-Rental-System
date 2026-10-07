using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRental.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedCompletionDate",
                table: "MaintenanceRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_CarId",
                table: "MaintenanceRecords",
                column: "CarId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRecords_Cars_CarId",
                table: "MaintenanceRecords",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "CarId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRecords_Cars_CarId",
                table: "MaintenanceRecords");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRecords_CarId",
                table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "EstimatedCompletionDate",
                table: "MaintenanceRecords");
        }
    }
}
