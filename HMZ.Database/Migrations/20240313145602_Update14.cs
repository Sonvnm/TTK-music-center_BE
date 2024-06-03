using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HMZ.Database.Migrations
{
    /// <inheritdoc />
    public partial class Update14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Subjects_SubjectId",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Schedules");

            migrationBuilder.RenameColumn(
                name: "SubjectId",
                table: "Schedules",
                newName: "CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_Schedules_SubjectId",
                table: "Schedules",
                newName: "IX_Schedules_CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Courses_CourseId",
                table: "Schedules",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Courses_CourseId",
                table: "Schedules");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "Schedules",
                newName: "SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Schedules_CourseId",
                table: "Schedules",
                newName: "IX_Schedules_SubjectId");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Schedules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Subjects_SubjectId",
                table: "Schedules",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id");
        }
    }
}
