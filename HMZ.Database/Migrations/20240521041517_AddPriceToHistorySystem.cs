using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HMZ.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceToHistorySystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "HistorySystems",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "HistorySystems");
        }
    }
}
