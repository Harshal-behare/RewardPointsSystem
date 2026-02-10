using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RewardPointsSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDataIntegrityConstraintsIndexesAndEventEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BannerImageUrl",
                table: "Events",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Events",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxParticipants",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationEndDate",
                table: "Events",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationStartDate",
                table: "Events",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VirtualLink",
                table: "Events",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Redemption_Quantity",
                table: "Redemptions",
                sql: "[Quantity] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProductPricing_PointsCost",
                table: "ProductPricings",
                sql: "[PointsCost] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PointsAccount_CurrentBalance",
                table: "PointsAccounts",
                sql: "[CurrentBalance] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PointsAccount_TotalEarned",
                table: "PointsAccounts",
                sql: "[TotalEarned] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PointsAccount_TotalRedeemed",
                table: "PointsAccounts",
                sql: "[TotalRedeemed] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_InventoryItem_QuantityAvailable",
                table: "InventoryItems",
                sql: "[QuantityAvailable] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_InventoryItem_QuantityReserved",
                table: "InventoryItems",
                sql: "[QuantityReserved] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Event_TotalPointsPool",
                table: "Events",
                sql: "[TotalPointsPool] > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Redemption_Quantity",
                table: "Redemptions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProductPricing_PointsCost",
                table: "ProductPricings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PointsAccount_CurrentBalance",
                table: "PointsAccounts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PointsAccount_TotalEarned",
                table: "PointsAccounts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PointsAccount_TotalRedeemed",
                table: "PointsAccounts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InventoryItem_QuantityAvailable",
                table: "InventoryItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InventoryItem_QuantityReserved",
                table: "InventoryItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Event_TotalPointsPool",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "BannerImageUrl",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "MaxParticipants",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "RegistrationEndDate",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "RegistrationStartDate",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "VirtualLink",
                table: "Events");
        }
    }
}
