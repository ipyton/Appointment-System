using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Appointment_System.Migrations
{
    /// <inheritdoc />
    public partial class fixDaySegmentRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Segments_Days_DayId",
                table: "Segments");

            migrationBuilder.DropForeignKey(
                name: "FK_Segments_Days_DayId1",
                table: "Segments");

            migrationBuilder.DropIndex(
                name: "IX_Segments_DayId1",
                table: "Segments");

            migrationBuilder.DropColumn(
                name: "DayId1",
                table: "Segments");

            migrationBuilder.AddForeignKey(
                name: "FK_Segments_Days_DayId",
                table: "Segments",
                column: "DayId",
                principalTable: "Days",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Segments_Days_DayId",
                table: "Segments");

            migrationBuilder.AddColumn<int>(
                name: "DayId1",
                table: "Segments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Segments_DayId1",
                table: "Segments",
                column: "DayId1");

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
                principalColumn: "Id");
        }
    }
}
