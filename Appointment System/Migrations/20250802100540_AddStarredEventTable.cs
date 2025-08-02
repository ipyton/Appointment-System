using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Appointment_System.Migrations
{
    /// <inheritdoc />
    public partial class AddStarredEventTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsStarred",
                table: "CalendarEvents");

            migrationBuilder.CreateTable(
                name: "StarredEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppointmentId = table.Column<int>(type: "int", nullable: true),
                    CalendarEventId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StarredEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StarredEvents_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StarredEvents_CalendarEvents_CalendarEventId",
                        column: x => x.CalendarEventId,
                        principalTable: "CalendarEvents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_StarredEvents_AppointmentId",
                table: "StarredEvents",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_StarredEvents_CalendarEventId",
                table: "StarredEvents",
                column: "CalendarEventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StarredEvents");

            migrationBuilder.AddColumn<bool>(
                name: "IsStarred",
                table: "CalendarEvents",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
