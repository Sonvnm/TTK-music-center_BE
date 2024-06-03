using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HMZ.Database.Migrations
{
    /// <inheritdoc />
    public partial class IsMakeUpClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMakeUpClass",
                table: "ScheduleDetails",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMakeUpClass",
                table: "ScheduleDetails");
        }
    }
}
