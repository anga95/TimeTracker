using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTracker.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDbSetNameToTimeEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkItems_Projects_ProjectId",
                table: "WorkItems");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkItems_WorkDays_WorkDayId",
                table: "WorkItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkItems",
                table: "WorkItems");

            migrationBuilder.RenameTable(
                name: "WorkItems",
                newName: "TimeEntries");

            migrationBuilder.RenameIndex(
                name: "IX_WorkItems_WorkDayId",
                table: "TimeEntries",
                newName: "IX_TimeEntries_WorkDayId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkItems_ProjectId",
                table: "TimeEntries",
                newName: "IX_TimeEntries_ProjectId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimeEntries",
                table: "TimeEntries",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntries_Projects_ProjectId",
                table: "TimeEntries",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntries_WorkDays_WorkDayId",
                table: "TimeEntries",
                column: "WorkDayId",
                principalTable: "WorkDays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntries_Projects_ProjectId",
                table: "TimeEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntries_WorkDays_WorkDayId",
                table: "TimeEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TimeEntries",
                table: "TimeEntries");

            migrationBuilder.RenameTable(
                name: "TimeEntries",
                newName: "WorkItems");

            migrationBuilder.RenameIndex(
                name: "IX_TimeEntries_WorkDayId",
                table: "WorkItems",
                newName: "IX_WorkItems_WorkDayId");

            migrationBuilder.RenameIndex(
                name: "IX_TimeEntries_ProjectId",
                table: "WorkItems",
                newName: "IX_WorkItems_ProjectId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkItems",
                table: "WorkItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItems_Projects_ProjectId",
                table: "WorkItems",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItems_WorkDays_WorkDayId",
                table: "WorkItems",
                column: "WorkDayId",
                principalTable: "WorkDays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
