using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLibrary.Migrations
{
    public partial class NewCommandTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "__MigrationHistory",
            //    columns: table => new
            //    {
            //        MigrationId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
            //        ContextKey = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
            //        Model = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
            //        ProductVersion = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_dbo.__MigrationHistory", x => new { x.MigrationId, x.ContextKey });
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Appointments",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        AppointmentType = table.Column<int>(type: "int", nullable: false),
            //        StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        Caption = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
            //        Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
            //        Location = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
            //        Label = table.Column<int>(type: "int", nullable: false),
            //        Status = table.Column<int>(type: "int", nullable: false),
            //        AllDay = table.Column<bool>(type: "bit", nullable: false),
            //        Recurrence = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Appointments", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AspNetRoles",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetRoles", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AspNetUsers",
            //    columns: table => new
            //    {
            //        Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
            //        PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
            //        TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
            //        LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
            //        LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
            //        AccessFailedCount = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetUsers", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Categories",
            //    columns: table => new
            //    {
            //        ID = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
            //        Category_Type = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
            //        Sensitive = table.Column<bool>(type: "bit", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Categories", x => x.ID);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Computers",
            //    columns: table => new
            //    {
            //        ID = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        ComputerName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Computers", x => x.ID);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CurrentWindow",
            //    columns: table => new
            //    {
            //        ID = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Handle = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CurrentWindow", x => x.ID);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Examples",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        NumberValue = table.Column<int>(type: "int", nullable: false),
            //        Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        LargeText = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Boolean = table.Column<bool>(type: "bit", nullable: false),
            //        DateValue = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Examples", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "GeneralLookups",
            //    columns: table => new
            //    {
            //        ID = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Item_Value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
            //        Category = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
            //        SortOrder = table.Column<int>(type: "int", nullable: true),
            //        DisplayValue = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_GeneralLookups", x => x.ID);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "HtmlTags",
            //    columns: table => new
            //    {
            //        ID = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Tag = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
            //        Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
            //        ListValue = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
            //        Include = table.Column<bool>(type: "bit", nullable: false),
            //        SpokenForm = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_HtmlTags", x => x.ID);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Languages",
            //    columns: table => new
            //    {
            //        ID = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Language = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
            //        Active = table.Column<bool>(type: "bit", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Languages", x => x.ID);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Logins",
            //    columns: table => new
            //    {
            //        ID = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
            //        Username = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
            //        Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
            //        Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Logins", x => x.ID);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "MousePositions",
            //    columns: table => new
            //    {
            //        ID = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Command = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
            //        MouseLeft = table.Column<int>(type: "int", nullable: false),
            //        MouseTop = table.Column<int>(type: "int", nullable: false),
            //        TabPageName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
            //        ControlName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
            //        Application = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_MousePositions", x => x.ID);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "MultipleLauncher",
            //    columns: table => new
            //    {
            //        ID = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Description = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_MultipleLauncher", x => x.ID);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "PropertyTabPositions",
            //    columns: table => new
            //    {
            //        ID = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        ObjectName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
            //        PropertyName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
            //        NumberOfTabs = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_PropertyTabPositions", x => x.ID);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "SavedMousePosition",
            //    columns: table => new
            //    {
            //        ID = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        NamedLocation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
            //        X = table.Column<int>(type: "int", nullable: false),
            //        Y = table.Column<int>(type: "int", nullable: false),
            //        Created = table.Column<DateTime>(type: "datetime", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_SavedMousePosition", x => x.ID);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Todos",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
            //        Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
            //        Completed = table.Column<bool>(type: "bit", nullable: false),
            //        Project = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
            //        Archived = table.Column<bool>(type: "bit", nullable: false),
            //        Created = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        SortPriority = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Todos", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "ValuesToInsert",
            //    columns: table => new
            //    {
            //        ID = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        ValueToInsert = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
            //        Lookup = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
            //        Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_ValuesToInsert", x => x.ID);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "VisualStudioCommands",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Caption = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Command = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_VisualStudioCommands", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AspNetRoleClaims",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
            //            column: x => x.RoleId,
            //            principalTable: "AspNetRoles",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AspNetUserClaims",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_AspNetUserClaims_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AspNetUserLogins",
            //    columns: table => new
            //    {
            //        LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
            //        ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
            //        ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
            //        table.ForeignKey(
            //            name: "FK_AspNetUserLogins_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AspNetUserRoles",
            //    columns: table => new
            //    {
            //        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
            //        table.ForeignKey(
            //            name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
            //            column: x => x.RoleId,
            //            principalTable: "AspNetRoles",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_AspNetUserRoles_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AspNetUserTokens",
            //    columns: table => new
            //    {
            //        UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
            //        Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
            //        Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
            //        table.ForeignKey(
            //            name: "FK_AspNetUserTokens_AspNetUsers_UserId",
            //            column: x => x.UserId,
            //            principalTable: "AspNetUsers",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "Launcher",
            //    columns: table => new
            //    {
            //        ID = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
            //        CommandLine = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
            //        CategoryID = table.Column<int>(type: "int", nullable: false),
            //        ComputerID = table.Column<int>(type: "int", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Launcher", x => x.ID);
            //        table.ForeignKey(
            //            name: "FK_dbo.Launcher_dbo.Categories_CategoryID",
            //            column: x => x.CategoryID,
            //            principalTable: "Categories",
            //            principalColumn: "ID",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_dbo.Launcher_dbo.Computers_ComputerID",
            //            column: x => x.ComputerID,
            //            principalTable: "Computers",
            //            principalColumn: "ID");
            //    });

            //migrationBuilder.CreateTable(
            //    name: "CustomIntelliSense",
            //    columns: table => new
            //    {
            //        ID = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        LanguageID = table.Column<int>(type: "int", nullable: false),
            //        Display_Value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
            //        SendKeys_Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Command_Type = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
            //        CategoryID = table.Column<int>(type: "int", nullable: false),
            //        Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
            //        Search = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ComputerID = table.Column<int>(type: "int", nullable: true),
            //        DeliveryType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_CustomIntelliSense", x => x.ID);
            //        table.ForeignKey(
            //            name: "FK_dbo.CustomIntelliSense_dbo.Categories_CategoryID",
            //            column: x => x.CategoryID,
            //            principalTable: "Categories",
            //            principalColumn: "ID",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_dbo.CustomIntelliSense_dbo.Computers_ComputerID",
            //            column: x => x.ComputerID,
            //            principalTable: "Computers",
            //            principalColumn: "ID");
            //        table.ForeignKey(
            //            name: "FK_dbo.CustomIntelliSense_dbo.Languages_LanguageID",
            //            column: x => x.LanguageID,
            //            principalTable: "Languages",
            //            principalColumn: "ID",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "LauncherMultipleLauncherBridge",
            //    columns: table => new
            //    {
            //        ID = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        LauncherID = table.Column<int>(type: "int", nullable: false),
            //        MultipleLauncherID = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_LauncherMultipleLauncherBridge", x => x.ID);
            //        table.ForeignKey(
            //            name: "FK_LauncherMultipleLauncherBridge_Launcher_LauncherID",
            //            column: x => x.LauncherID,
            //            principalTable: "Launcher",
            //            principalColumn: "ID",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_LauncherMultipleLauncherBridge_MultipleLauncher_MultipleLauncherID",
            //            column: x => x.MultipleLauncherID,
            //            principalTable: "MultipleLauncher",
            //            principalColumn: "ID",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AdditionalCommands",
            //    columns: table => new
            //    {
            //        ID = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        CustomIntelliSenseID = table.Column<int>(type: "int", nullable: false),
            //        WaitBefore = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
            //        SendKeys_Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
            //        DeliveryType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AdditionalCommands", x => x.ID);
            //        table.ForeignKey(
            //            name: "FK_AdditionalCommands_CustomIntelliSense_CustomIntelliSenseID",
            //            column: x => x.CustomIntelliSenseID,
            //            principalTable: "CustomIntelliSense",
            //            principalColumn: "ID",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_AdditionalCommands_CustomIntelliSenseID",
            //    table: "AdditionalCommands",
            //    column: "CustomIntelliSenseID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetRoleClaims_RoleId",
            //    table: "AspNetRoleClaims",
            //    column: "RoleId");

            //migrationBuilder.CreateIndex(
            //    name: "RoleNameIndex",
            //    table: "AspNetRoles",
            //    column: "NormalizedName",
            //    unique: true,
            //    filter: "([NormalizedName] IS NOT NULL)");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetUserClaims_UserId",
            //    table: "AspNetUserClaims",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetUserLogins_UserId",
            //    table: "AspNetUserLogins",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AspNetUserRoles_RoleId",
            //    table: "AspNetUserRoles",
            //    column: "RoleId");

            //migrationBuilder.CreateIndex(
            //    name: "EmailIndex",
            //    table: "AspNetUsers",
            //    column: "NormalizedEmail");

            //migrationBuilder.CreateIndex(
            //    name: "UserNameIndex",
            //    table: "AspNetUsers",
            //    column: "NormalizedUserName",
            //    unique: true,
            //    filter: "([NormalizedUserName] IS NOT NULL)");

            //migrationBuilder.CreateIndex(
            //    name: "IX_CategoryID",
            //    table: "CustomIntelliSense",
            //    column: "CategoryID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_ComputerID",
            //    table: "CustomIntelliSense",
            //    column: "ComputerID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_LanguageID",
            //    table: "CustomIntelliSense",
            //    column: "LanguageID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_CategoryID",
            //    table: "Launcher",
            //    column: "CategoryID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_ComputerID",
            //    table: "Launcher",
            //    column: "ComputerID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_LauncherMultipleLauncherBridge_LauncherID",
            //    table: "LauncherMultipleLauncherBridge",
            //    column: "LauncherID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_LauncherMultipleLauncherBridge_MultipleLauncherID",
            //    table: "LauncherMultipleLauncherBridge",
            //    column: "MultipleLauncherID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "__MigrationHistory");

            migrationBuilder.DropTable(
                name: "AdditionalCommands");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CurrentWindow");

            migrationBuilder.DropTable(
                name: "Examples");

            migrationBuilder.DropTable(
                name: "GeneralLookups");

            migrationBuilder.DropTable(
                name: "HtmlTags");

            migrationBuilder.DropTable(
                name: "LauncherMultipleLauncherBridge");

            migrationBuilder.DropTable(
                name: "Logins");

            migrationBuilder.DropTable(
                name: "MousePositions");

            migrationBuilder.DropTable(
                name: "PropertyTabPositions");

            migrationBuilder.DropTable(
                name: "SavedMousePosition");

            migrationBuilder.DropTable(
                name: "Todos");

            migrationBuilder.DropTable(
                name: "ValuesToInsert");

            migrationBuilder.DropTable(
                name: "VisualStudioCommands");

            migrationBuilder.DropTable(
                name: "CustomIntelliSense");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Launcher");

            migrationBuilder.DropTable(
                name: "MultipleLauncher");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Computers");
        }
    }
}
