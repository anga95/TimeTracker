using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTracker.Migrations
{
    /// <inheritdoc />
    public partial class FixTicketFieldsAndLoggedAtDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE [TimeEntries]
SET [WorkDate] = DATEFROMPARTS(YEAR([WorkDate]), MONTH([WorkDate]), DAY([WorkDate]))
WHERE [WorkDate] IS NOT NULL
");
            
            migrationBuilder.AlterColumn<DateTime>(
                name: "WorkDate",
                table: "TimeEntries",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
            
            migrationBuilder.Sql(@"
UPDATE [TimeEntries]
SET [LoggedAt] = SYSUTCDATETIME()
WHERE [LoggedAt] IS NULL
   OR (DATEPART(year, [LoggedAt]) = 1 AND DATEPART(month, [LoggedAt]) = 1 AND DATEPART(day, [LoggedAt]) = 1)
");

            migrationBuilder.AlterColumn<string>(
                name: "TicketUrl",
                table: "TimeEntries",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TicketKey",
                table: "TimeEntries",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
            
            migrationBuilder.Sql(@"
DECLARE @df sysname;
SELECT @df = d.name
FROM sys.default_constraints d
JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id
WHERE d.parent_object_id = OBJECT_ID('dbo.TimeEntries') AND c.name = 'LoggedAt';
IF @df IS NOT NULL
    EXEC('ALTER TABLE dbo.TimeEntries DROP CONSTRAINT [' + @df + ']');
");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "LoggedAt",
                table: "TimeEntries",
                type: "datetimeoffset(7)",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");
            
            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_TicketKey",
                table: "TimeEntries",
                column: "TicketKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "WorkDate",
                table: "TimeEntries",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<string>(
                name: "TicketUrl",
                table: "TimeEntries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(512)",
                oldMaxLength: 512,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TicketKey",
                table: "TimeEntries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "LoggedAt",
                table: "TimeEntries",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset(7)");
            
            migrationBuilder.DropIndex(
                name: "IX_TimeEntries_TicketKey",
                table: "TimeEntries");
        }
    }
}
