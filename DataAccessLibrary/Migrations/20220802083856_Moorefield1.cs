using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLibrary.Migrations
{
    public partial class Moorefield1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationName",
                table: "WindowsSpeechVoiceCommand",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AlternateKey",
                table: "CustomWindowsSpeechCommands",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ControlKey",
                table: "CustomWindowsSpeechCommands",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShiftKey",
                table: "CustomWindowsSpeechCommands",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "WaitTime",
                table: "CustomWindowsSpeechCommands",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationName",
                table: "WindowsSpeechVoiceCommand");

            migrationBuilder.DropColumn(
                name: "AlternateKey",
                table: "CustomWindowsSpeechCommands");

            migrationBuilder.DropColumn(
                name: "ControlKey",
                table: "CustomWindowsSpeechCommands");

            migrationBuilder.DropColumn(
                name: "ShiftKey",
                table: "CustomWindowsSpeechCommands");

            migrationBuilder.DropColumn(
                name: "WaitTime",
                table: "CustomWindowsSpeechCommands");
        }
    }
}
