using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLibrary.Migrations
{
    /// <inheritdoc />
    public partial class MIGSpokenForms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                table: "Launcher",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "SpokenForm",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpokenFormText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WindowsSpeechVoiceCommandId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpokenForm", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpokenForm_WindowsSpeechVoiceCommand_WindowsSpeechVoiceCommandId",
                        column: x => x.WindowsSpeechVoiceCommandId,
                        principalTable: "WindowsSpeechVoiceCommand",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SpokenForm_SpokenFormText_WindowsSpeechVoiceCommandId",
                table: "SpokenForm",
                columns: new[] { "SpokenFormText", "WindowsSpeechVoiceCommandId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpokenForm_WindowsSpeechVoiceCommandId",
                table: "SpokenForm",
                column: "WindowsSpeechVoiceCommandId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpokenForm");

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                table: "Launcher",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
