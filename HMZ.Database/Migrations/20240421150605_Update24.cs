using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HMZ.Database.Migrations
{
    /// <inheritdoc />
    public partial class Update24 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearningProcesses_Schedules_ScheduleId",
                table: "LearningProcesses");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "LearningProcesses");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "LearningProcesses");

            migrationBuilder.AddColumn<Guid>(
                name: "ScheduleDetailId",
                table: "LearningProcesses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ScheduleDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleDetails_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearningProcesses_ScheduleDetailId",
                table: "LearningProcesses",
                column: "ScheduleDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleDetails_ScheduleId",
                table: "ScheduleDetails",
                column: "ScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_LearningProcesses_ScheduleDetails_ScheduleDetailId",
                table: "LearningProcesses",
                column: "ScheduleDetailId",
                principalTable: "ScheduleDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_LearningProcesses_ScheduleDetails_ScheduleDetailId",
                table: "LearningProcesses");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningProcesses_Schedules_ScheduleId",
                table: "LearningProcesses");

            migrationBuilder.DropTable(
                name: "ScheduleDetails");

            migrationBuilder.DropIndex(
                name: "IX_LearningProcesses_ScheduleDetailId",
                table: "LearningProcesses");

            migrationBuilder.DropColumn(
                name: "ScheduleDetailId",
                table: "LearningProcesses");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "LearningProcesses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "LearningProcesses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningProcesses_Schedules_ScheduleId",
                table: "LearningProcesses",
                column: "ScheduleId",
                principalTable: "Schedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
