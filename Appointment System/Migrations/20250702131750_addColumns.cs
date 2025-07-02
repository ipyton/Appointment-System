using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Appointment_System.Migrations
{
    /// <inheritdoc />
    public partial class addColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProviderId",
                table: "Templates",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Templates",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Templates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Templates",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Templates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Templates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentAppointmentCount",
                table: "Slots",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Duration",
                table: "Slots",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Slots",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SegmentId",
                table: "Slots",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DayId1",
                table: "Segments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Segments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxConcurrentAppointments",
                table: "Segments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "GroupName",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Days",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Templates_ProviderId",
                table: "Templates",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Slots_SegmentId",
                table: "Slots",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Segments_DayId1",
                table: "Segments",
                column: "DayId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Segments_Days_DayId1",
                table: "Segments",
                column: "DayId1",
                principalTable: "Days",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Slots_Segments_SegmentId",
                table: "Slots",
                column: "SegmentId",
                principalTable: "Segments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Templates_AspNetUsers_ProviderId",
                table: "Templates",
                column: "ProviderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Segments_Days_DayId1",
                table: "Segments");

            migrationBuilder.DropForeignKey(
                name: "FK_Slots_Segments_SegmentId",
                table: "Slots");

            migrationBuilder.DropForeignKey(
                name: "FK_Templates_AspNetUsers_ProviderId",
                table: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_Templates_ProviderId",
                table: "Templates");

            migrationBuilder.DropIndex(
                name: "IX_Slots_SegmentId",
                table: "Slots");

            migrationBuilder.DropIndex(
                name: "IX_Segments_DayId1",
                table: "Segments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "CurrentAppointmentCount",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "SegmentId",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "DayId1",
                table: "Segments");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Segments");

            migrationBuilder.DropColumn(
                name: "MaxConcurrentAppointments",
                table: "Segments");

            migrationBuilder.DropColumn(
                name: "GroupName",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Days");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderId",
                table: "Templates",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Templates",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Templates",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);
        }
    }
}
