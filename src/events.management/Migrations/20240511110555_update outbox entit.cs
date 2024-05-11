using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace events.management.Migrations
{
    /// <inheritdoc />
    public partial class updateoutboxentit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClrType",
                table: "OutboxMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntryState",
                table: "OutboxMessages",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClrType",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "EntryState",
                table: "OutboxMessages");
        }
    }
}
