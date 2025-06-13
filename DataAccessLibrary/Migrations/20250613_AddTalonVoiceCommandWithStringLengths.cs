using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccessLibrary.Migrations
{
    public partial class AddTalonVoiceCommandWithStringLengths : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TalonVoiceCommands",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Command = table.Column<string>(maxLength: 100, nullable: false),
                    Script = table.Column<string>(maxLength: 1000, nullable: false),
                    Application = table.Column<string>(maxLength: 100, nullable: false, defaultValue: "global"),
                    Mode = table.Column<string>(maxLength: 100, nullable: true),
                    FilePath = table.Column<string>(maxLength: 250, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TalonVoiceCommands", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TalonVoiceCommands");
        }
    }
}
