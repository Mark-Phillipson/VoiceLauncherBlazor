using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataAccessLibrary.Models;
using DataAccessLibrary.Services;
using DataAccessLibrary;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLibrary
{
    /// <summary>
    /// Diagnostic utility to find TalonList entries that exceed database column size limits
    /// </summary>
    public class TalonListDiagnostics
    {
        /// <summary>
        /// Creates PowerShell commands for copying and pasting to diagnose column size issues
        /// </summary>
        /// <returns>PowerShell commands as strings</returns>
        public static List<string> GetDiagnosticCommands()
        {
            var commands = new List<string>
            {
                "# Run these PowerShell commands to diagnose TalonList column size issues:",
                "",
                "# 1. Navigate to the DataAccessLibrary project directory",
                "cd \"c:\\Users\\MPhil\\source\\repos\\VoiceLauncherBlazor\\DataAccessLibrary\"",
                "",
                "# 2. Run the diagnostic script",
                "dotnet run --project . -- --diagnose-talon-lists",
                "",
                "# Alternative: Create a simple console app to test the data",
                "# Create a temporary test file and run it",
                "",
                "# If you need to check specific values manually, use this SQL query:",
                "# (Run this in SQL Server Management Studio or similar tool)",
                "",
                "# Check for values exceeding limits:",
                "SELECT ",
                "    Id,",
                "    ListName,",
                "    LEN(ListName) as ListNameLength,",
                "    SpokenForm,", 
                "    LEN(SpokenForm) as SpokenFormLength,",
                "    LEFT(ListValue, 100) as ListValueSample,",
                "    LEN(ListValue) as ListValueLength,",
                "    SourceFile,",
                "    LEN(SourceFile) as SourceFileLength",
                "FROM TalonLists",
                "WHERE LEN(ListName) > 100 ",
                "   OR LEN(SpokenForm) > 100",
                "   OR LEN(ListValue) > 500",
                "   OR LEN(SourceFile) > 250",
                "ORDER BY ListValueLength DESC, ListNameLength DESC, SpokenFormLength DESC, SourceFileLength DESC"
            };

            return commands;
        }

        /// <summary>
        /// Creates the Entity Framework migration script commands for copying and pasting
        /// </summary>
        /// <returns>PowerShell commands to create a migration for larger column sizes</returns>
        public static List<string> GetMigrationCommands()
        {
            var commands = new List<string>
            {
                "# Entity Framework Migration Commands for TalonList Column Size Increases:",
                "",
                "# 1. Navigate to the DataAccessLibrary project directory", 
                "cd \"c:\\Users\\MPhil\\source\\repos\\VoiceLauncherBlazor\\DataAccessLibrary\"",
                "",
                "# 2. Create a new migration for column size increases",
                "dotnet ef migrations add IncreaseTalonListColumnSizes --configuration Debug",
                "",
                "# 3. Review the generated migration file before applying",
                "# Check the file in: Migrations/[timestamp]_IncreaseTalonListColumnSizes.cs",
                "",
                "# 4. Generate the SQL script (DO NOT use Update-Database)",
                "dotnet ef migrations script --configuration Debug > TalonListColumnSizes-Migration-Script.sql",
                "",
                "# 5. Review the SQL script and apply it manually to your database",
                "# The script will be saved as: TalonListColumnSizes-Migration-Script.sql"
            };

            return commands;
        }

        /// <summary>
        /// Suggested new column sizes based on typical data patterns
        /// </summary>
        public static Dictionary<string, int> GetSuggestedColumnSizes()
        {
            return new Dictionary<string, int>
            {
                { "ListName", 200 },      // Increase from 100 to 200
                { "SpokenForm", 200 },    // Increase from 100 to 200  
                { "ListValue", 1000 },    // Increase from 500 to 1000
                { "SourceFile", 500 }     // Increase from 250 to 500
            };
        }

        /// <summary>
        /// Gets the manual SQL commands to update column sizes
        /// </summary>
        /// <returns>SQL ALTER TABLE commands</returns>
        public static List<string> GetManualSqlCommands()
        {
            var commands = new List<string>
            {
                "-- Manual SQL commands to increase TalonList column sizes:",
                "-- (Run these in SQL Server Management Studio or similar)",
                "",
                "USE [YourDatabaseName] -- Replace with your actual database name",
                "GO",
                "",
                "-- Increase ListName column size from 100 to 200",
                "ALTER TABLE [dbo].[TalonLists]",
                "ALTER COLUMN [ListName] NVARCHAR(200) NOT NULL;",
                "",
                "-- Increase SpokenForm column size from 100 to 200", 
                "ALTER TABLE [dbo].[TalonLists]",
                "ALTER COLUMN [SpokenForm] NVARCHAR(200) NOT NULL;",
                "",
                "-- Increase ListValue column size from 500 to 1000",
                "ALTER TABLE [dbo].[TalonLists]", 
                "ALTER COLUMN [ListValue] NVARCHAR(1000) NOT NULL;",
                "",
                "-- Increase SourceFile column size from 250 to 500",
                "ALTER TABLE [dbo].[TalonLists]",
                "ALTER COLUMN [SourceFile] NVARCHAR(500) NULL;",
                "",
                "-- Verify the changes",
                "SELECT ",
                "    COLUMN_NAME,",
                "    DATA_TYPE,", 
                "    CHARACTER_MAXIMUM_LENGTH,",
                "    IS_NULLABLE",
                "FROM INFORMATION_SCHEMA.COLUMNS",
                "WHERE TABLE_NAME = 'TalonLists'",
                "ORDER BY ORDINAL_POSITION;"
            };

            return commands;
        }
    }
}
