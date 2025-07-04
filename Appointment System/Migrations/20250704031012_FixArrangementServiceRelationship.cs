using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Appointment_System.Migrations
{
    /// <inheritdoc />
    public partial class FixArrangementServiceRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Arrangements_Services_ServiceId1",
                table: "Arrangements");

            migrationBuilder.DropIndex(
                name: "IX_Arrangements_ServiceId1",
                table: "Arrangements");

            migrationBuilder.DropColumn(
                name: "ServiceId1",
                table: "Arrangements");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServiceId1",
                table: "Arrangements",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Arrangements_ServiceId1",
                table: "Arrangements",
                column: "ServiceId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Arrangements_Services_ServiceId1",
                table: "Arrangements",
                column: "ServiceId1",
                principalTable: "Services",
                principalColumn: "Id");
        }
    }
}
