using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RewardPointsSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBalanceAfterToTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PointsTransactions_UserId",
                table: "PointsTransactions");

            migrationBuilder.AddColumn<int>(
                name: "BalanceAfter",
                table: "PointsTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PointsTransactions_UserId_Timestamp",
                table: "PointsTransactions",
                columns: new[] { "UserId", "Timestamp" },
                descending: new[] { false, true })
                .Annotation("SqlServer:Include", new[] { "BalanceAfter", "Points" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PointsTransactions_UserId_Timestamp",
                table: "PointsTransactions");

            migrationBuilder.DropColumn(
                name: "BalanceAfter",
                table: "PointsTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_PointsTransactions_UserId",
                table: "PointsTransactions",
                column: "UserId");
        }
    }
}
