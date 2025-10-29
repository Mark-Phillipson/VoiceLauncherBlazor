# VS Code Extension for Talon Command Search - Development Plan

## Overview
Port existing Blazor Server + .NET solution to a VS Code extension for searching ~10,000 Talon commands and ~800 lists. The current solution uses:
- **TalonVoiceCommandsServer**: Blazor Server app with IndexedDB/localStorage for browser-based persistence
- **DataAccessLibrary**: Entity Framework Core with SQL Server backend (legacy approach)

## Existing Architecture Analysis

### Current Implementation (TalonVoiceCommandsServer)
**Data Models:**
- `TalonVoiceCommand`: Command(200), Script(2000), Application(200), Mode(300), OS(100), FilePath(500), Repository(200), Tags(500), Title(200), CodeLanguage(100), Language(50), Hostname(100)
- `TalonList`: ListName(100), SpokenForm(100), ListValue(700), SourceFile(250)

**Key Features Already Implemented:**
- ✅ .talon file parsing with header detection (app, mode, os, tags, title, code.language, language, hostname)
- ✅ Repository extraction from file paths (e.g., community, knausj_talon)
- ✅ Multi-line script support
- ✅ Headerless file handling (defaults to global)
- ✅ IndexedDB storage for large datasets (10k+ commands) with chunked loading
- ✅ LocalStorage fallback for smaller datasets
- ✅ List expansion in scripts ({list_name} and function call patterns)
- ✅ Enhanced search with list item matching (e.g., search "angry" finds commands using emoji lists)
- ✅ Multiple search scopes: CommandNamesOnly (exact match), ScriptOnly, All
- ✅ Filter by: Application, Mode, OS, Repository, Tags, Title, CodeLanguage
- ✅ JavaScript-based filter extraction (avoids loading all data into C# memory)
- ✅ Progress reporting during bulk imports
- ✅ Command breakdown/analytics
- ✅ Script expansion showing list values inline

**Search Capabilities:**
- Text search across commands, scripts, applications, titles, tags
- Semantic matching with LocalEmbedder (SmartComponents.LocalEmbeddings)
- List-aware search (finds commands referencing lists containing search term)
- Fuzzy matching fallbacks
- Result limiting to prevent SignalR timeouts (default 500 max)

### Legacy Implementation (DataAccessLibrary)
- Uses Entity Framework Core + SQL Server
- Similar parsing logic but database-backed
- Less suitable for VS Code extension (requires SQL Server installation)

## Architecture Decisions

### Database Strategy
**Recommended: IndexedDB via Browser APIs (Port from TalonVoiceCommandsServer)**
- Already proven to work with 10k+ commands in the Blazor implementation
- VS Code extensions can use embedded webviews with IndexedDB
- No external dependencies
- Chunked loading prevents memory issues
- Fast client-side filtering

**Alternative Options:**
1. **SQLite with better-sqlite3**: Traditional approach, well-documented
2. **Keep SQL Server**: Requires backend API, complex deployment
3. **JSON files**: Simple but slow for 10k+ records

**Decision: Use IndexedDB approach** - Port the existing JavaScript storage layer from TalonVoiceCommandsServer's wwwroot/js files

### Storage Location
Use `context.globalStorageUri` for IndexedDB path, ensuring workspace-independent persistence.

## Project Structure

```
talon-command-search-extension/
├── src/
│   ├── extension.ts              # Entry point, command registration
│   ├── storage/
│   │   ├── indexedDBManager.ts   # Port from TalonStorageDB.js
│   │   └── dataModels.ts         # TypeScript interfaces
│   ├── parser/
│   │   ├── talonFileParser.ts    # Port from TalonVoiceCommandDataService
│   │   ├── talonListParser.ts    # List file parsing
│   │   └── headerParser.ts       # Header extraction logic
│   ├── search/
│   │   ├── searchEngine.ts       # Port search logic
│   │   ├── filterManager.ts      # Multi-filter support
│   │   └── listMatcher.ts        # List expansion & matching
│   ├── ui/
│   │   ├── searchPanel.ts        # Quick Pick or Webview
│   │   ├── resultsView.ts        # TreeView for results
│   │   └── listExplorer.ts       # Browse lists
│   ├── scanner/
│   │   └── fileWatcher.ts        # Watch .talon files for changes
│   └── utils/
│       ├── pathUtils.ts          # Repository extraction, normalization
│       └── textUtils.ts          # String truncation, escaping
├── webview/                       # Optional: Advanced UI
│   ├── index.html
│   ├── search.js                 # Port from TalonVoiceCommandSearch.razor.js
│   └── styles.css
├── package.json                   # Extension manifest
├── tsconfig.json
└── README.md
```

## Data Models (Port from Existing)

```typescript
// filepath: src/storage/dataModels.ts

export interface TalonVoiceCommand {
    id: number;
    command: string;              // max 200 chars
    script: string;               // max 2000 chars
    application: string;          // max 200 chars, default "global"
    title?: string;               // max 200 chars
    mode?: string;                // max 300 chars (comma-separated)
    operatingSystem?: string;     // max 100 chars
    filePath: string;             // max 500 chars
    repository?: string;          // max 200 chars (e.g., "community", "knausj_talon")
    tags?: string;                // max 500 chars (comma-separated)
    codeLanguage?: string;        // max 100 chars
    language?: string;            // max 50 chars
    hostname?: string;            // max 100 chars
    createdAt: string;            // ISO datetime
}

export interface TalonList {
    id: number;
    listName: string;             // max 100 chars (e.g., "user.emoji")
    spokenForm: string;           // max 100 chars (e.g., "angry")
    listValue: string;            // max 700 chars (e.g., "😠")
    sourceFile?: string;          // max 250 chars
    createdAt: string;
    importedAt: string;
}

export interface SearchOptions {
    searchTerm?: string;
    application?: string;
    mode?: string;
    operatingSystem?: string;
    repository?: string;
    tags?: string;
    title?: string;
    codeLanguage?: string;
    searchScope: SearchScope;     // CommandNamesOnly | ScriptOnly | All
    maxResults?: number;          // default 500
}

export enum SearchScope {
    CommandNamesOnly = 0,  // Exact equality, case-insensitive
    ScriptOnly = 1,        // Search only in script content
    All = 2                // Search command, script, title, tags + list matching
}

export interface FilterValues {
    applications: string[];
    modes: string[];
    operatingSystems: string[];
    repositories: string[];
    tags: string[];
    titles: string[];
    codeLanguages: string[];
}
```

## Implementation Phases

### Phase 1: Basic Extension Setup (Week 1)
- [ ] Initialize VS Code extension project
- [ ] Set up TypeScript configuration
- [ ] Install dependencies (none needed for IndexedDB approach)
- [ ] Create basic command registration
- [ ] Set up webview infrastructure for IndexedDB access
- [ ] Port predefined filter values from TalonVoiceCommandSearch.razor.cs

### Phase 2: IndexedDB Storage Layer (Week 1-2)
- [ ] Port TalonStorageDB.js initialization logic
- [ ] Implement chunked save/load for commands (1000 per chunk)
- [ ] Implement list storage operations
- [ ] Port filter extraction JavaScript functions (getFilterValues, getDataStatistics)
- [ ] Add migration support from localStorage to IndexedDB
- [ ] Test with 10k+ command dataset

### Phase 3: Talon File Parser (Week 2)
- [ ] Port ParseApplicationFromHeaderLine logic
- [ ] Port header parsing (app, mode, os, tags, title, code.language, language, hostname)
- [ ] Port repository extraction (ExtractRepositoryFromPath)
- [ ] Handle headerless files (default to global)
- [ ] Port multi-line script parsing
- [ ] Validate field length truncation logic (Command: 200, Script: 2000, etc.)
- [ ] Test with various .talon file formats

### Phase 4: List Parsing & Management (Week 2)
- [ ] Parse .talon-list files (user.list_name format)
- [ ] Port TalonLists.txt import functionality
- [ ] Implement list expansion in scripts (ExpandListsInScriptAsync)
- [ ] Handle {list_name} and <list_name> references
- [ ] Support function call patterns: key(arrow_key), insert(text)
- [ ] Test list value display (show first 5 items with "... and X more")

### Phase 5: Search Engine (Week 2-3)
- [ ] Port SearchFilteredCommandsLimitedAsync (with maxResults limiting)
- [ ] Implement CommandNamesOnly search (exact equality, case-insensitive)
- [ ] Implement ScriptOnly search
- [ ] Port ApplyEnhancedSearchWithLists (list-aware search)
- [ ] Handle list reference patterns: {user.emoji}, <user.emoji>, {emoji}
- [ ] Port JavaScript-based filtering (searchFilteredCommands, searchFilteredCommandIds)
- [ ] Implement result truncation warnings
- [ ] Add search scope enum support

### Phase 6: Filter Management (Week 3)
- [ ] Port GetFilterValuesFromJavaScriptAsync pattern
- [ ] Implement multi-filter UI (dropdowns or Quick Pick)
- [ ] Port predefined filter lists (60+ applications, modes, etc.)
- [ ] Implement filter caching/invalidation
- [ ] Add "clear all filters" functionality
- [ ] Port auto-filter by current application logic
- [ ] Test filter population from IndexedDB without loading all commands

### Phase 7: File Watching & Updates (Week 3)
- [ ] Set up file watcher for Talon user folder
- [ ] Implement incremental index updates
- [ ] Handle file additions/deletions/modifications
- [ ] Port ImportAllTalonFilesWithProgressAsync with progress reporting
- [ ] Add manual refresh command
- [ ] Test with live .talon file changes

### Phase 8: UI & Results Display (Week 3-4)
- [ ] Create search panel (Quick Pick or custom webview)
- [ ] Port ScriptCard display logic (truncated vs full view)
- [ ] Implement list panel/explorer for browsing all lists
- [ ] Add "focus mode" for single command detail view
- [ ] Port script expansion display with list values
- [ ] Show repository, tags, mode, OS in results
- [ ] Add copy-to-clipboard functionality
- [ ] Implement "jump to file" for FilePath navigation

### Phase 9: Analytics & Diagnostics (Week 4)
- [ ] Port GetTalonCommandsBreakdownAsync
- [ ] Show statistics: total commands, total lists, repository counts
- [ ] Port FindOversizedTalonListValues diagnostics
- [ ] Display import progress and errors
- [ ] Add data validation warnings

### Phase 10: Testing & Polish (Week 4-5)
- [ ] Unit tests for file parsing
- [ ] Integration tests for IndexedDB operations
- [ ] Performance testing with 10k+ commands
- [ ] Test SignalR timeout scenarios (ensure <30 second responses)
- [ ] Cross-platform testing (Windows/Mac/Linux)
- [ ] Documentation and README
- [ ] Package for VS Code marketplace

## Key Features to Port from TalonVoiceCommandsServer

### From TalonVoiceCommandDataService.cs
**File Parsing:**
- `ImportTalonFileContentAsync`: Complete .talon file parsing with header detection
- `ExtractRepositoryFromPath`: Extract repository name from file path
- `ParseApplicationFromHeaderLine`: Parse app: lines with various formats
- Header detection logic for delimiter (`-`)
- Multi-line script accumulation
- Field length truncation to database limits

**Search Operations:**
- `SearchFilteredCommandsLimitedAsync`: Limited results with total count tracking
- `SearchCommandNamesOnlyAsync`: Exact command name matching
- `SearchScriptOnlyAsync`: Script-only text search
- `SemanticSearchWithListsAsync`: Enhanced search with list item matching
- `ApplyEnhancedSearchWithLists`: Finds commands using lists containing search term
- `GetFilteredCommandsInMemory`: Multi-filter application
- Result limiting to prevent timeout (default 500 max)

**List Operations:**
- `ImportTalonListsFromFileAsync`: Parse TalonLists.txt format
- `ExpandListsInScriptAsync`: Expand {list} and <list> references
- `GetExpandedListString`: Show first 5 items with "... and X more"
- `GetListContentsAsync`: Retrieve all items for a list
- List reference patterns: {user.emoji}, <user.emoji>, {emoji}, <emoji>
- Function call pattern detection: key(arrow_key), insert(text)

**Filter Management:**
- `GetFilterValuesFromJavaScriptAsync`: Extract unique filter values from IndexedDB
- `GetDataStatisticsFromJavaScriptAsync`: Get count without loading all data
- Predefined filter values (60+ applications, modes, etc.)

**Storage Operations:**
- `EnsureLoadedFromIndexedDBAsync`: Lazy load with chunked reading
- `SaveToIndexedDBAsync`: Chunked saving for large datasets
- `MigrateToIndexedDBAsync`: Migrate from localStorage
- Quota detection and error handling

**Diagnostics:**
- `FindOversizedTalonListValues`: Validate field lengths before save
- `GetTalonListMaxLengths`: Analyze max field sizes in dataset
- `GetTalonCommandsBreakdownAsync`: Repository-based analytics

### From TalonVoiceCommandSearch.razor.cs
**UI Components:**
- Multi-filter dropdown system (Application, Mode, OS, Repository, Tags, Title, CodeLanguage)
- Search scope selector (CommandNamesOnly, ScriptOnly, All)
- Result card display (truncated/expanded views)
- List panel/explorer
- Focus mode for single command detail
- Auto-rotation through results
- Import progress tracking

**Filter Logic:**
- Auto-filter by current application
- Filter collapse/expand state management
- Predefined filter lists with 60+ applications
- Cache invalidation after imports
- Modal-based filter selection

**Search Features:**
- Debounced search input
- Result count display (truncation warnings)
- JavaScript-based result display (avoid C# memory load)
- Copy command/script to clipboard
- Jump to file path

### From wwwroot/js (TalonStorageDB.js)
**IndexedDB Operations:**
- Database initialization with object stores
- Chunked command loading (1000 per chunk)
- Filter value extraction in JavaScript
- Search operations client-side
- Migration from localStorage detection
- Quota/storage error handling

### From DataAccessLibrary (Legacy - Lower Priority)
- Entity Framework Core migrations (if SQL backend needed)
- SQL Server connection management
- SmartComponents.LocalEmbeddings semantic search (optional enhancement)

## Critical Implementation Details

### Header Parsing
```csharp
// Port this exact logic from TalonVoiceCommandDataService.cs
private string? ParseApplicationFromHeaderLine(string line)
{
    // Handles: "app: chrome", "app.name: visual studio code", "app.exe: code.exe"
    // See lines ~50-80 in TalonVoiceCommandDataService.cs
}
```

### Repository Extraction
```csharp
// Port from TalonVoiceCommandDataService.cs
private string? ExtractRepositoryFromPath(string filePath)
{
    // Finds first subdirectory after '/user/' in path
    // Example: C:\Users\...\talon\user\community\file.talon -> "community"
}
```

### List-Aware Search
```csharp
// Port from TalonVoiceCommandDataService.cs lines ~800-900
private IEnumerable<TalonVoiceCommand> ApplyEnhancedSearchWithLists(...)
{
    // 1. Find direct text matches
    // 2. Split search term into words
    // 3. Find lists containing those words
    // 4. Find commands referencing those lists ({list}, <list>)
    // 5. Combine and deduplicate results
}
```

### IndexedDB Chunked Loading
```javascript
// Port from TalonStorageDB.js
async function loadCommandsChunked(chunkSize = 1000) {
    // Load commands in chunks to avoid memory issues
    // Return combined array
}
```

### Field Length Limits (CRITICAL)
```typescript
// Enforce these exact limits from Models
const LIMITS = {
    command: 200,
    script: 2000,
    application: 200,
    title: 200,
    mode: 300,
    operatingSystem: 100,
    filePath: 500,
    repository: 200,
    tags: 500,
    codeLanguage: 100,
    language: 50,
    hostname: 100,
    listName: 100,
    spokenForm: 100,
    listValue: 700,
    sourceFile: 250
};
```

## Configuration (package.json)

```json
{
    "name": "talon-command-search",
    "displayName": "Talon Command Search",
    "description": "Search and browse Talon Voice commands and lists with advanced filtering",
    "version": "0.1.0",
    "engines": {
        "vscode": "^1.85.0"
    },
    "categories": ["Other"],
    "activationEvents": ["onStartupFinished"],
    "main": "./out/extension.js",
    "contributes": {
        "commands": [
            {
                "command": "talonSearch.searchCommands",
                "title": "Talon: Search Commands"
            },
            {
                "command": "talonSearch.searchCommandsExact",
                "title": "Talon: Search Command Names (Exact Match)"
            },
            {
                "command": "talonSearch.searchScript",
                "title": "Talon: Search in Scripts"
            },
            {
                "command": "talonSearch.searchLists",
                "title": "Talon: Browse Lists"
            },
            {
                "command": "talonSearch.refreshIndex",
                "title": "Talon: Refresh Command Index"
            },
            {
                "command": "talonSearch.importFromDirectory",
                "title": "Talon: Import from Directory"
            },
            {
                "command": "talonSearch.showAnalytics",
                "title": "Talon: Show Analytics"
            },
            {
                "command": "talonSearch.clearFilters",
                "title": "Talon: Clear All Filters"
            },
            {
                "command": "talonSearch.openSettings",
                "title": "Talon: Open Search Settings"
            }
        ],
        "views": {
            "explorer": [
                {
                    "id": "talonCommandResults",
                    "name": "Talon Search Results"
                },
                {
                    "id": "talonListExplorer",
                    "name": "Talon Lists"
                }
            ]
        },
        "configuration": {
            "title": "Talon Command Search",
            "properties": {
                "talonSearch.userFolderPath": {
                    "type": "string",
                    "default": "",
                    "description": "Path to Talon user folder (leave empty for auto-detection)"
                },
                "talonSearch.enableAutoIndexing": {
                    "type": "boolean",
                    "default": true,
                    "description": "Automatically index Talon files on startup and file changes"
                },
                "talonSearch.maxSearchResults": {
                    "type": "number",
                    "default": 500,
                    "description": "Maximum number of search results to display (prevents timeouts)"
                },
                "talonSearch.showFullCards": {
                    "type": "boolean",
                    "default": false,
                    "description": "Show full command cards by default (vs collapsed)"
                },
                "talonSearch.autoFilterByApp": {
                    "type": "boolean",
                    "default": false,
                    "description": "Automatically filter by current application"
                },
                "talonSearch.defaultSearchScope": {
                    "type": "string",
                    "enum": ["CommandNamesOnly", "ScriptOnly", "All"],
                    "default": "CommandNamesOnly",
                    "description": "Default search scope"
                }
            }
        },
        "keybindings": [
            {
                "command": "talonSearch.searchCommands",
                "key": "ctrl+shift+t",
                "mac": "cmd+shift+t"
            },
            {
                "command": "talonSearch.searchCommandsExact",
                "key": "ctrl+shift+alt+t",
                "mac": "cmd+shift+alt+t"
            }
        ]
    }
}
```

## Dependencies (package.json)

```json
{
    "dependencies": {
        "@vscode/webview-ui-toolkit": "^1.4.0"
    },
    "devDependencies": {
        "@types/node": "^20.0.0",
        "@types/vscode": "^1.85.0",
        "typescript": "^5.3.0",
        "@vscode/test-electron": "^2.3.8",
        "esbuild": "^0.19.0"
    }
}
```

**Note:** No external database libraries needed - using browser IndexedDB via webview

## Initial Extension Code Template

```typescript
// filepath: src/extension.ts
import * as vscode from 'vscode';
import * as path from 'path';
import * as fs from 'fs';

let storagePanel: vscode.WebviewPanel | undefined;

export async function activate(context: vscode.ExtensionContext) {
    console.log('Talon Command Search extension is now active');

    // Initialize storage (IndexedDB via webview)
    const storageUri = context.globalStorageUri;
    
    // Register commands
    context.subscriptions.push(
        vscode.commands.registerCommand('talonSearch.searchCommands', async () => {
            await showSearchPanel(context, 'All');
        })
    );

    context.subscriptions.push(
        vscode.commands.registerCommand('talonSearch.searchCommandsExact', async () => {
            await showSearchPanel(context, 'CommandNamesOnly');
        })
    );

    context.subscriptions.push(
        vscode.commands.registerCommand('talonSearch.searchScript', async () => {
            await showSearchPanel(context, 'ScriptOnly');
        })
    );

    context.subscriptions.push(
        vscode.commands.registerCommand('talonSearch.refreshIndex', async () => {
            const config = vscode.workspace.getConfiguration('talonSearch');
            let talonPath = config.get<string>('userFolderPath');
            
            if (!talonPath) {
                talonPath = await autoDetectTalonPath();
            }

            if (talonPath) {
                await importTalonFiles(context, talonPath);
                vscode.window.showInformationMessage('Talon command index refreshed');
            } else {
                vscode.window.showErrorMessage('Talon user folder not found. Please configure talonSearch.userFolderPath');
            }
        })
    );

    context.subscriptions.push(
        vscode.commands.registerCommand('talonSearch.importFromDirectory', async () => {
            const folderUri = await vscode.window.showOpenDialog({
                canSelectFolders: true,
                canSelectFiles: false,
                canSelectMany: false,
                title: 'Select Talon User Folder'
            });

            if (folderUri && folderUri[0]) {
                await importTalonFiles(context, folderUri[0].fsPath);
            }
        })
    );

    // Auto-import on startup if enabled
    const config = vscode.workspace.getConfiguration('talonSearch');
    if (config.get<boolean>('enableAutoIndexing')) {
        let talonPath = config.get<string>('userFolderPath');
        if (!talonPath) {
            talonPath = await autoDetectTalonPath();
        }
        if (talonPath) {
            await importTalonFiles(context, talonPath);
        }
    }
}

async function showSearchPanel(context: vscode.ExtensionContext, searchScope: string) {
    if (!storagePanel) {
        storagePanel = vscode.window.createWebviewPanel(
            'talonSearch',
            'Talon Command Search',
            vscode.ViewColumn.One,
            {
                enableScripts: true,
                retainContextWhenHidden: true,
                localResourceRoots: [
                    vscode.Uri.joinPath(context.extensionUri, 'webview')
                ]
            }
        );

        storagePanel.onDidDispose(() => {
            storagePanel = undefined;
        }, null, context.subscriptions);

        // Load webview HTML
        const webviewPath = vscode.Uri.joinPath(context.extensionUri, 'webview', 'index.html');
        const html = fs.readFileSync(webviewPath.fsPath, 'utf8');
        
        // Replace resource URIs
        const scriptUri = storagePanel.webview.asWebviewUri(
            vscode.Uri.joinPath(context.extensionUri, 'webview', 'search.js')
        );
        const styleUri = storagePanel.webview.asWebviewUri(
            vscode.Uri.joinPath(context.extensionUri, 'webview', 'styles.css')
        );

        storagePanel.webview.html = html
            .replace('{{scriptUri}}', scriptUri.toString())
            .replace('{{styleUri}}', styleUri.toString());

        // Handle messages from webview
        storagePanel.webview.onDidReceiveMessage(
            async message => {
                switch (message.command) {
                    case 'openFile':
                        await openTalonFile(message.filePath);
                        break;
                    case 'copyText':
                        await vscode.env.clipboard.writeText(message.text);
                        vscode.window.showInformationMessage('Copied to clipboard');
                        break;
                }
            },
            undefined,
            context.subscriptions
        );
    }

    storagePanel.reveal();
    
    // Send search scope to webview
    storagePanel.webview.postMessage({ 
        command: 'setSearchScope', 
        scope: searchScope 
    });
}

async function importTalonFiles(context: vscode.ExtensionContext, rootFolder: string) {
    // Show progress
    await vscode.window.withProgress({
        location: vscode.ProgressLocation.Notification,
        title: "Importing Talon files",
        cancellable: false
    }, async (progress) => {
        progress.report({ increment: 0, message: "Scanning files..." });

        // Get all .talon files
        const files = await getTalonFiles(rootFolder);
        
        progress.report({ increment: 10, message: `Found ${files.length} files` });

        // Send to webview for parsing and storage
        if (storagePanel) {
            storagePanel.webview.postMessage({
                command: 'importFiles',
                files: files
            });
        }

        progress.report({ increment: 100, message: "Import complete" });
    });
}

async function getTalonFiles(rootFolder: string): Promise<{path: string, content: string}[]> {
    const files: {path: string, content: string}[] = [];
    
    async function scanDirectory(dir: string) {
        const entries = fs.readdirSync(dir, { withFileTypes: true });
        
        for (const entry of entries) {
            const fullPath = path.join(dir, entry.name);
            
            if (entry.isDirectory()) {
                await scanDirectory(fullPath);
            } else if (entry.name.endsWith('.talon')) {
                const content = fs.readFileSync(fullPath, 'utf8');
                files.push({ path: fullPath, content });
            }
        }
    }

    await scanDirectory(rootFolder);
    return files;
}

async function openTalonFile(filePath: string) {
    try {
        const uri = vscode.Uri.file(filePath);
        const doc = await vscode.workspace.openTextDocument(uri);
        await vscode.window.showTextDocument(doc);
    } catch (error) {
        vscode.window.showErrorMessage(`Could not open file: ${filePath}`);
    }
}

async function autoDetectTalonPath(): Promise<string | undefined> {
    const isWindows = process.platform === 'win32';
    const isMac = process.platform === 'darwin';
    
    const possiblePaths = isWindows ? [
        path.join(process.env.APPDATA || '', 'talon', 'user'),
        path.join(process.env.USERPROFILE || '', 'AppData', 'Roaming', 'talon', 'user')
    ] : isMac ? [
        path.join(process.env.HOME || '', '.talon', 'user')
    ] : [
        path.join(process.env.HOME || '', '.talon', 'user')
    ];

    for (const testPath of possiblePaths) {
        if (fs.existsSync(testPath)) {
            return testPath;
        }
    }

    return undefined;
}

export function deactivate() {
    if (storagePanel) {
        storagePanel.dispose();
    }
}
```

## Migration Strategy from Existing Solutions

### Primary Source: TalonVoiceCommandsServer (Blazor)
**Advantages:**
- Already uses IndexedDB/localStorage (browser-based persistence)
- Proven to handle 10k+ commands without SQL Server
- Complete search and filter logic
- List expansion and matching
- Field length validation

**Migration Steps:**
1. Extract `wwwroot/js/TalonStorageDB.js` → Port to webview JavaScript
2. Port `TalonVoiceCommandDataService.cs` parsing logic → TypeScript
3. Extract filter management from `TalonVoiceCommandSearch.razor.cs` → Extension UI
4. Port predefined filter lists (applications, modes, etc.)
5. Adapt webview messaging for VS Code extension communication

### Secondary Source: DataAccessLibrary (EF Core + SQL Server)
**Use Only If:**
- Team scenario requiring centralized command database
- REST API approach desired
- Semantic search with SmartComponents.LocalEmbeddings needed

**Not Recommended Because:**
- Requires SQL Server installation
- More complex deployment
- Overkill for single-user VS Code extension

### Recommended Approach: Pure IndexedDB Port
1. Start with TalonVoiceCommandsServer as primary reference
2. Port IndexedDB operations directly (no SQLite translation)
3. Use webview for IndexedDB access (VS Code extensions can't use IndexedDB directly in Node.js)
4. Reuse exact parsing logic from C# (translate to TypeScript)
5. Maintain same data models and field limits

## Testing Checklist

- [ ] File parsing with various .talon formats (header/headerless)
- [ ] Repository extraction from paths
- [ ] Multi-line script handling
- [ ] Field length truncation validation
- [ ] IndexedDB operations (CRUD)
- [ ] Chunked loading (1000+ commands)
- [ ] Filter extraction without loading all data
- [ ] Search with CommandNamesOnly (exact match)
- [ ] Search with ScriptOnly
- [ ] Search with All (list-aware)
- [ ] List expansion in scripts
- [ ] List item matching (e.g., "angry" finds emoji commands)
- [ ] Result limiting (500 max)
- [ ] Progress reporting during import
- [ ] File watcher responsiveness
- [ ] Cross-platform compatibility (Windows/Mac/Linux)
- [ ] Extension activation time
- [ ] Memory usage with large index (10k+ commands)
- [ ] Timeout prevention (30 second SignalR limit equivalent)

## Performance Targets

- **Activation**: < 2 seconds
- **Import 10k commands**: < 30 seconds with progress
- **Search response**: < 1 second for filtered results
- **Filter extraction**: < 5 seconds from IndexedDB
- **Memory usage**: < 100MB for 10k commands
- **IndexedDB size**: ~5-10MB for 10k commands (JSON storage)

## Source File Reference Map

| Feature | Source File | Port Destination |
|---------|-------------|------------------|
| File parsing | `TalonVoiceCommandDataService.cs` (lines 1-500) | `src/parser/talonFileParser.ts` |
| Header parsing | `TalonVoiceCommandDataService.cs` (lines 50-80) | `src/parser/headerParser.ts` |
| Repository extraction | `TalonVoiceCommandDataService.cs` (lines 30-50) | `src/utils/pathUtils.ts` |
| IndexedDB operations | `wwwroot/js/TalonStorageDB.js` | `webview/storage.js` |
| Search logic | `TalonVoiceCommandDataService.cs` (lines 600-1000) | `src/search/searchEngine.ts` |
| List expansion | `TalonVoiceCommandDataService.cs` (lines 1200-1400) | `src/search/listMatcher.ts` |
| Filter extraction | `TalonVoiceCommandDataService.cs` (lines 550-650) | `webview/filters.js` |
| UI components | `TalonVoiceCommandSearch.razor.cs` | `webview/index.html` + `webview/search.js` |
| Predefined filters | `TalonVoiceCommandSearch.razor.cs` (lines 500-700) | `src/utils/predefinedFilters.ts` |
| List import | `TalonVoiceCommandDataService.cs` (lines 1500-1700) | `src/parser/talonListParser.ts` |
| Analytics | `TalonVoiceCommandDataService.cs` (GetTalonCommandsBreakdownAsync) | `src/analytics/commandBreakdown.ts` |

## Next Steps

1. **Set up basic extension project structure**
   - Initialize with `yo code` (VS Code Extension Generator)
   - Configure TypeScript
   - Set up webview infrastructure

2. **Port IndexedDB storage layer**
   - Copy TalonStorageDB.js logic
   - Test with sample data (100 commands)
   - Verify chunked loading works

3. **Port file parser**
   - Translate C# parsing logic to TypeScript
   - Test with various .talon file formats
   - Validate field length truncation

4. **Implement basic search**
   - Port SearchFilteredCommandsLimitedAsync
   - Add CommandNamesOnly exact matching
   - Test with 1000+ commands

5. **Add filter management**
   - Port GetFilterValuesFromJavaScriptAsync pattern
   - Implement dropdown/QuickPick UI
   - Load predefined filter lists

6. **Iterate and refine**
   - Add list parsing
   - Implement list-aware search
   - Add file watching
   - Polish UI and documentation