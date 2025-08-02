using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Appointment_System.Migrations
{
    /// <inheritdoc />
    public partial class AddStarredService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StarredEvents");

            migrationBuilder.CreateTable(
                name: "StarredServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StarredServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StarredServices_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StarredServices_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StarredServices_ServiceId",
                table: "StarredServices",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_StarredServices_UserId_ServiceId",
                table: "StarredServices",
                columns: new[] { "UserId", "ServiceId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StarredServices");

            migrationBuilder.CreateTable(
                name: "StarredEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppointmentId = table.Column<int>(type: "int", nullable: true),
                    CalendarEventId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
    }
}
