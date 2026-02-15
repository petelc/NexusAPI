# Teams Migration - SQL Scripts

## Overview

This migration creates the Teams and TeamMembers tables for the NEXUS Knowledge Management System.

## Files

1. **001_Create_Teams_Tables.sql** - Main migration script
2. **001_Rollback_Teams_Tables.sql** - Rollback script (if needed)

## Database Schema

### Tables Created

#### Teams
- `Id` (UNIQUEIDENTIFIER, PK)
- `Name` (NVARCHAR(200), NOT NULL)
- `Description` (NVARCHAR(1000), NULL)
- `CreatedBy` (UNIQUEIDENTIFIER, NOT NULL)
- `CreatedAt` (DATETIME2(7), NOT NULL)
- `UpdatedAt` (DATETIME2(7), NOT NULL)
- `IsDeleted` (BIT, NOT NULL, DEFAULT 0)
- `DeletedAt` (DATETIME2(7), NULL)
- `DeletedBy` (UNIQUEIDENTIFIER, NULL)

#### TeamMembers
- `Id` (UNIQUEIDENTIFIER, PK)
- `TeamId` (UNIQUEIDENTIFIER, NOT NULL, FK to Teams)
- `UserId` (UNIQUEIDENTIFIER, NOT NULL, FK to Users)
- `Role` (INT, NOT NULL, DEFAULT 1)
  - 1 = Member
  - 2 = Admin
  - 3 = Owner
- `InvitedBy` (UNIQUEIDENTIFIER, NULL, FK to Users)
- `JoinedAt` (DATETIME2(7), NOT NULL)
- `IsActive` (BIT, NOT NULL, DEFAULT 1)
- `LeftAt` (DATETIME2(7), NULL)

### Indexes

**Teams:**
- IX_Teams_Name (filtered where IsDeleted = 0)
- IX_Teams_CreatedBy
- IX_Teams_CreatedAt
- IX_Teams_IsDeleted

**TeamMembers:**
- IX_TeamMembers_TeamId (covering: UserId, Role, IsActive)
- IX_TeamMembers_UserId (covering: TeamId, Role, IsActive)
- IX_TeamMembers_TeamId_IsActive (filtered where IsActive = 1)
- IX_TeamMembers_Role (filtered where IsActive = 1)

### Foreign Keys

- FK_TeamMembers_Teams_TeamId (CASCADE DELETE)
- FK_Teams_Users_CreatedBy
- FK_Teams_Users_DeletedBy
- FK_TeamMembers_Users_UserId
- FK_TeamMembers_Users_InvitedBy

### Constraints

- UQ_TeamMembers_TeamUser (unique team-user combination)
- CK_TeamMembers_Role (role must be 1-3)
- CK_Teams_DeletedAt (soft delete consistency)
- CK_TeamMembers_LeftAt (active/left consistency)

## How to Run

### Option 1: SQL Server Management Studio (SSMS)

1. Open **001_Create_Teams_Tables.sql** in SSMS
2. Make sure you're connected to the correct database
3. Update the database name in line 8 if needed: `USE [NexusDb]`
4. Execute the script (F5 or Execute button)
5. Check the Messages tab for success/error messages

### Option 2: Command Line (sqlcmd)

```bash
# Windows
sqlcmd -S localhost -d NexusDb -i 001_Create_Teams_Tables.sql

# With authentication
sqlcmd -S localhost -U sa -P YourPassword -d NexusDb -i 001_Create_Teams_Tables.sql
```

### Option 3: Azure Data Studio

1. Open **001_Create_Teams_Tables.sql**
2. Connect to your database
3. Click "Run" or press F5
4. Check the Messages pane for output

### Option 4: From .NET Application

```csharp
// Read and execute SQL script
var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Migrations", "001_Create_Teams_Tables.sql");
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
# Execute: 001_Rollback_Teams_Tables.sql

# sqlcmd
sqlcmd -S localhost -d NexusDb -i 001_Rollback_Teams_Tables.sql
```

⚠️ **WARNING**: Rollback will delete all Teams data!

## Verification

After running the migration, verify it succeeded:

```sql
-- Check if tables exist
SELECT name FROM sys.tables 
WHERE name IN ('Teams', 'TeamMembers')
ORDER BY name;

-- Check indexes
SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE t.name IN ('Teams', 'TeamMembers')
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
WHERE OBJECT_NAME(fk.parent_object_id) IN ('Teams', 'TeamMembers')
ORDER BY fk.name;

-- Check constraints
SELECT 
    t.name AS TableName,
    cc.name AS ConstraintName,
    cc.definition AS ConstraintDefinition
FROM sys.check_constraints cc
INNER JOIN sys.tables t ON cc.parent_object_id = t.object_id
WHERE t.name IN ('Teams', 'TeamMembers')
ORDER BY t.name, cc.name;
```

## Troubleshooting

### Error: Database does not exist

Update line 8 to match your database name:
```sql
USE [YourDatabaseName]
```

### Error: Users table not found

The script will skip foreign keys to Users if the table doesn't exist. You'll need to add them manually later:

```sql
ALTER TABLE [dbo].[Teams]
    ADD CONSTRAINT [FK_Teams_Users_CreatedBy]
    FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[Users]([Id]);
-- Repeat for other FKs...
```

### Error: Tables already exist

The script is idempotent - it checks for existing tables and skips creation if they exist. You can safely re-run it.

### Error: Permission denied

Make sure your SQL user has:
- CREATE TABLE permission
- ALTER TABLE permission
- CREATE INDEX permission

## Next Steps

After running this migration:

1. ✅ Verify tables were created successfully
2. ✅ Update your DbContext if needed (already done in AppDbContext.cs)
3. ✅ Test the Teams endpoints
4. ✅ Create some test data

## Test Data (Optional)

```sql
-- Create a test team
DECLARE @TeamId UNIQUEIDENTIFIER = NEWID();
DECLARE @UserId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Users); -- Use existing user

INSERT INTO Teams (Id, Name, Description, CreatedBy, CreatedAt, UpdatedAt)
VALUES (@TeamId, 'Development Team', 'Main development team', @UserId, GETUTCDATE(), GETUTCDATE());

-- Add team member
INSERT INTO TeamMembers (Id, TeamId, UserId, Role, JoinedAt, IsActive)
VALUES (NEWID(), @TeamId, @UserId, 3, GETUTCDATE(), 1); -- Role 3 = Owner

-- Verify
SELECT * FROM Teams;
SELECT * FROM TeamMembers;
```

## Support

For issues or questions:
- Check the error messages in the script output
- Verify your database connection
- Ensure you have the necessary permissions
- Review the constraints and foreign keys

---

**Created:** 2026-02-08  
**Version:** 1.0  
**Database:** SQL Server 2022
