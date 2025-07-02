using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Appointment_System.Migrations
{
    /// <inheritdoc />
    public partial class services : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Arrangements_TemplateId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_AspNetUsers_ApplicationUserId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_AspNetUsers_UserId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Services_ServiceId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Arrangements_Services_ServiceId1",
                table: "Arrangements");

            migrationBuilder.DropForeignKey(
                name: "FK_Arrangements_Templates_TemplateId1",
                table: "Arrangements");

            migrationBuilder.DropForeignKey(
                name: "FK_Segments_Services_ServiceId",
                table: "Segments");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_AspNetUsers_ProviderId",
                table: "Services");

            migrationBuilder.DropForeignKey(
                name: "FK_Slots_Segments_SegmentId",
                table: "Slots");

            migrationBuilder.DropIndex(
                name: "IX_Segments_ServiceId",
                table: "Segments");

            migrationBuilder.DropIndex(
                name: "IX_Arrangements_TemplateId1",
                table: "Arrangements");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ApplicationUserId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ServiceId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_TemplateId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "DayId",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "Segments");

            migrationBuilder.DropColumn(
                name: "RepeatInterval",
                table: "Arrangements");

            migrationBuilder.DropColumn(
                name: "RepeatTimes",
                table: "Arrangements");

            migrationBuilder.DropColumn(
                name: "StartDateTime",
                table: "Arrangements");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "DayId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "SegmentId",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "SegmentId",
                table: "Slots",
                newName: "ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_Slots_SegmentId",
                table: "Slots",
                newName: "IX_Slots_ServiceId");

            migrationBuilder.RenameColumn(
                name: "TemplateId1",
                table: "Arrangements",
                newName: "Index");

            migrationBuilder.RenameColumn(
                name: "TemplateId",
                table: "Appointments",
                newName: "ProviderId");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "StartTime",
                table: "Slots",
                type: "time",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "EndTime",
                table: "Slots",
                type: "time",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "Slots",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AlterColumn<int>(
                name: "ServiceId1",
                table: "Arrangements",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateOnly>(
                name: "StartDate",
                table: "Arrangements",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_AspNetUsers_UserId",
                table: "Appointments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Arrangements_Services_ServiceId1",
                table: "Arrangements",
                column: "ServiceId1",
                principalTable: "Services",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_AspNetUsers_ProviderId",
                table: "Services",
                column: "ProviderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Slots_Services_ServiceId",
                table: "Slots",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_AspNetUsers_UserId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Arrangements_Services_ServiceId1",
                table: "Arrangements");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_AspNetUsers_ProviderId",
                table: "Services");

            migrationBuilder.DropForeignKey(
                name: "FK_Slots_Services_ServiceId",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Arrangements");

            migrationBuilder.RenameColumn(
                name: "ServiceId",
                table: "Slots",
                newName: "SegmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Slots_ServiceId",
                table: "Slots",
                newName: "IX_Slots_SegmentId");

            migrationBuilder.RenameColumn(
                name: "Index",
                table: "Arrangements",
                newName: "TemplateId1");

            migrationBuilder.RenameColumn(
                name: "ProviderId",
                table: "Appointments",
                newName: "TemplateId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "Slots",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndTime",
                table: "Slots",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time");

            migrationBuilder.AddColumn<int>(
                name: "DayId",
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

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "Services",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "Segments",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ServiceId1",
                table: "Arrangements",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RepeatInterval",
                table: "Arrangements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RepeatTimes",
                table: "Arrangements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDateTime",
                table: "Arrangements",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Appointments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DayId",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SegmentId",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Segments_ServiceId",
                table: "Segments",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Arrangements_TemplateId1",
                table: "Arrangements",
                column: "TemplateId1");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ApplicationUserId",
                table: "Appointments",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ServiceId",
                table: "Appointments",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_TemplateId",
                table: "Appointments",
                column: "TemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Arrangements_TemplateId",
                table: "Appointments",
                column: "TemplateId",
                principalTable: "Arrangements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_AspNetUsers_ApplicationUserId",
                table: "Appointments",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_AspNetUsers_UserId",
                table: "Appointments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Services_ServiceId",
                table: "Appointments",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Arrangements_Services_ServiceId1",
                table: "Arrangements",
                column: "ServiceId1",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Arrangements_Templates_TemplateId1",
                table: "Arrangements",
                column: "TemplateId1",
                principalTable: "Templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Segments_Services_ServiceId",
                table: "Segments",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_AspNetUsers_ProviderId",
                table: "Services",
                column: "ProviderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Slots_Segments_SegmentId",
                table: "Slots",
                column: "SegmentId",
                principalTable: "Segments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
