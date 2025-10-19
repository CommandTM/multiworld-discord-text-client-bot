using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiworldTextClient.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessedItems",
                columns: table => new
                {
                    TrackerUuid = table.Column<string>(type: "TEXT", nullable: false),
                    ItemId = table.Column<long>(type: "INTEGER", nullable: false),
                    LocationId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessedItems");
        }
    }
}
