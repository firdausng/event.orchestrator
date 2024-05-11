using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace events.management.Migrations
{
    /// <inheritdoc />
    public partial class addgoogledestination : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Query",
                table: "Destinations",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Query",
                table: "Destinations");
        }
    }
}
