using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLibrary.Migrations
{
    /// <inheritdoc />
    public partial class InitialSQLiteSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "__MigrationHistory",
                columns: table => new
                {
                    MigrationId = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    ContextKey = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Model = table.Column<byte[]>(type: "BLOB", nullable: false),
                    ProductVersion = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.__MigrationHistory", x => new { x.MigrationId, x.ContextKey });
                });

            migrationBuilder.CreateTable(
                name: "ApplicationDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProcessName = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    ApplicationTitle = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AppointmentType = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Caption = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Location = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Label = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    AllDay = table.Column<bool>(type: "INTEGER", nullable: false),
                    Recurrence = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Category = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Category_Type = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Sensitive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Colour = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    Icon = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Computers",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ComputerName = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Computers", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "CssProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PropertyName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CssProperties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CurrentWindow",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Handle = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrentWindow", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "CursorlessCheatsheetItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SpokenForm = table.Column<string>(type: "TEXT", nullable: false),
                    Meaning = table.Column<string>(type: "TEXT", nullable: true),
                    CursorlessType = table.Column<string>(type: "TEXT", nullable: true),
                    YoutubeLink = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CursorlessCheatsheetItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Examples",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NumberValue = table.Column<int>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", nullable: false),
                    LargeText = table.Column<string>(type: "TEXT", nullable: false),
                    Boolean = table.Column<bool>(type: "INTEGER", nullable: false),
                    DateValue = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Examples", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FaceImages",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ImageName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ImageData = table.Column<string>(type: "TEXT", nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    UploadDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaceImages", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "GeneralLookups",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Item_Value = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: true),
                    DisplayValue = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralLookups", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "GrammarNames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NameOfGrammar = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrammarNames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HtmlTags",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tag = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ListValue = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Include = table.Column<bool>(type: "INTEGER", nullable: false),
                    SpokenForm = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HtmlTags", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Idiosyncrasies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FindString = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    ReplaceWith = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    StringFormattingMethod = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Idiosyncrasies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Language = table.Column<string>(type: "TEXT", maxLength: 25, nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false),
                    Colour = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    ImageLink = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Logins",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Password = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logins", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Microphones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MicrophoneName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Default = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Microphones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MousePositions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Command = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    MouseLeft = table.Column<int>(type: "INTEGER", nullable: false),
                    MouseTop = table.Column<int>(type: "INTEGER", nullable: false),
                    TabPageName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ControlName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Application = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MousePositions", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "MultipleLauncher",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 70, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultipleLauncher", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PhraseListGrammars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PhraseListGrammarValue = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhraseListGrammars", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Prompts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PromptText = table.Column<string>(type: "TEXT", maxLength: 3000, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prompts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PropertyTabPositions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ObjectName = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    PropertyName = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    NumberOfTabs = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyTabPositions", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "QuickPrompts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Command = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PromptText = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuickPrompts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SavedMousePosition",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NamedLocation = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    X = table.Column<int>(type: "INTEGER", nullable: false),
                    Y = table.Column<int>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedMousePosition", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TalonAlphabets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Letter = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PictureUrl = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    DefaultLetter = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DefaultPictureUrl = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TalonAlphabets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TalonLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ListName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SpokenForm = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ListValue = table.Column<string>(type: "TEXT", maxLength: 700, nullable: false),
                    SourceFile = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TalonLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TalonVoiceCommands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Command = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Script = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Application = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Mode = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    OperatingSystem = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Repository = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CodeLanguage = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Language = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Hostname = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TalonVoiceCommands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Todos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Completed = table.Column<bool>(type: "INTEGER", nullable: false),
                    Project = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Archived = table.Column<bool>(type: "INTEGER", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SortPriority = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Todos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    Type = table.Column<string>(type: "TEXT", maxLength: 70, nullable: true),
                    MoneyIn = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MoneyOut = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MyTransactionType = table.Column<string>(type: "TEXT", maxLength: 70, nullable: true),
                    ImportFilename = table.Column<string>(type: "TEXT", nullable: true),
                    ImportDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionTypeMapping",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MyTransactionType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionTypeMapping", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ValuesToInsert",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ValueToInsert = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Lookup = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValuesToInsert", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "VisualStudioCommands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Caption = table.Column<string>(type: "TEXT", nullable: false),
                    Command = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisualStudioCommands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WindowsSpeechVoiceCommand",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SpokenCommand = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ApplicationName = table.Column<string>(type: "TEXT", nullable: true),
                    AutoCreated = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WindowsSpeechVoiceCommand", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Launcher",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CommandLine = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    WorkingDirectory = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Arguments = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    CategoryID = table.Column<int>(type: "INTEGER", nullable: false),
                    ComputerID = table.Column<int>(type: "INTEGER", nullable: true),
                    Colour = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Icon = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Favourite = table.Column<bool>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Launcher", x => x.ID);
                    table.ForeignKey(
                        name: "FK_dbo.Launcher_dbo.Categories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "Categories",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.Launcher_dbo.Computers_ComputerID",
                        column: x => x.ComputerID,
                        principalTable: "Computers",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "FaceTags",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FaceImageId = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    X = table.Column<double>(type: "REAL", nullable: false),
                    Y = table.Column<double>(type: "REAL", nullable: false),
                    Width = table.Column<double>(type: "REAL", nullable: false),
                    Height = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaceTags", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FaceTags_FaceImages_FaceImageId",
                        column: x => x.FaceImageId,
                        principalTable: "FaceImages",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrammarItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GrammarNameId = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false)
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

            migrationBuilder.CreateTable(
                name: "CustomIntelliSense",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LanguageID = table.Column<int>(type: "INTEGER", nullable: false),
                    Display_Value = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    SendKeys_Value = table.Column<string>(type: "TEXT", nullable: false),
                    Command_Type = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    CategoryID = table.Column<int>(type: "INTEGER", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Search = table.Column<string>(type: "TEXT", nullable: true),
                    ComputerID = table.Column<int>(type: "INTEGER", nullable: true),
                    DeliveryType = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Variable1 = table.Column<string>(type: "TEXT", maxLength: 60, nullable: true),
                    Variable2 = table.Column<string>(type: "TEXT", maxLength: 60, nullable: true),
                    Variable3 = table.Column<string>(type: "TEXT", maxLength: 60, nullable: true),
                    SelectWordFromRight = table.Column<int>(type: "INTEGER", nullable: false),
                    MoveCharactersLeft = table.Column<int>(type: "INTEGER", nullable: false),
                    SelectCharactersLeft = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomIntelliSense", x => x.ID);
                    table.ForeignKey(
                        name: "FK_dbo.CustomIntelliSense_dbo.Categories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "Categories",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.CustomIntelliSense_dbo.Computers_ComputerID",
                        column: x => x.ComputerID,
                        principalTable: "Computers",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_dbo.CustomIntelliSense_dbo.Languages_LanguageID",
                        column: x => x.LanguageID,
                        principalTable: "Languages",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomWindowsSpeechCommands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WindowsSpeechVoiceCommandId = table.Column<int>(type: "INTEGER", nullable: true),
                    TextToEnter = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    SendKeysValue = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    KeyDownValue = table.Column<int>(type: "INTEGER", nullable: true),
                    ModifierKey = table.Column<int>(type: "INTEGER", nullable: true),
                    ControlKey = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShiftKey = table.Column<bool>(type: "INTEGER", nullable: false),
                    AlternateKey = table.Column<bool>(type: "INTEGER", nullable: false),
                    WindowsKey = table.Column<bool>(type: "INTEGER", nullable: false),
                    KeyPressValue = table.Column<int>(type: "INTEGER", nullable: true),
                    KeyUpValue = table.Column<int>(type: "INTEGER", nullable: true),
                    MouseCommand = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    MouseMoveX = table.Column<int>(type: "INTEGER", nullable: false),
                    MouseMoveY = table.Column<int>(type: "INTEGER", nullable: false),
                    AbsoluteX = table.Column<double>(type: "REAL", nullable: false),
                    AbsoluteY = table.Column<double>(type: "REAL", nullable: false),
                    ScrollAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    ProcessStart = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    CommandLineArguments = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    WaitTime = table.Column<int>(type: "INTEGER", nullable: false),
                    HowToFormatDictation = table.Column<string>(type: "TEXT", maxLength: 55, nullable: true),
                    MethodToCall = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomWindowsSpeechCommands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomWindowsSpeechCommands_WindowsSpeechVoiceCommand_WindowsSpeechVoiceCommandId",
                        column: x => x.WindowsSpeechVoiceCommandId,
                        principalTable: "WindowsSpeechVoiceCommand",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SpokenForm",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SpokenFormText = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    WindowsSpeechVoiceCommandId = table.Column<int>(type: "INTEGER", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "LauncherCategoryBridge",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LauncherID = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoryID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LauncherCategoryBridge", x => x.ID);
                    table.ForeignKey(
                        name: "FK_LauncherCategoryBridge_Categories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "Categories",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LauncherCategoryBridge_Launcher_LauncherID",
                        column: x => x.LauncherID,
                        principalTable: "Launcher",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LauncherMultipleLauncherBridge",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LauncherID = table.Column<int>(type: "INTEGER", nullable: false),
                    MultipleLauncherID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LauncherMultipleLauncherBridge", x => x.ID);
                    table.ForeignKey(
                        name: "FK_LauncherMultipleLauncherBridge_Launcher_LauncherID",
                        column: x => x.LauncherID,
                        principalTable: "Launcher",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LauncherMultipleLauncherBridge_MultipleLauncher_MultipleLauncherID",
                        column: x => x.MultipleLauncherID,
                        principalTable: "MultipleLauncher",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdditionalCommands",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustomIntelliSenseID = table.Column<int>(type: "INTEGER", nullable: false),
                    WaitBefore = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    SendKeys_Value = table.Column<string>(type: "TEXT", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DeliveryType = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalCommands", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AdditionalCommands_CustomIntelliSense_CustomIntelliSenseID",
                        column: x => x.CustomIntelliSenseID,
                        principalTable: "CustomIntelliSense",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalCommands_CustomIntelliSenseID",
                table: "AdditionalCommands",
                column: "CustomIntelliSenseID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomIntelliSense_CategoryID",
                table: "CustomIntelliSense",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomIntelliSense_ComputerID",
                table: "CustomIntelliSense",
                column: "ComputerID");

            migrationBuilder.CreateIndex(
                name: "IX_LanguageID",
                table: "CustomIntelliSense",
                column: "LanguageID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomWindowsSpeechCommands_WindowsSpeechVoiceCommandId",
                table: "CustomWindowsSpeechCommands",
                column: "WindowsSpeechVoiceCommandId");

            migrationBuilder.CreateIndex(
                name: "IX_FaceTags_FaceImageId",
                table: "FaceTags",
                column: "FaceImageId");

            migrationBuilder.CreateIndex(
                name: "IX_GrammarItems_GrammarNameId",
                table: "GrammarItems",
                column: "GrammarNameId");

            migrationBuilder.CreateIndex(
                name: "IX_GrammarNames_NameOfGrammar",
                table: "GrammarNames",
                column: "NameOfGrammar",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Launcher_CategoryID",
                table: "Launcher",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Launcher_ComputerID",
                table: "Launcher",
                column: "ComputerID");

            migrationBuilder.CreateIndex(
                name: "IX_LauncherCategoryBridge_CategoryID",
                table: "LauncherCategoryBridge",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_LauncherCategoryBridge_LauncherID",
                table: "LauncherCategoryBridge",
                column: "LauncherID");

            migrationBuilder.CreateIndex(
                name: "IX_LauncherMultipleLauncherBridge_LauncherID",
                table: "LauncherMultipleLauncherBridge",
                column: "LauncherID");

            migrationBuilder.CreateIndex(
                name: "IX_LauncherMultipleLauncherBridge_MultipleLauncherID",
                table: "LauncherMultipleLauncherBridge",
                column: "MultipleLauncherID");

            migrationBuilder.CreateIndex(
                name: "IX_SpokenForm_SpokenFormText_WindowsSpeechVoiceCommandId",
                table: "SpokenForm",
                columns: new[] { "SpokenFormText", "WindowsSpeechVoiceCommandId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpokenForm_WindowsSpeechVoiceCommandId",
                table: "SpokenForm",
                column: "WindowsSpeechVoiceCommandId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypeMapping_MyTransactionType_Type",
                table: "TransactionTypeMapping",
                columns: new[] { "MyTransactionType", "Type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "__MigrationHistory");

            migrationBuilder.DropTable(
                name: "AdditionalCommands");

            migrationBuilder.DropTable(
                name: "ApplicationDetails");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "CssProperties");

            migrationBuilder.DropTable(
                name: "CurrentWindow");

            migrationBuilder.DropTable(
                name: "CursorlessCheatsheetItems");

            migrationBuilder.DropTable(
                name: "CustomWindowsSpeechCommands");

            migrationBuilder.DropTable(
                name: "Examples");

            migrationBuilder.DropTable(
                name: "FaceTags");

            migrationBuilder.DropTable(
                name: "GeneralLookups");

            migrationBuilder.DropTable(
                name: "GrammarItems");

            migrationBuilder.DropTable(
                name: "HtmlTags");

            migrationBuilder.DropTable(
                name: "Idiosyncrasies");

            migrationBuilder.DropTable(
                name: "LauncherCategoryBridge");

            migrationBuilder.DropTable(
                name: "LauncherMultipleLauncherBridge");

            migrationBuilder.DropTable(
                name: "Logins");

            migrationBuilder.DropTable(
                name: "Microphones");

            migrationBuilder.DropTable(
                name: "MousePositions");

            migrationBuilder.DropTable(
                name: "PhraseListGrammars");

            migrationBuilder.DropTable(
                name: "Prompts");

            migrationBuilder.DropTable(
                name: "PropertyTabPositions");

            migrationBuilder.DropTable(
                name: "QuickPrompts");

            migrationBuilder.DropTable(
                name: "SavedMousePosition");

            migrationBuilder.DropTable(
                name: "SpokenForm");

            migrationBuilder.DropTable(
                name: "TalonAlphabets");

            migrationBuilder.DropTable(
                name: "TalonLists");

            migrationBuilder.DropTable(
                name: "TalonVoiceCommands");

            migrationBuilder.DropTable(
                name: "Todos");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "TransactionTypeMapping");

            migrationBuilder.DropTable(
                name: "ValuesToInsert");

            migrationBuilder.DropTable(
                name: "VisualStudioCommands");

            migrationBuilder.DropTable(
                name: "CustomIntelliSense");

            migrationBuilder.DropTable(
                name: "FaceImages");

            migrationBuilder.DropTable(
                name: "GrammarNames");

            migrationBuilder.DropTable(
                name: "Launcher");

            migrationBuilder.DropTable(
                name: "MultipleLauncher");

            migrationBuilder.DropTable(
                name: "WindowsSpeechVoiceCommand");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Computers");
        }
    }
}
