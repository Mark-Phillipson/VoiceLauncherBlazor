using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLibrary.Migrations
{
    /// <inheritdoc />
    public partial class MIGVariables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Variable1",
                table: "CustomIntelliSense",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Variable2",
                table: "CustomIntelliSense",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Variable3",
                table: "CustomIntelliSense",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Variable1",
                table: "CustomIntelliSense");

            migrationBuilder.DropColumn(
                name: "Variable2",
                table: "CustomIntelliSense");

            migrationBuilder.DropColumn(
                name: "Variable3",
                table: "CustomIntelliSense");
        }
    }
}
