# Workspaces Migration - SQL Scripts

## Overview

This migration creates the Workspaces and WorkspaceMembers tables for the NEXUS Knowledge Management System. A Workspace is a team's collaborative space containing collections and content.

## Files

1. **002_Create_Workspaces_Tables.sql** - Main migration script
2. **002_Rollback_Workspaces_Tables.sql** - Rollback script (if needed)

## Database Schema

### Tables Created

#### Workspaces
- `Id` (UNIQUEIDENTIFIER, PK)
- `Name` (NVARCHAR(200), NOT NULL)
- `Description` (NVARCHAR(1000), NULL)
- `TeamId` (UNIQUEIDENTIFIER, NOT NULL, FK to Teams)
- `CreatedBy` (UNIQUEIDENTIFIER, NOT NULL, FK to Users)
- `CreatedAt` (DATETIME2(7), NOT NULL)
- `UpdatedAt` (DATETIME2(7), NOT NULL)
- `IsDeleted` (BIT, NOT NULL, DEFAULT 0)
- `DeletedAt` (DATETIME2(7), NULL)
- `DeletedBy` (UNIQUEIDENTIFIER, NULL, FK to Users)

#### WorkspaceMembers
- `Id` (UNIQUEIDENTIFIER, PK)
- `WorkspaceId` (UNIQUEIDENTIFIER, NOT NULL, FK to Workspaces)
- `UserId` (UNIQUEIDENTIFIER, NOT NULL, FK to Users)
- `Role` (INT, NOT NULL, DEFAULT 1)
  - 1 = Viewer (read-only access)
  - 2 = Editor (can edit content)
  - 3 = Admin (can manage members)
  - 4 = Owner (full control)
- `AddedBy` (UNIQUEIDENTIFIER, NULL, FK to Users)
- `AddedAt` (DATETIME2(7), NOT NULL)
- `IsActive` (BIT, NOT NULL, DEFAULT 1)
- `RemovedAt` (DATETIME2(7), NULL)

### Indexes

**Workspaces:**
- IX_Workspaces_TeamId (covering: Name, IsDeleted; filtered where IsDeleted = 0)
- IX_Workspaces_CreatedBy (filtered where IsDeleted = 0)
- IX_Workspaces_Name (filtered where IsDeleted = 0)
- IX_Workspaces_CreatedAt
- IX_Workspaces_IsDeleted

**WorkspaceMembers:**
- IX_WorkspaceMembers_WorkspaceId (covering: UserId, Role, IsActive)
- IX_WorkspaceMembers_UserId (covering: WorkspaceId, Role, IsActive)
- IX_WorkspaceMembers_WorkspaceId_IsActive (filtered where IsActive = 1)
- IX_WorkspaceMembers_Role (filtered where IsActive = 1)

### Foreign Keys

- FK_WorkspaceMembers_Workspaces_WorkspaceId (CASCADE DELETE)
- FK_Workspaces_Teams_TeamId
- FK_Workspaces_Users_CreatedBy
- FK_Workspaces_Users_DeletedBy
- FK_WorkspaceMembers_Users_UserId
- FK_WorkspaceMembers_Users_AddedBy

### Constraints

- UQ_WorkspaceMembers_WorkspaceUser (unique workspace-user combination)
- CK_WorkspaceMembers_Role (role must be 1-4)
- CK_Workspaces_DeletedAt (soft delete consistency)
- CK_WorkspaceMembers_RemovedAt (active/removed consistency)

## Prerequisites

⚠️ **IMPORTANT**: This migration requires the **Teams** tables to exist first!

Run the Teams migration before this one:
1. First: `001_Create_Teams_Tables.sql`
2. Then: `002_Create_Workspaces_Tables.sql`

If Teams tables don't exist, the script will still create Workspaces and WorkspaceMembers tables, but you'll need to manually add the FK to Teams later.

## How to Run

### Option 1: SQL Server Management Studio (SSMS)

1. Open **002_Create_Workspaces_Tables.sql** in SSMS
2. Make sure you're connected to the correct database
3. Update the database name in line 8 if needed: `USE [NexusDb]`
4. Execute the script (F5 or Execute button)
5. Check the Messages tab for success/error messages

### Option 2: Command Line (sqlcmd)

