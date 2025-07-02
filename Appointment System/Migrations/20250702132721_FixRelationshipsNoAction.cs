using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Appointment_System.Migrations
{
    /// <inheritdoc />
    public partial class FixRelationshipsNoAction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Days_Templates_TemplateId",
                table: "Days");

            migrationBuilder.DropForeignKey(
                name: "FK_Days_Templates_TemplateId1",
                table: "Days");

            migrationBuilder.DropForeignKey(
                name: "FK_Segments_Days_DayId",
                table: "Segments");

            migrationBuilder.DropForeignKey(
                name: "FK_Segments_Days_DayId1",
                table: "Segments");

            migrationBuilder.DropForeignKey(
                name: "FK_Slots_Segments_DayId",
                table: "Slots");

            migrationBuilder.DropForeignKey(
                name: "FK_Slots_Segments_SegmentId",
                table: "Slots");

            migrationBuilder.DropIndex(
                name: "IX_Slots_DayId",
                table: "Slots");

            migrationBuilder.DropIndex(
                name: "IX_Segments_DayId1",
                table: "Segments");

            migrationBuilder.DropIndex(
                name: "IX_Days_TemplateId1",
                table: "Days");

            migrationBuilder.DropColumn(
                name: "DayId1",
                table: "Segments");

            migrationBuilder.DropColumn(
                name: "TemplateId1",
                table: "Days");

            migrationBuilder.AddColumn<int>(
                name: "SegmentId1",
                table: "Slots",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Slots_SegmentId1",
                table: "Slots",
                column: "SegmentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Days_Templates_TemplateId",
                table: "Days",
                column: "TemplateId",
                principalTable: "Templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Segments_Days_DayId",
                table: "Segments",
                column: "DayId",
                principalTable: "Days",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Slots_Segments_SegmentId",
                table: "Slots",
                column: "SegmentId",
                principalTable: "Segments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Slots_Segments_SegmentId1",
                table: "Slots",
                column: "SegmentId1",
                principalTable: "Segments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Days_Templates_TemplateId",
                table: "Days");

            migrationBuilder.DropForeignKey(
                name: "FK_Segments_Days_DayId",
                table: "Segments");

            migrationBuilder.DropForeignKey(
                name: "FK_Slots_Segments_SegmentId",
                table: "Slots");

            migrationBuilder.DropForeignKey(
                name: "FK_Slots_Segments_SegmentId1",
                table: "Slots");

            migrationBuilder.DropIndex(
                name: "IX_Slots_SegmentId1",
                table: "Slots");

            migrationBuilder.DropColumn(
                name: "SegmentId1",
                table: "Slots");

            migrationBuilder.AddColumn<int>(
                name: "DayId1",
                table: "Segments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TemplateId1",
                table: "Days",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Slots_DayId",
                table: "Slots",
                column: "DayId");

            migrationBuilder.CreateIndex(
                name: "IX_Segments_DayId1",
                table: "Segments",
                column: "DayId1");

            migrationBuilder.CreateIndex(
                name: "IX_Days_TemplateId1",
                table: "Days",
                column: "TemplateId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Days_Templates_TemplateId",
                table: "Days",
                column: "TemplateId",
                principalTable: "Templates",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Days_Templates_TemplateId1",
                table: "Days",
                column: "TemplateId1",
                principalTable: "Templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Segments_Days_DayId",
                table: "Segments",
                column: "DayId",
                principalTable: "Days",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Segments_Days_DayId1",
                table: "Segments",
                column: "DayId1",
                principalTable: "Days",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Slots_Segments_DayId",
                table: "Slots",
                column: "DayId",
                principalTable: "Segments",
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
