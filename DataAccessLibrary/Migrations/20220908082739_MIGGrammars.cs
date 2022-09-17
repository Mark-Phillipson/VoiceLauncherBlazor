using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLibrary.Migrations
{
    public partial class MIGGrammars : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GrammarNames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameOfGrammar = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrammarNames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GrammarItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GrammarNameId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrammarItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrammarItems_GrammarNames_GrammarNameId",
                        column: x => x.GrammarNameId,
                        principalTable: "GrammarNames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GrammarItems_GrammarNameId",
                table: "GrammarItems",
                column: "GrammarNameId");

            migrationBuilder.CreateIndex(
                name: "IX_GrammarNames_NameOfGrammar",
                table: "GrammarNames",
                column: "NameOfGrammar",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GrammarItems");

            migrationBuilder.DropTable(
                name: "GrammarNames");
        }
    }
}
