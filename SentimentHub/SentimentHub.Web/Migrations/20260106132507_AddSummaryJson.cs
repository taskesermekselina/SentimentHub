using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SentimentHub.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddSummaryJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SummaryJson",
                table: "Analyses",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SummaryJson",
                table: "Analyses");
        }
    }
}
