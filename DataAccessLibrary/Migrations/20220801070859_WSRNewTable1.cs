using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLibrary.Migrations
{
    public partial class WSRNewTable1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "CustomWindowsSpeechCommands");

            migrationBuilder.DropColumn(
                name: "SpokenCommand",
                table: "CustomWindowsSpeechCommands");

            migrationBuilder.AddColumn<int>(
                name: "WindowsSpeechVoiceCommandId",
                table: "CustomWindowsSpeechCommands",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "WindowsSpeechVoiceCommand",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpokenCommand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WindowsSpeechVoiceCommand", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomWindowsSpeechCommands_WindowsSpeechVoiceCommandId",
                table: "CustomWindowsSpeechCommands",
                column: "WindowsSpeechVoiceCommandId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomWindowsSpeechCommands_WindowsSpeechVoiceCommand_WindowsSpeechVoiceCommandId",
                table: "CustomWindowsSpeechCommands",
                column: "WindowsSpeechVoiceCommandId",
                principalTable: "WindowsSpeechVoiceCommand",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomWindowsSpeechCommands_WindowsSpeechVoiceCommand_WindowsSpeechVoiceCommandId",
                table: "CustomWindowsSpeechCommands");

            migrationBuilder.DropTable(
                name: "WindowsSpeechVoiceCommand");

            migrationBuilder.DropIndex(
                name: "IX_CustomWindowsSpeechCommands_WindowsSpeechVoiceCommandId",
                table: "CustomWindowsSpeechCommands");

            migrationBuilder.DropColumn(
                name: "WindowsSpeechVoiceCommandId",
                table: "CustomWindowsSpeechCommands");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CustomWindowsSpeechCommands",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpokenCommand",
                table: "CustomWindowsSpeechCommands",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
