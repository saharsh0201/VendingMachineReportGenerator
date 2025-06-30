using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CsvProcessorApp.Migrations
{
    /// <inheritdoc />
    public partial class AddMachineWiseStatsJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MachineWiseStatsJson",
                table: "ProcessingResults",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MachineWiseStatsJson",
                table: "ProcessingResults");
        }
    }
}
