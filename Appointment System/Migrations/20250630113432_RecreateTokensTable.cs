using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Appointment_System.Migrations
{
    /// <inheritdoc />
    public partial class RecreateTokensTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_Tokens_AspNetUsers_ApplicationUserId",
                table: "Tokens");

            // Drop existing table
            migrationBuilder.DropTable(
                name: "Tokens");

            // Recreate table with varchar instead of nvarchar
            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    AccessToken = table.Column<string>(type: "varchar(900)", nullable: false),
                    ExpiresOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => x.AccessToken);
                    table.ForeignKey(
                        name: "FK_Tokens_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_ApplicationUserId",
                table: "Tokens",
                column: "ApplicationUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop recreated table
            migrationBuilder.DropTable(
                name: "Tokens");

            // Recreate original table with nvarchar
            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    AccessToken = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExpiresOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tokens", x => x.AccessToken);
                    table.ForeignKey(
                        name: "FK_Tokens_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_ApplicationUserId",
                table: "Tokens",
                column: "ApplicationUserId");
        }
    }
}
