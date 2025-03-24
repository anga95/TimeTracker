using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectName",
                table: "WorkItems");

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "WorkItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_ProjectId",
                table: "WorkItems",
                column: "ProjectId");

            // Efter att tabellen Projects har skapats, lägg in ett standardprojekt:
            migrationBuilder.Sql("INSERT INTO Projects (Name) VALUES ('Övrigt');");

            // Uppdatera befintliga WorkItems (där ProjectId = 0) att använda det nya standardprojektets Id.
            // OBS! Om Projects-tabellen är tom, kommer Standardprojekt att få Id 1 (om inga andra rader finns).
            migrationBuilder.Sql("UPDATE WorkItems SET ProjectId = (SELECT TOP 1 Id FROM Projects WHERE Name = 'Övrigt') WHERE ProjectId = 0;");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItems_Projects_ProjectId",
                table: "WorkItems",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkItems_Projects_ProjectId",
                table: "WorkItems");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_WorkItems_ProjectId",
                table: "WorkItems");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "WorkItems");

            migrationBuilder.AddColumn<string>(
                name: "ProjectName",
                table: "WorkItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
