# Club Member Name Learning Feature Plan

## Goal
Create a feature to help users remember the names of club members by displaying their pictures and first names.

## Architecture
- **Project**: `VoiceLauncher` (Blazor Server)
- **Data Access**: `DataAccessLibrary` (Entity Framework Core)
- **Database**: SQL Server

## Database Schema
New table: `ClubMembers`

| Column | Type | Description |
|--------|------|-------------|
| `Id` | `int` | Primary Key (Identity) |
| `FirstName` | `nvarchar(100)` | The member's first name |
| `LastName` | `nvarchar(100)` | The member's last name (optional, for completeness) |
| `ImageData` | `varbinary(MAX)` | The member's picture stored directly in the database |
| `ContentType` | `nvarchar(50)` | MIME type of the image (e.g., 'image/jpeg') |
| `CreatedAt` | `datetime2` | Record creation timestamp |

*Note: Storing images in the database ensures all data is self-contained and simplifies backup/migration, fulfilling the requirement to "store information in the database".*

## Implementation Steps

### 1. Data Access Layer (`DataAccessLibrary`)
- Create `ClubMember` model class.
- Add `DbSet<ClubMember> ClubMembers` to `ApplicationDbContext`.
- Create EF Core migration to generate the table.
- Create `IClubMemberService` and `ClubMemberService` to handle CRUD operations.

### 2. UI Layer (`VoiceLauncher`)
- **Page**: `/club-members`
- **Features**:
    - **List View**: Display cards with member photos and names.
    - **Add/Edit Member**: Form to input name and upload an image file.
    - **Learning Mode** : A mode to hide names until clicked, to test memory.

### 3. Blazor Server Specifics
- Use `InputFile` component for image uploads.
- Convert uploaded stream to `byte[]` for storage.
- Render images using `data:image/png;base64,...` string.

## Computer Name / Environment
- Since this is a Blazor Server app, we can access `Environment.MachineName`  this option should only be shown in the menu and be allowed to be opened if the computer name is correct for example: J40L4V3
- *Assumption*: The list is global for the database.

## Next Steps
1.  Create the `ClubMember` model.
2.  Generate the Entity Framework Core migration script.
3.  Implement the Service.
4.  Build the UI.
