using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RewardPointsSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceTrackingToEventParticipants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttendanceStatus",
                table: "EventParticipants",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckedInAt",
                table: "EventParticipants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventParticipants_AttendanceStatus",
                table: "EventParticipants",
                column: "AttendanceStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EventParticipants_AttendanceStatus",
                table: "EventParticipants");

            migrationBuilder.DropColumn(
                name: "AttendanceStatus",
                table: "EventParticipants");

            migrationBuilder.DropColumn(
                name: "CheckedInAt",
                table: "EventParticipants");
        }
    }
}
