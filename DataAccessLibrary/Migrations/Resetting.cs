using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLibrary.Migrations
{
    /// <inheritdoc />
    public partial class Resetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        //     migrationBuilder.DropForeignKey(
        //         name: "FK_CustomWindowsSpeechCommands_WindowsSpeechVoiceCommand_WindowsSpeechVoiceCommandId",
        //         table: "CustomWindowsSpeechCommands");

        //     migrationBuilder.AlterColumn<string>(
        //         name: "Command",
        //         table: "VisualStudioCommands",
        //         type: "nvarchar(max)",
        //         nullable: false,
        //         defaultValue: "",
        //         oldClrType: typeof(string),
        //         oldType: "nvarchar(max)",
        //         oldNullable: true);

        //     migrationBuilder.AlterColumn<string>(
        //         name: "Caption",
        //         table: "VisualStudioCommands",
        //         type: "nvarchar(max)",
        //         nullable: false,
        //         defaultValue: "",
        //         oldClrType: typeof(string),
        //         oldType: "nvarchar(max)",
        //         oldNullable: true);

        //     migrationBuilder.AlterColumn<string>(
        //         name: "PictureUrl",
        //         table: "TalonAlphabets",
        //         type: "nvarchar(255)",
        //         maxLength: 255,
        //         nullable: false,
        //         defaultValue: "",
        //         oldClrType: typeof(string),
        //         oldType: "nvarchar(255)",
        //         oldMaxLength: 255,
        //         oldNullable: true);

        //     migrationBuilder.AlterColumn<string>(
        //         name: "Username",
        //         table: "Logins",
        //         type: "nvarchar(255)",
        //         maxLength: 255,
        //         nullable: false,
        //         defaultValue: "",
        //         oldClrType: typeof(string),
        //         oldType: "nvarchar(255)",
        //         oldMaxLength: 255,
        //         oldNullable: true);

        //     migrationBuilder.AlterColumn<string>(
        //         name: "Password",
        //         table: "Logins",
        //         type: "nvarchar(255)",
        //         maxLength: 255,
        //         nullable: false,
        //         defaultValue: "",
        //         oldClrType: typeof(string),
        //         oldType: "nvarchar(255)",
        //         oldMaxLength: 255,
        //         oldNullable: true);

        //     migrationBuilder.AlterColumn<string>(
        //         name: "CommandLine",
        //         table: "Launcher",
        //         type: "nvarchar(255)",
        //         maxLength: 255,
        //         nullable: false,
        //         defaultValue: "",
        //         oldClrType: typeof(string),
        //         oldType: "nvarchar(255)",
        //         oldMaxLength: 255,
        //         oldNullable: true);

        //     migrationBuilder.AlterColumn<string>(
        //         name: "Colour",
        //         table: "Launcher",
        //         type: "nvarchar(30)",
        //         maxLength: 30,
        //         nullable: false,
        //         defaultValue: "",
        //         oldClrType: typeof(string),
        //         oldType: "nvarchar(30)",
        //         oldMaxLength: 30,
        //         oldNullable: true);

        //     migrationBuilder.AlterColumn<string>(
        //         name: "StringFormattingMethod",
        //         table: "Idiosyncrasies",
        //         type: "nvarchar(60)",
        //         maxLength: 60,
        //         nullable: false,
        //         defaultValue: "",
        //         oldClrType: typeof(string),
        //         oldType: "nvarchar(60)",
        //         oldMaxLength: 60,
        //         oldNullable: true);

        //     migrationBuilder.AlterColumn<string>(
        //         name: "ReplaceWith",
        //         table: "Idiosyncrasies",
        //         type: "nvarchar(60)",
        //         maxLength: 60,
        //         nullable: false,
        //         defaultValue: "",
        //         oldClrType: typeof(string),
        //         oldType: "nvarchar(60)",
        //         oldMaxLength: 60,
        //         oldNullable: true);

        //     migrationBuilder.AlterColumn<string>(
        //         name: "FindString",
        //         table: "Idiosyncrasies",
        //         type: "nvarchar(60)",
        //         maxLength: 60,
        //         nullable: false,
        //         defaultValue: "",
        //         oldClrType: typeof(string),
        //         oldType: "nvarchar(60)",
        //         oldMaxLength: 60,
        //         oldNullable: true);

        //     migrationBuilder.AlterColumn<string>(
        //         name: "Tag",
        //         table: "HtmlTags",
        //         type: "nvarchar(255)",
        //         maxLength: 255,
        //         nullable: false,
        //         defaultValue: "",
        //         oldClrType: typeof(string),
        //         oldType: "nvarchar(255)",
        //         oldMaxLength: 255,
        //         oldNullable: true);

        //     migrationBuilder.AlterColumn<string>(
        //         name: "ListValue",
        //         table: "HtmlTags",
        //         type: "nvarchar(255)",
        //         maxLength: 255,
        //         nullable: false,
        //         defaultValue: "",
        //         oldClrType: typeof(string),
        //         oldType: "nvarchar(255)",
        //         oldMaxLength: 255,
        //         oldNullable: true);

        //     migrationBuilder.AlterColumn<int>(
        //         name: "WindowsSpeechVoiceCommandId",
        //         table: "CustomWindowsSpeechCommands",
        //         type: "int",
        //         nullable: true,
        //         oldClrType: typeof(int),
        //         oldType: "int");

        //     migrationBuilder.AddForeignKey(
        //         name: "FK_CustomWindowsSpeechCommands_WindowsSpeechVoiceCommand_WindowsSpeechVoiceCommandId",
        //         table: "CustomWindowsSpeechCommands",
        //         column: "WindowsSpeechVoiceCommandId",
        //         principalTable: "WindowsSpeechVoiceCommand",
        //         principalColumn: "Id");
         }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomWindowsSpeechCommands_WindowsSpeechVoiceCommand_WindowsSpeechVoiceCommandId",
                table: "CustomWindowsSpeechCommands");

            migrationBuilder.AlterColumn<string>(
                name: "Command",
                table: "VisualStudioCommands",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Caption",
                table: "VisualStudioCommands",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PictureUrl",
                table: "TalonAlphabets",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Logins",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Logins",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "CommandLine",
                table: "Launcher",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Colour",
                table: "Launcher",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "StringFormattingMethod",
                table: "Idiosyncrasies",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(60)",
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<string>(
                name: "ReplaceWith",
                table: "Idiosyncrasies",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(60)",
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<string>(
                name: "FindString",
                table: "Idiosyncrasies",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(60)",
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<string>(
                name: "Tag",
                table: "HtmlTags",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "ListValue",
                table: "HtmlTags",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<int>(
                name: "WindowsSpeechVoiceCommandId",
                table: "CustomWindowsSpeechCommands",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomWindowsSpeechCommands_WindowsSpeechVoiceCommand_WindowsSpeechVoiceCommandId",
                table: "CustomWindowsSpeechCommands",
                column: "WindowsSpeechVoiceCommandId",
                principalTable: "WindowsSpeechVoiceCommand",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
