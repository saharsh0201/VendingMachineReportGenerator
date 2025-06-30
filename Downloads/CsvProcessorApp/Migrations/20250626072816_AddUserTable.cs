using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CsvProcessorApp.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MachineWiseStatsJson",
                table: "ProcessingResults");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.AddColumn<string>(
                name: "MachineWiseStatsJson",
                table: "ProcessingResults",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
