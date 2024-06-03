using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HMZ.Database.Migrations
{
    /// <inheritdoc />
    public partial class Update17 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearningProcesses_Classes_ClassId",
                table: "LearningProcesses");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningProcesses_Rooms_RoomId",
                table: "LearningProcesses");

            migrationBuilder.DropIndex(
                name: "IX_LearningProcesses_ClassId",
                table: "LearningProcesses");

            migrationBuilder.DropColumn(
                name: "ClassId",
                table: "LearningProcesses");

            migrationBuilder.RenameColumn(
                name: "RoomId",
                table: "LearningProcesses",
                newName: "ScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_LearningProcesses_RoomId",
                table: "LearningProcesses",
                newName: "IX_LearningProcesses_ScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_LearningProcesses_Schedules_ScheduleId",
                table: "LearningProcesses",
                column: "ScheduleId",
                principalTable: "Schedules",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearningProcesses_Schedules_ScheduleId",
                table: "LearningProcesses");

            migrationBuilder.RenameColumn(
                name: "ScheduleId",
                table: "LearningProcesses",
                newName: "RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_LearningProcesses_ScheduleId",
                table: "LearningProcesses",
                newName: "IX_LearningProcesses_RoomId");

            migrationBuilder.AddColumn<Guid>(
                name: "ClassId",
                table: "LearningProcesses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningProcesses_ClassId",
                table: "LearningProcesses",
                column: "ClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_LearningProcesses_Classes_ClassId",
                table: "LearningProcesses",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LearningProcesses_Rooms_RoomId",
                table: "LearningProcesses",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id");
        }
    }
}
