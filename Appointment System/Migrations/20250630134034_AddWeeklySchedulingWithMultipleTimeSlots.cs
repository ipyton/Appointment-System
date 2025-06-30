using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Appointment_System.Migrations
{
    /// <inheritdoc />
    public partial class AddWeeklySchedulingWithMultipleTimeSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceAvailabilities_RecurringServicePatterns_RecurringPatternId",
                table: "ServiceAvailabilities");

            migrationBuilder.DropTable(
                name: "RecurringServicePatterns");

            migrationBuilder.DropIndex(
                name: "IX_ServiceAvailabilities_RecurringPatternId",
                table: "ServiceAvailabilities");

            migrationBuilder.DropColumn(
                name: "MaxAppointmentsPerSlot",
                table: "ServiceAvailabilities");

            migrationBuilder.DropColumn(
                name: "RecurringPatternId",
                table: "ServiceAvailabilities");

            migrationBuilder.DropColumn(
                name: "SlotIntervalMinutes",
                table: "ServiceAvailabilities");

            migrationBuilder.CreateTable(
                name: "ServiceSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RepeatWeeks = table.Column<int>(type: "int", nullable: true),
                    SlotDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceSchedules_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeeklyAvailabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceScheduleId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeeklyAvailabilities_ServiceSchedules_ServiceScheduleId",
                        column: x => x.ServiceScheduleId,
                        principalTable: "ServiceSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WeeklyAvailabilityId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    MaxConcurrentAppointments = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeSlots_WeeklyAvailabilities_WeeklyAvailabilityId",
                        column: x => x.WeeklyAvailabilityId,
                        principalTable: "WeeklyAvailabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceSchedules_ServiceId",
                table: "ServiceSchedules",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeSlots_WeeklyAvailabilityId",
                table: "TimeSlots",
                column: "WeeklyAvailabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyAvailabilities_ServiceScheduleId",
                table: "WeeklyAvailabilities",
                column: "ServiceScheduleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimeSlots");

            migrationBuilder.DropTable(
                name: "WeeklyAvailabilities");

            migrationBuilder.DropTable(
                name: "ServiceSchedules");

            migrationBuilder.AddColumn<int>(
                name: "MaxAppointmentsPerSlot",
                table: "ServiceAvailabilities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RecurringPatternId",
                table: "ServiceAvailabilities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SlotIntervalMinutes",
                table: "ServiceAvailabilities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "RecurringServicePatterns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MaxRepetitions = table.Column<int>(type: "int", nullable: true),
                    RecurrenceType = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringServicePatterns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecurringServicePatterns_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceAvailabilities_RecurringPatternId",
                table: "ServiceAvailabilities",
                column: "RecurringPatternId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringServicePatterns_ServiceId",
                table: "RecurringServicePatterns",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceAvailabilities_RecurringServicePatterns_RecurringPatternId",
                table: "ServiceAvailabilities",
                column: "RecurringPatternId",
                principalTable: "RecurringServicePatterns",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
