using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLibrary.Migrations
{
    public partial class MouseCommandColumns1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AbsoluteX",
                table: "CustomWindowsSpeechCommands",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "AbsoluteY",
                table: "CustomWindowsSpeechCommands",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "MouseMoveX",
                table: "CustomWindowsSpeechCommands",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MouseMoveY",
                table: "CustomWindowsSpeechCommands",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ScrollAmount",
                table: "CustomWindowsSpeechCommands",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "WindowsKey",
                table: "CustomWindowsSpeechCommands",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AbsoluteX",
                table: "CustomWindowsSpeechCommands");

            migrationBuilder.DropColumn(
                name: "AbsoluteY",
                table: "CustomWindowsSpeechCommands");

            migrationBuilder.DropColumn(
                name: "MouseMoveX",
                table: "CustomWindowsSpeechCommands");

            migrationBuilder.DropColumn(
                name: "MouseMoveY",
                table: "CustomWindowsSpeechCommands");

            migrationBuilder.DropColumn(
                name: "ScrollAmount",
                table: "CustomWindowsSpeechCommands");

            migrationBuilder.DropColumn(
                name: "WindowsKey",
                table: "CustomWindowsSpeechCommands");
        }
    }
}
