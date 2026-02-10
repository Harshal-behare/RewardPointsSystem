using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RewardPointsSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenamePointsEntitiesToUserPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename tables to preserve data
            migrationBuilder.RenameTable(
                name: "PointsAccounts",
                newName: "UserPointsAccounts");

            migrationBuilder.RenameTable(
                name: "PointsTransactions",
                newName: "UserPointsTransactions");

            // Rename column in UserPointsTransactions
            migrationBuilder.RenameColumn(
                name: "Points",
                table: "UserPointsTransactions",
                newName: "UserPoints");

            // Drop old check constraints
            migrationBuilder.DropCheckConstraint(
                name: "CK_PointsAccount_CurrentBalance",
                table: "UserPointsAccounts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PointsAccount_TotalEarned",
                table: "UserPointsAccounts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PointsAccount_TotalRedeemed",
                table: "UserPointsAccounts");

            // Add new check constraints with updated names
            migrationBuilder.AddCheckConstraint(
                name: "CK_UserPointsAccount_CurrentBalance",
                table: "UserPointsAccounts",
                sql: "[CurrentBalance] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserPointsAccount_TotalEarned",
                table: "UserPointsAccounts",
                sql: "[TotalEarned] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserPointsAccount_TotalRedeemed",
                table: "UserPointsAccounts",
                sql: "[TotalRedeemed] >= 0");

            // Rename indexes for UserPointsAccounts
            migrationBuilder.RenameIndex(
                name: "IX_PointsAccounts_UpdatedBy",
                table: "UserPointsAccounts",
                newName: "IX_UserPointsAccounts_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_PointsAccounts_UserId",
                table: "UserPointsAccounts",
                newName: "IX_UserPointsAccounts_UserId");

            // Rename indexes for UserPointsTransactions
            migrationBuilder.RenameIndex(
                name: "IX_PointsTransactions_Timestamp",
                table: "UserPointsTransactions",
                newName: "IX_UserPointsTransactions_Timestamp");

            migrationBuilder.RenameIndex(
                name: "IX_PointsTransactions_UserId_Timestamp",
                table: "UserPointsTransactions",
                newName: "IX_UserPointsTransactions_UserId_Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rename tables back
            migrationBuilder.RenameTable(
                name: "UserPointsAccounts",
                newName: "PointsAccounts");

            migrationBuilder.RenameTable(
                name: "UserPointsTransactions",
                newName: "PointsTransactions");

            // Rename column back
            migrationBuilder.RenameColumn(
                name: "UserPoints",
                table: "PointsTransactions",
                newName: "Points");

            // Drop new check constraints
            migrationBuilder.DropCheckConstraint(
                name: "CK_UserPointsAccount_CurrentBalance",
                table: "PointsAccounts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserPointsAccount_TotalEarned",
                table: "PointsAccounts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserPointsAccount_TotalRedeemed",
                table: "PointsAccounts");

            // Add old check constraints back
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

            // Rename indexes back for PointsAccounts
            migrationBuilder.RenameIndex(
                name: "IX_UserPointsAccounts_UpdatedBy",
                table: "PointsAccounts",
                newName: "IX_PointsAccounts_UpdatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_UserPointsAccounts_UserId",
                table: "PointsAccounts",
                newName: "IX_PointsAccounts_UserId");

            // Rename indexes back for PointsTransactions
            migrationBuilder.RenameIndex(
                name: "IX_UserPointsTransactions_Timestamp",
                table: "PointsTransactions",
                newName: "IX_PointsTransactions_Timestamp");

            migrationBuilder.RenameIndex(
                name: "IX_UserPointsTransactions_UserId_Timestamp",
                table: "PointsTransactions",
                newName: "IX_PointsTransactions_UserId_Timestamp");
        }
    }
}
