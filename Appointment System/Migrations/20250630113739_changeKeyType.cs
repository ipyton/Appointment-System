using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Appointment_System.Migrations
{
    /// <inheritdoc />
    public partial class changeKeyType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "Tokens",
                type: "varchar(900)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(2000)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "Tokens",
                type: "varchar(2000)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(900)");
        }
    }
}
