using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLibrary.Migrations
{
    /// <inheritdoc />
    public partial class LanguageCategoryBridge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.CreateTable(
            //     name: "LauncherCategoryBridge",
            //     columns: table => new
            //     {
            //         ID = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         LauncherID = table.Column<int>(type: "int", nullable: false),
            //         CategoryID = table.Column<int>(type: "int", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_LauncherCategoryBridge", x => x.ID);
            //         table.ForeignKey(
            //             name: "FK_LauncherCategoryBridge_Categories_CategoryID",
            //             column: x => x.CategoryID,
            //             principalTable: "Categories",
            //             principalColumn: "ID",
            //             onDelete: ReferentialAction.Cascade);
            //         table.ForeignKey(
            //             name: "FK_LauncherCategoryBridge_Launcher_LauncherID",
            //             column: x => x.LauncherID,
            //             principalTable: "Launcher",
            //             principalColumn: "ID",
            //             onDelete: ReferentialAction.Cascade);
            //     });

            // migrationBuilder.CreateIndex(
            //     name: "IX_LauncherCategoryBridge_CategoryID",
            //     table: "LauncherCategoryBridge",
            //     column: "CategoryID");

            // migrationBuilder.CreateIndex(
            //     name: "IX_LauncherCategoryBridge_LauncherID",
            //     table: "LauncherCategoryBridge",
            //     column: "LauncherID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LauncherCategoryBridge");
        }
    }
}
