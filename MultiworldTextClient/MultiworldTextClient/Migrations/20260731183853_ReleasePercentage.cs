using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiworldTextClient.Migrations
{
    /// <inheritdoc />
    public partial class ReleasePercentage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ReleasePercent",
                table: "TrackedWorlds",
                type: "REAL",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReleasePercent",
                table: "TrackedWorlds");
        }
    }
}
