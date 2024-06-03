using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HMZ.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDB_addColumType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubjectCourse_Courses_CourseId",
                table: "SubjectCourse");

            migrationBuilder.DropForeignKey(
                name: "FK_SubjectCourse_Subjects_SubjectId",
                table: "SubjectCourse");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubjectCourse",
                table: "SubjectCourse");

            migrationBuilder.RenameTable(
                name: "SubjectCourse",
                newName: "SubjectCourses");

            migrationBuilder.RenameIndex(
                name: "IX_SubjectCourse_SubjectId",
                table: "SubjectCourses",
                newName: "IX_SubjectCourses_SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_SubjectCourse_CourseId",
                table: "SubjectCourses",
                newName: "IX_SubjectCourses_CourseId");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "HistorySystems",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubjectCourses",
                table: "SubjectCourses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectCourses_Courses_CourseId",
                table: "SubjectCourses",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectCourses_Subjects_SubjectId",
                table: "SubjectCourses",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubjectCourses_Courses_CourseId",
                table: "SubjectCourses");

            migrationBuilder.DropForeignKey(
                name: "FK_SubjectCourses_Subjects_SubjectId",
                table: "SubjectCourses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubjectCourses",
                table: "SubjectCourses");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "HistorySystems");

            migrationBuilder.RenameTable(
                name: "SubjectCourses",
                newName: "SubjectCourse");

            migrationBuilder.RenameIndex(
                name: "IX_SubjectCourses_SubjectId",
                table: "SubjectCourse",
                newName: "IX_SubjectCourse_SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_SubjectCourses_CourseId",
                table: "SubjectCourse",
                newName: "IX_SubjectCourse_CourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubjectCourse",
                table: "SubjectCourse",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectCourse_Courses_CourseId",
                table: "SubjectCourse",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectCourse_Subjects_SubjectId",
                table: "SubjectCourse",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id");
        }
    }
}
