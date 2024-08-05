using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLibrary.Migrations
{
    /// <inheritdoc />
    public partial class MoveAndSelectCharactersLeft : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MoveCharactersLeft",
                table: "CustomIntelliSense",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SelectCharactersLeft",
                table: "CustomIntelliSense",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MoveCharactersLeft",
                table: "CustomIntelliSense");

            migrationBuilder.DropColumn(
                name: "SelectCharactersLeft",
                table: "CustomIntelliSense");
        }
    }
}
