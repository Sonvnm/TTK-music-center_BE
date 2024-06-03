using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HMZ.Database.Migrations
{
    /// <inheritdoc />
    public partial class Update23 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Classes_ClassId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_StudentClasses_StudentClassId",
                table: "Messages");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Classes_ClassId",
                table: "Messages",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_StudentClasses_StudentClassId",
                table: "Messages",
                column: "StudentClassId",
                principalTable: "StudentClasses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Classes_ClassId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_StudentClasses_StudentClassId",
                table: "Messages");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Classes_ClassId",
                table: "Messages",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_StudentClasses_StudentClassId",
                table: "Messages",
                column: "StudentClassId",
                principalTable: "StudentClasses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
