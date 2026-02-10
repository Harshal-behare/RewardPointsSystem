using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RewardPointsSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEventPrizeDistribution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EventEndDate",
                table: "Events",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FirstPlacePoints",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SecondPlacePoints",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ThirdPlacePoints",
                table: "Events",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventEndDate",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "FirstPlacePoints",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "SecondPlacePoints",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ThirdPlacePoints",
                table: "Events");
        }
    }
}
