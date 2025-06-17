using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguagesAndHostName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodeLanguage",
                table: "TalonVoiceCommands",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Hostname",
                table: "TalonVoiceCommands",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "TalonVoiceCommands",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeLanguage",
                table: "TalonVoiceCommands");

            migrationBuilder.DropColumn(
                name: "Hostname",
                table: "TalonVoiceCommands");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "TalonVoiceCommands");
        }
    }
}
