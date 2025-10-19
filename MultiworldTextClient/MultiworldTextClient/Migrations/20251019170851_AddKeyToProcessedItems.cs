using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiworldTextClient.Migrations
{
    /// <inheritdoc />
    public partial class AddKeyToProcessedItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ProcessedItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProcessedItems",
                table: "ProcessedItems",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProcessedItems",
                table: "ProcessedItems");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ProcessedItems");
        }
    }
}
