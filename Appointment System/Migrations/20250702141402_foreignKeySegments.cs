using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Appointment_System.Migrations
{
    /// <inheritdoc />
    public partial class foreignKeySegments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Segments_TemplateId",
                table: "Segments",
                column: "TemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Segments_Templates_TemplateId",
                table: "Segments",
                column: "TemplateId",
                principalTable: "Templates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Segments_Templates_TemplateId",
                table: "Segments");

            migrationBuilder.DropIndex(
                name: "IX_Segments_TemplateId",
                table: "Segments");
        }
    }
}