```bash
# Windows
sqlcmd -S localhost -d NexusDb -i 002_Create_Workspaces_Tables.sql

# With authentication
sqlcmd -S localhost -U sa -P YourPassword -d NexusDb -i 002_Create_Workspaces_Tables.sql
```

### Option 3: Azure Data Studio

1. Open **002_Create_Workspaces_Tables.sql**
2. Connect to your database
3. Click "Run" or press F5
4. Check the Messages pane for output

### Option 4: From .NET Application

```csharp
// Read and execute SQL script
var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Migrations", "002_Create_Workspaces_Tables.sql");
var script = await File.ReadAllTextAsync(scriptPath);

using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();

// Split by GO statements and execute each batch
var batches = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
foreach (var batch in batches)
{
    if (!string.IsNullOrWhiteSpace(batch))
    {
        using var command = new SqlCommand(batch, connection);
        await command.ExecuteNonQueryAsync();
    }
}
```

## Rollback

If you need to undo this migration:

```bash
# SSMS or Azure Data Studio
# Execute: 002_Rollback_Workspaces_Tables.sql

# sqlcmd
sqlcmd -S localhost -d NexusDb -i 002_Rollback_Workspaces_Tables.sql
```

⚠️ **WARNING**: 
- Rollback will delete all Workspaces data!
- If you have Collections referencing Workspaces, they will have broken references after rollback

## Verification

After running the migration, verify it succeeded:

```sql
-- Check if tables exist
SELECT name FROM sys.tables 
WHERE name IN ('Workspaces', 'WorkspaceMembers')
ORDER BY name;

-- Check indexes
SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE t.name IN ('Workspaces', 'WorkspaceMembers')
AND i.name IS NOT NULL
ORDER BY t.name, i.name;

-- Check foreign keys
SELECT 
    fk.name AS ForeignKey,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fk.parent_object_id, fkc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
    COL_NAME(fk.referenced_object_id, fkc.referenced_column_id) AS ReferencedColumn
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
WHERE OBJECT_NAME(fk.parent_object_id) IN ('Workspaces', 'WorkspaceMembers')
ORDER BY fk.name;

-- Check constraints
SELECT 
    t.name AS TableName,
    cc.name AS ConstraintName,
    cc.definition AS ConstraintDefinition
FROM sys.check_constraints cc
INNER JOIN sys.tables t ON cc.parent_object_id = t.object_id
WHERE t.name IN ('Workspaces', 'WorkspaceMembers')
ORDER BY t.name, cc.name;

-- Check for any workspaces (should be empty initially)
SELECT COUNT(*) AS WorkspaceCount FROM Workspaces;
SELECT COUNT(*) AS MemberCount FROM WorkspaceMembers;
```

## Relationship with Collections

**IMPORTANT**: Collections have a `WorkspaceId` foreign key that references Workspaces. This means:

1. ✅ Workspaces should be created BEFORE Collections
2. ✅ Every Collection must belong to a Workspace
3. ⚠️ Deleting a Workspace will NOT cascade delete Collections (you need to handle this in your application logic or add a CASCADE DELETE constraint)

If Collections already exist in your database, you may need to update them with valid WorkspaceIds after creating Workspaces.

## Migration Order

The correct order for running all migrations:

1. **Users** (from Identity system)
2. **Teams** (`001_Create_Teams_Tables.sql`)
3. **Workspaces** (`002_Create_Workspaces_Tables.sql`) ← YOU ARE HERE
4. **Collections** (if not already created)
5. **Documents, Diagrams, etc.**

## Troubleshooting

### Error: Teams table not found

If you get this error:
```
⚠ Teams table not found - FK_Workspaces_Teams_TeamId will need to be added later
```

**Solution**: Run the Teams migration first (`001_Create_Teams_Tables.sql`), then manually add the FK:

```sql
ALTER TABLE [dbo].[Workspaces]
    ADD CONSTRAINT [FK_Workspaces_Teams_TeamId]
    FOREIGN KEY ([TeamId]) REFERENCES [dbo].[Teams]([Id]);
```

### Error: Users table not found

The script will skip foreign keys to Users if the table doesn't exist. Add them manually later:

