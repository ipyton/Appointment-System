using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Appointment_System.Migrations
{
    /// <inheritdoc />
    public partial class AddRecurringServicePattern : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaxRepetitions = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecurrenceType = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
