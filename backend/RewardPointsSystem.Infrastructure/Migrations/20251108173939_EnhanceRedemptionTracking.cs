using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RewardPointsSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceRedemptionTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DeliveryNotes",
                table: "Redemptions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedBy",
                table: "Redemptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Redemptions",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Redemptions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Redemptions_ApprovedBy",
                table: "Redemptions",
                column: "ApprovedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Redemptions_Users_ApprovedBy",
                table: "Redemptions",
                column: "ApprovedBy",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Redemptions_Users_ApprovedBy",
                table: "Redemptions");

            migrationBuilder.DropIndex(
                name: "IX_Redemptions_ApprovedBy",
                table: "Redemptions");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "Redemptions");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Redemptions");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Redemptions");

            migrationBuilder.AlterColumn<string>(
                name: "DeliveryNotes",
                table: "Redemptions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);
        }
    }
}
