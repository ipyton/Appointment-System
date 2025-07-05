using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Appointment_System.Migrations
{
    /// <inheritdoc />
    public partial class FixAppointmentBillRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bills_Appointments_AppointmentId1",
                table: "Bills");

            migrationBuilder.DropIndex(
                name: "IX_Bills_AppointmentId1",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "AppointmentId1",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "BillId",
                table: "Appointments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AppointmentId1",
                table: "Bills",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BillId",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Bills_AppointmentId1",
                table: "Bills",
                column: "AppointmentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Bills_Appointments_AppointmentId1",
                table: "Bills",
                column: "AppointmentId1",
                principalTable: "Appointments",
                principalColumn: "Id");
        }
    }
}
