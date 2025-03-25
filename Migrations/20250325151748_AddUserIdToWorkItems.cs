using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToWorkItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "WorkItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "WorkItems");
        }
    }
}
