using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRental.Backend.Migrations
{
    public partial class AddReservationReminderPreference : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ReservationRemindersEnabled",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReservationRemindersEnabled",
                table: "Users");
        }
    }
}