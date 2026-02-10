using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RewardPointsSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFieldsExpandEventStatusAndValidations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "PointsAccounts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "InventoryItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UpdatedBy",
                table: "Users",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PointsAccounts_UpdatedBy",
                table: "PointsAccounts",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_UpdatedBy",
                table: "InventoryItems",
                column: "UpdatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Users_UpdatedBy",
                table: "InventoryItems",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PointsAccounts_Users_UpdatedBy",
                table: "PointsAccounts",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_UpdatedBy",
                table: "Users",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Users_UpdatedBy",
                table: "InventoryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PointsAccounts_Users_UpdatedBy",
                table: "PointsAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_UpdatedBy",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_UpdatedBy",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_PointsAccounts_UpdatedBy",
                table: "PointsAccounts");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_UpdatedBy",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "PointsAccounts");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "InventoryItems");
        }
    }
}
