using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CsvProcessorApp.Migrations
{
    /// <inheritdoc />
    public partial class InitDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessingResults",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalUsedAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    TotalRefundedAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    FullyRefundedAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    PartiallyRefundedAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    TotalTransactions = table.Column<int>(type: "INTEGER", nullable: false),
                    RefundedTransactions = table.Column<int>(type: "INTEGER", nullable: false),
                    PartiallyRefundedTransactions = table.Column<int>(type: "INTEGER", nullable: false),
                    ResultFilePath = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessingResults", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessingResults");
        }
    }
}
