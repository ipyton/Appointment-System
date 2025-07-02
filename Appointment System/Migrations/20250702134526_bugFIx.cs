using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Appointment_System.Migrations
{
    /// <inheritdoc />
    public partial class bugFIx : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Days_Templates_TemplateId1",
                table: "Days");

            migrationBuilder.DropIndex(
                name: "IX_Days_TemplateId1",
                table: "Days");

            migrationBuilder.DropColumn(
                name: "TemplateId1",
                table: "Days");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TemplateId1",
                table: "Days",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Days_TemplateId1",
                table: "Days",
                column: "TemplateId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Days_Templates_TemplateId1",
                table: "Days",
                column: "TemplateId1",
                principalTable: "Templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
