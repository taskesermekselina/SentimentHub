using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SentimentHub.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddComparisonName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ComparisonReports",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "ComparisonReports");
        }
    }
}
