using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RewardPointsSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminMonthlyBudgets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminMonthlyBudgets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdminUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MonthYear = table.Column<int>(type: "int", nullable: false),
                    BudgetLimit = table.Column<int>(type: "int", nullable: false),
                    PointsAwarded = table.Column<int>(type: "int", nullable: false),
                    IsHardLimit = table.Column<bool>(type: "bit", nullable: false),
                    WarningThreshold = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminMonthlyBudgets", x => x.Id);
                    table.CheckConstraint("CK_AdminMonthlyBudget_BudgetLimit", "[BudgetLimit] > 0");
                    table.CheckConstraint("CK_AdminMonthlyBudget_PointsAwarded", "[PointsAwarded] >= 0");
                    table.CheckConstraint("CK_AdminMonthlyBudget_WarningThreshold", "[WarningThreshold] >= 0 AND [WarningThreshold] <= 100");
                    table.ForeignKey(
                        name: "FK_AdminMonthlyBudgets_Users_AdminUserId",
                        column: x => x.AdminUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminMonthlyBudgets_AdminUserId_MonthYear",
                table: "AdminMonthlyBudgets",
                columns: new[] { "AdminUserId", "MonthYear" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminMonthlyBudgets_MonthYear",
                table: "AdminMonthlyBudgets",
                column: "MonthYear");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminMonthlyBudgets");
        }
    }
}
