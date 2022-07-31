using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLibrary.Migrations
{
    public partial class NewCommandTable2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomWindowsSpeechCommands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TextToEnter = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    KeyDownValue = table.Column<int>(type: "int", nullable: true),
                    ModifierKey = table.Column<int>(type: "int", nullable: true),
                    KeyPressValue = table.Column<int>(type: "int", nullable: true),
                    MouseCommand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProcessStart = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CommandLineArguments = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomWindowsSpeechCommands", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomWindowsSpeechCommands");
        }
    }
}
