using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HMZ.Database.Migrations
{
    /// <inheritdoc />
    public partial class update27 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RoomId",
                table: "ScheduleDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleDetails_RoomId",
                table: "ScheduleDetails",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleDetails_Rooms_RoomId",
                table: "ScheduleDetails",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleDetails_Rooms_RoomId",
                table: "ScheduleDetails");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleDetails_RoomId",
                table: "ScheduleDetails");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "ScheduleDetails");
        }
    }
}