```sql
-- Add all User FKs
ALTER TABLE [dbo].[Workspaces]
    ADD CONSTRAINT [FK_Workspaces_Users_CreatedBy]
    FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users]([Id]);

ALTER TABLE [dbo].[Workspaces]
    ADD CONSTRAINT [FK_Workspaces_Users_DeletedBy]
    FOREIGN KEY ([DeletedBy]) REFERENCES [dbo].[Users]([Id]);

ALTER TABLE [dbo].[WorkspaceMembers]
    ADD CONSTRAINT [FK_WorkspaceMembers_Users_UserId]
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]);

ALTER TABLE [dbo].[WorkspaceMembers]
    ADD CONSTRAINT [FK_WorkspaceMembers_Users_AddedBy]
    FOREIGN KEY ([AddedBy]) REFERENCES [dbo].[Users]([Id]);
```

### Error: Tables already exist

The script is idempotent - it checks for existing tables and skips creation if they exist. You can safely re-run it.

### Error: Permission denied

Make sure your SQL user has:
- CREATE TABLE permission
- ALTER TABLE permission
- CREATE INDEX permission

## Test Data (Optional)

After migration, create some test data:

```sql
-- Assume you have existing Teams and Users
DECLARE @TeamId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Teams);
DECLARE @UserId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Users);

-- Create a test workspace
DECLARE @WorkspaceId UNIQUEIDENTIFIER = NEWID();

INSERT INTO Workspaces (Id, Name, Description, TeamId, CreatedBy, CreatedAt, UpdatedAt)
VALUES (
    @WorkspaceId, 
    'Development Workspace', 
    'Main workspace for development team', 
    @TeamId, 
    @UserId, 
    GETUTCDATE(), 
    GETUTCDATE()
);

-- Add workspace member (Owner role)
INSERT INTO WorkspaceMembers (Id, WorkspaceId, UserId, Role, AddedBy, AddedAt, IsActive)
VALUES (
    NEWID(), 
    @WorkspaceId, 
    @UserId, 
    4, -- Owner
    @UserId, 
    GETUTCDATE(), 
    1
);

-- Verify
SELECT 
    w.Name AS WorkspaceName,
    w.Description,
    t.Name AS TeamName,
    u.Email AS CreatedBy,
    (SELECT COUNT(*) FROM WorkspaceMembers WHERE WorkspaceId = w.Id) AS MemberCount
FROM Workspaces w
INNER JOIN Teams t ON w.TeamId = t.Id
INNER JOIN Users u ON w.CreatedBy = u.Id;

-- View members
SELECT 
    w.Name AS WorkspaceName,
    u.Email AS MemberEmail,
    CASE wm.Role
        WHEN 1 THEN 'Viewer'
        WHEN 2 THEN 'Editor'
        WHEN 3 THEN 'Admin'
        WHEN 4 THEN 'Owner'
    END AS Role
FROM WorkspaceMembers wm
INNER JOIN Workspaces w ON wm.WorkspaceId = w.Id
INNER JOIN Users u ON wm.UserId = u.Id
WHERE wm.IsActive = 1;
```

## Role Hierarchy

Understanding workspace roles:

| Role | Value | Can View | Can Edit | Can Add Members | Can Delete |
|------|-------|----------|----------|-----------------|------------|
| Viewer | 1 | ✅ | ❌ | ❌ | ❌ |
| Editor | 2 | ✅ | ✅ | ❌ | ❌ |
| Admin | 3 | ✅ | ✅ | ✅ | ❌ |
| Owner | 4 | ✅ | ✅ | ✅ | ✅ |

**Best Practices:**
- Every workspace should have at least one Owner
- Limit the number of Owners to 2-3 trusted users
- Use Editor role for most active contributors
- Use Viewer role for read-only access

## Support

For issues or questions:
- Check the error messages in the script output
- Verify your database connection
- Ensure you have the necessary permissions
- Make sure Teams migration has been run first
- Review the constraints and foreign keys

## Next Steps

After running this migration:

1. ✅ Verify tables were created successfully
2. ✅ Update your DbContext if needed (already done in AppDbContext.cs)
3. ✅ Create Collections migration (if not already done)
4. ✅ Test the Workspaces endpoints
5. ✅ Create some test data

---

**Created:** 2026-02-08  
**Version:** 1.0  
**Database:** SQL Server 2022  
**Dependencies:** Teams (001_Create_Teams_Tables.sql)
