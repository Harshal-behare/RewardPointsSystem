using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RewardPointsSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRedemptionAuditProductCategoriesAndPricingOptimizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductPricings_ProductId_IsActive",
                table: "ProductPricings");

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "Redemptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProcessedBy",
                table: "Redemptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Redemptions_ProcessedBy",
                table: "Redemptions",
                column: "ProcessedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPricings_ProductId_IsActive_EffectiveFrom",
                table: "ProductPricings",
                columns: new[] { "ProductId", "IsActive", "EffectiveFrom" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_DisplayOrder",
                table: "ProductCategories",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_IsActive",
                table: "ProductCategories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_Name",
                table: "ProductCategories",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ProductCategories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "ProductCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Redemptions_Users_ProcessedBy",
                table: "Redemptions",
                column: "ProcessedBy",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_ProductCategories_CategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Redemptions_Users_ProcessedBy",
                table: "Redemptions");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropIndex(
                name: "IX_Redemptions_ProcessedBy",
                table: "Redemptions");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ProductPricings_ProductId_IsActive_EffectiveFrom",
                table: "ProductPricings");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                table: "Redemptions");

            migrationBuilder.DropColumn(
                name: "ProcessedBy",
                table: "Redemptions");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Products");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPricings_ProductId_IsActive",
                table: "ProductPricings",
                columns: new[] { "ProductId", "IsActive" });
        }
    }
}
