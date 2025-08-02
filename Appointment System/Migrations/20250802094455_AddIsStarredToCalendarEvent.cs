using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Appointment_System.Migrations
{
    /// <inheritdoc />
    public partial class AddIsStarredToCalendarEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsStarred",
                table: "CalendarEvents",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsStarred",
                table: "CalendarEvents");
        }
    }
}
