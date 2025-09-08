# Blazor WASM + IndexedDB Plan

Date: 2025-09-08

Purpose

This file documents the chosen approach for a low-cost, offline-first Blazor WebAssembly (WASM) client that stores Talon lists and Talon commands in the browser using IndexedDB. It records interfaces, storage behaviors (including a required "clear data" operation), suggested packages, migration advice, and next steps to implement the prototype in the repository.

Requirements checklist

- [x] Use IndexedDB as the local data source for the Blazor WASM client. (decided)
- [x] Reuse parsing/import logic from `RazorClassLibrary` by extracting pure parsing/validation into a shared service. (design)
- [x] Provide ability to clear all stored data on user request. (designed + required in repository interface)
- [x] Provide export/import (backup/restore) UX for cross-device transfer without server costs. (designed)
- [ ] Scaffold the Blazor WASM project and repository implementation. (next step)

High-level architecture

- Client: Blazor WebAssembly (PWA optional) running in browser.
- Storage: IndexedDB (recommended) via a small wrapper library.
- Shared code: Move parsing/validation into `RazorClassLibrary/Services` and models into `RazorClassLibrary/Models` so the WASM client can reference them.
- Persistence abstraction: `ITalonRepository` interface defined in `RazorClassLibrary` with two implementations:
  - `EfCoreTalonRepository` (server-side, existing EF Core code)
  - `IndexedDbTalonRepository` (client-side, WASM)

Data model (summary)

Use the existing models where possible. Example minimal DTOs (place in `RazorClassLibrary/Models`):

```csharp
public class TalonListModel
{
    public string Id { get; set; } // GUID or content-hash
    public string Name { get; set; }
    public List<string> Items { get; set; }
    public string Source { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class TalonCommandModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public Dictionary<string,string> Metadata { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
```

Repository abstraction (`RazorClassLibrary/Services`)

Define a small contract so both server and client can use the same import and UI code.

```csharp
public interface ITalonRepository
{
    Task SaveListAsync(TalonListModel list);
    Task<IEnumerable<TalonListModel>> GetListsAsync();
    Task SaveCommandsAsync(IEnumerable<TalonCommandModel> commands);
    Task<IEnumerable<TalonCommandModel>> GetCommandsAsync();
    Task DeleteAllAsync(); // required: clear out data on user request
    Task<string> ExportAllJsonAsync(); // optional convenience
    Task ImportFromJsonAsync(string json); // optional convenience
}
```

IndexedDB implementation notes

- IndexedDB is asynchronous and robust; preferred over `localStorage` for capacity and reliability.
- Recommended libraries (pick one;  recommend use the first one):
  - `TG.Blazor.IndexedDB` (mature, typed mapping) — good for structured stores.
  - `Blazored.LocalStorage` (simpler) — acceptable only for very small datasets; not recommended for scaling.

Suggested IndexedDB schema (logical):
- Store: `talon-lists` (key: `Id`) — JSON objects using `TalonListModel`.
- Store: `talon-commands` (key: `Id`) — JSON objects using `TalonCommandModel`.
- Optionally: `indexes` store to maintain quick lookup or list index.

Clear-data behavior

- Implement `DeleteAllAsync()` on `IndexedDbTalonRepository` to remove all entries from `talon-lists` and `talon-commands` stores.
- Expose a UI action `Clear Local Data` with an explicit confirmation modal and an undo/backup advice step.
- For safety, `Clear Local Data` should:
  1. Offer an export (download JSON) as a backup.
  2. Require typed confirmation (e.g., user types DELETE) before clearing.
  3. Call `DeleteAllAsync()` and surface success/failure to user.

Export / Import (backup & move)

- `ExportAllJsonAsync()` should read both stores and return a single JSON file containing metadata, lists, and commands.
- The UI should allow the user to download that JSON and to upload it later to `ImportFromJsonAsync(string json)`.

PWA & Storage quota

- Consider making the WASM app a PWA. Installed PWAs often get higher storage allowances on some platforms and provide a better offline UX.  yes we'll need it to be a PWA!
- Be aware that browsers may prompt when storage crosses thresholds. For most talon datasets this is not an issue, but document this for users.

Security and privacy

- Browser storage is local and not encrypted by default. Document that data is tied to the browser profile and recommend users export backups when migrating devices.
- Do not store sensitive secrets in IndexedDB. If any secret storage is needed later, layer encryption on top or require a server.

Packages / CLI commands (WASM project)

Example commands to add packages to the future WASM project (use `pwsh`):

```powershell
dotnet add <WasmProject.csproj> package TG.Blazor.IndexedDB
```

Notes on code reuse

- Parsing/validation/import logic should be pure .NET (no EF Core, no System.Data.SqlClient) and placed into `RazorClassLibrary/Services/TalonImportService`. Both the server and WASM projects can reference `RazorClassLibrary` to use the same logic.
- If the current import code depends on EF Core or other server-only libraries, refactor it so the core parsing functions are separated and accept `ITalonRepository` for persistence.

Files to add (suggested)

- `RazorClassLibrary/Models/TalonListModel.cs` (if not present)
- `RazorClassLibrary/Models/TalonCommandModel.cs`
- `RazorClassLibrary/Services/ITalonRepository.cs`
- `RazorClassLibrary/Services/TalonImportService.cs` (pure parsing + validation)
- `src/VoiceLauncher.Wasm/` (new project folder; name may vary)
  - `Program.cs`, `App.razor`, `wwwroot/` etc.
  - `Services/IndexedDbTalonRepository.cs` (implements `ITalonRepository`)
  - `Pages/Import.razor` (file picker + import UI)
  - `Pages/Settings.razor` (Clear Local Data + Export/Import)

Minimal sample: DeleteAllAsync() for IndexedDB (conceptual)

```csharp
public async Task DeleteAllAsync()
{
    // using TG.Blazor.IndexedDB style pseudo-code
    await _indexedDbManager.ClearStoreAsync("talon-lists");
    await _indexedDbManager.ClearStoreAsync("talon-commands");
}
```

UX recommendations

- Provide a single-page Import experience with drag/drop or file picker.
- Show validation errors from `TalonImportService` before persisting.
- Add a small Settings page that contains:
  - Export backup (Download JSON)
  - Import backup (Upload JSON)
  - Clear Local Data (with confirmation)
- Provide an option to switch between dark and light mode.

Next steps (what I will do next if you want a prototype)

1. Create `ITalonRepository` and `TalonListModel` / `TalonCommandModel` in `RazorClassLibrary` (pure .NET). 
2. Scaffold a minimal Blazor WASM project in the solution named `VoiceLauncher.Wasm` and add a page `Import.razor` and `Settings.razor`.
3. Add `TG.Blazor.IndexedDB` to the WASM project and implement `IndexedDbTalonRepository` with `DeleteAllAsync`, `ExportAllJsonAsync`, and `ImportFromJsonAsync`.
4. Wire up DI in `Program.cs` to register `ITalonRepository` to the IndexedDB implementation.
5. Add a README with testing instructions (how to run locally and how to export/import/clear).

If you'd like me to scaffold the prototype now, say so and I will create the repository interface and initial WASM project files (I will implement the `DeleteAllAsync()` behavior and the Settings UI as part of that first iteration).

Requirements coverage (quick)

- IndexedDB as data source: Done (decision in this document)
- Ability to clear data: Done (interface + UI behavior documented)
- Reuse of shared import logic: Done (plan to move to `RazorClassLibrary`)
- Prototype scaffolding: Deferred (next steps available)



