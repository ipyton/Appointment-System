using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Appointment_System.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAppointmentSlotRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_SlotId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "PaymentAmount",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "PaymentCurrency",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_SlotId",
                table: "Appointments",
                column: "SlotId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_SlotId",
                table: "Appointments");

            migrationBuilder.AddColumn<decimal>(
                name: "PaymentAmount",
                table: "Appointments",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentCurrency",
                table: "Appointments",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "Appointments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Appointments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_SlotId",
                table: "Appointments",
                column: "SlotId");
        }
    }
}
