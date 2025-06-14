using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddTagsColumnToTalonVoiceCommands : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Repository",
                table: "TalonVoiceCommands",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "TalonVoiceCommands",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Repository",
                table: "TalonVoiceCommands");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "TalonVoiceCommands");
        }
    }
}
