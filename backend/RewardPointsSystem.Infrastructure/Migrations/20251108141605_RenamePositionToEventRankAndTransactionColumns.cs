using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RewardPointsSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenamePositionToEventRankAndTransactionColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "PointsTransactions",
                newName: "TransactionType");

            migrationBuilder.RenameColumn(
                name: "Source",
                table: "PointsTransactions",
                newName: "TransactionSource");

            migrationBuilder.RenameColumn(
                name: "Position",
                table: "EventParticipants",
                newName: "EventRank");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TransactionType",
                table: "PointsTransactions",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "TransactionSource",
                table: "PointsTransactions",
                newName: "Source");

            migrationBuilder.RenameColumn(
                name: "EventRank",
                table: "EventParticipants",
                newName: "Position");
        }
    }
}
