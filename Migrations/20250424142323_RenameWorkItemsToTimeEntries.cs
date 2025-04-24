using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTracker.Migrations
{
    /// <inheritdoc />
    public partial class RenameWorkItemsToTimeEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AISummaries",
                table: "AISummaries");

            migrationBuilder.RenameTable(
                name: "AISummaries",
                newName: "AiSummaries");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AiSummaries",
                table: "AiSummaries",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AiSummaries",
                table: "AiSummaries");

            migrationBuilder.RenameTable(
                name: "AiSummaries",
                newName: "AISummaries");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AISummaries",
                table: "AISummaries",
                column: "Id");
        }
    }
}
