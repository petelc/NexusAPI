# Workspace Endpoints Documentation

## Overview
Complete set of 10 FastEndpoints for managing workspaces and their members in the NEXUS API.

---

## Endpoints Summary

### Workspace Management (4 endpoints)
1. **POST** `/api/v1/workspaces` - Create workspace
2. **GET** `/api/v1/workspaces/{id}` - Get workspace by ID
3. **PUT** `/api/v1/workspaces/{id}` - Update workspace
4. **DELETE** `/api/v1/workspaces/{id}` - Delete workspace

### Member Management (3 endpoints)
5. **POST** `/api/v1/workspaces/{id}/members` - Add member
6. **DELETE** `/api/v1/workspaces/{id}/members/{userId}` - Remove member
7. **PUT** `/api/v1/workspaces/{id}/members/{userId}/role` - Change member role

### Query Endpoints (3 endpoints)
8. **GET** `/api/v1/workspaces/my` - Get user's workspaces
9. **GET** `/api/v1/teams/{teamId}/workspaces` - Get team workspaces
10. **GET** `/api/v1/workspaces/search` - Search workspaces

---

## Detailed Endpoint Documentation

### 1. Create Workspace
**POST** `/api/v1/workspaces`

**Authorization**: Roles: Editor, Admin

**Request Body**:
```json
{
  "name": "Engineering Team Workspace",
  "description": "Workspace for engineering team documentation",
  "teamId": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Success Response** (201 Created):
```json
{
  "workspaceId": "770e8400-e29b-41d4-a716-446655440111",
  "name": "Engineering Team Workspace",
  "description": "Workspace for engineering team documentation",
  "teamId": "550e8400-e29b-41d4-a716-446655440000",
  "createdBy": "660e8400-e29b-41d4-a716-446655440222",
  "createdAt": "2026-02-07T19:00:00Z",
  "updatedAt": "2026-02-07T19:00:00Z",
  "memberCount": 1,
  "members": [
    {
      "memberId": "880e8400-e29b-41d4-a716-446655440333",
      "userId": "660e8400-e29b-41d4-a716-446655440222",
      "role": "Owner",
      "joinedAt": "2026-02-07T19:00:00Z",
      "invitedBy": null,
      "isActive": true
    }
  ]
}
```

**Error Responses**:
- `400 Bad Request` - Invalid request body or validation error
- `401 Unauthorized` - Not authenticated
- `409 Conflict` - Workspace name already exists for this team

---

### 2. Get Workspace By ID
**GET** `/api/v1/workspaces/{id}`

**Authorization**: Roles: Viewer, Editor, Admin

**Query Parameters**:
- `includeMembers` (optional, boolean) - Include member details in response

**Success Response** (200 OK):
```json
{
  "workspaceId": "770e8400-e29b-41d4-a716-446655440111",
  "name": "Engineering Team Workspace",
  "description": "Workspace for engineering team documentation",
  "teamId": "550e8400-e29b-41d4-a716-446655440000",
  "createdBy": "660e8400-e29b-41d4-a716-446655440222",
  "createdAt": "2026-02-07T19:00:00Z",
  "updatedAt": "2026-02-07T19:00:00Z",
  "memberCount": 3,
  "members": [] // or populated if includeMembers=true
}
```

**Error Responses**:
- `400 Bad Request` - Invalid workspace ID format
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - User is not a member of this workspace
- `404 Not Found` - Workspace doesn't exist

---

### 3. Update Workspace
**PUT** `/api/v1/workspaces/{id}`

**Authorization**: Roles: Editor, Admin (must be Admin or Owner of workspace)

**Request Body**:
```json
{
  "name": "Updated Workspace Name",
  "description": "Updated description"
}
```

**Note**: Both fields are optional. Only provided fields will be updated.

**Success Response** (200 OK): Returns updated workspace object

**Error Responses**:
- `400 Bad Request` - Invalid request
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - User doesn't have permission to update
- `404 Not Found` - Workspace doesn't exist

---

### 4. Delete Workspace
**DELETE** `/api/v1/workspaces/{id}`

**Authorization**: Roles: Admin (must be Owner of workspace)

**Success Response** (204 No Content)

**Error Responses**:
- `400 Bad Request` - Invalid workspace ID
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - Only owners can delete workspaces
- `404 Not Found` - Workspace doesn't exist

---

### 5. Add Member
**POST** `/api/v1/workspaces/{id}/members`

**Authorization**: Roles: Editor, Admin (must be Admin or Owner of workspace)

**Request Body**:
```json
{
  "userId": "990e8400-e29b-41d4-a716-446655440444",
  "role": "Editor"
}
```

**Valid Roles**: Viewer, Editor, Admin, Owner

**Success Response** (201 Created):
```json
{
  "memberId": "aa0e8400-e29b-41d4-a716-446655440555",
  "userId": "990e8400-e29b-41d4-a716-446655440444",
  "role": "Editor",
  "joinedAt": "2026-02-07T19:00:00Z",
  "invitedBy": "660e8400-e29b-41d4-a716-446655440222",
  "isActive": true
}
```

**Error Responses**:
- `400 Bad Request` - Invalid request or user already a member
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - User doesn't have permission to add members
- `404 Not Found` - Workspace doesn't exist

---

### 6. Remove Member
**DELETE** `/api/v1/workspaces/{id}/members/{userId}`

**Authorization**: Roles: Editor, Admin (must be Admin or Owner of workspace)

**Success Response** (204 No Content)

**Error Responses**:
- `400 Bad Request` - Cannot remove last owner
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - User doesn't have permission to remove members
- `404 Not Found` - Workspace or member doesn't exist

---

### 7. Change Member Role
**PUT** `/api/v1/workspaces/{id}/members/{userId}/role`

**Authorization**: Roles: Editor, Admin (must be Admin or Owner of workspace)

**Request Body**:
```json
{
  "newRole": "Admin"
}
```

**Valid Roles**: Viewer, Editor, Admin, Owner

**Success Response** (200 OK): Returns updated member object

**Error Responses**:
- `400 Bad Request` - Invalid role or cannot demote last owner
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - User doesn't have permission to change roles
- `404 Not Found` - Workspace or member doesn't exist

---

### 8. Get User Workspaces
**GET** `/api/v1/workspaces/my`

**Authorization**: Roles: Viewer, Editor, Admin

**Success Response** (200 OK):
```json
[
  {
    "workspaceId": "770e8400-e29b-41d4-a716-446655440111",
    "name": "Engineering Team Workspace",
    "description": "Workspace for engineering team documentation",
    "teamId": "550e8400-e29b-41d4-a716-446655440000",
    "createdBy": "660e8400-e29b-41d4-a716-446655440222",
    "createdAt": "2026-02-07T19:00:00Z",
    "updatedAt": "2026-02-07T19:00:00Z",
    "memberCount": 3,
    "members": []
  },
  {
    "workspaceId": "bb0e8400-e29b-41d4-a716-446655440666",
    "name": "Product Team Workspace",
    "description": "Product documentation and planning",
    "teamId": "550e8400-e29b-41d4-a716-446655440000",
    "createdBy": "cc0e8400-e29b-41d4-a716-446655440777",
    "createdAt": "2026-02-05T14:30:00Z",
    "updatedAt": "2026-02-06T09:15:00Z",
    "memberCount": 5,
    "members": []
  }
]
```

**Error Responses**:
- `401 Unauthorized` - Not authenticated

---

### 9. Get Team Workspaces
**GET** `/api/v1/teams/{teamId}/workspaces`

**Authorization**: Roles: Viewer, Editor, Admin

**Success Response** (200 OK): Returns array of workspaces (same format as Get User Workspaces)

**Note**: Only returns workspaces where the current user is a member

**Error Responses**:
- `400 Bad Request` - Invalid team ID format
- `401 Unauthorized` - Not authenticated

---

### 10. Search Workspaces
**GET** `/api/v1/workspaces/search`

**Authorization**: Roles: Viewer, Editor, Admin

**Query Parameters**:
- `q` (required, string) - Search term
- `teamId` (optional, guid) - Filter by team

**Example**: `/api/v1/workspaces/search?q=engineering&teamId=550e8400-e29b-41d4-a716-446655440000`

**Success Response** (200 OK): Returns array of workspaces matching the search term

**Note**: Only returns workspaces where the current user is a member

**Error Responses**:
- `400 Bad Request` - Missing or invalid search term
- `401 Unauthorized` - Not authenticated

---

## Authorization Matrix

| Endpoint | Viewer | Editor | Admin | Owner |
|----------|--------|--------|-------|-------|
| Create Workspace | ❌ | ✅ | ✅ | N/A |
| Get Workspace | ✅ | ✅ | ✅ | ✅ |
| Update Workspace | ❌ | ❌ | ✅ | ✅ |
| Delete Workspace | ❌ | ❌ | ❌ | ✅ |
| Add Member | ❌ | ❌ | ✅ | ✅ |
| Remove Member | ❌ | ❌ | ✅ | ✅ |
| Change Member Role | ❌ | ❌ | ✅ | ✅ |
| Get User Workspaces | ✅ | ✅ | ✅ | ✅ |
| Get Team Workspaces | ✅ | ✅ | ✅ | ✅ |
| Search Workspaces | ✅ | ✅ | ✅ | ✅ |

---

## Common HTTP Status Codes

- **200 OK** - Request successful
- **201 Created** - Resource created successfully
- **204 No Content** - Request successful, no response body
- **400 Bad Request** - Invalid request format or validation error
- **401 Unauthorized** - Authentication required or failed
- **403 Forbidden** - Authenticated but insufficient permissions
- **404 Not Found** - Resource doesn't exist
- **409 Conflict** - Resource conflict (e.g., duplicate name)
- **500 Internal Server Error** - Server error

---

## Authentication

All endpoints require authentication using JWT Bearer tokens:

```
Authorization: Bearer <token>
```

The token must contain a valid user ID claim (`uid`).

---

## Workspace Member Roles

### Viewer (Level 1)
- View workspace and its contents
- Cannot edit or manage

### Editor (Level 2)
- All Viewer permissions
- Edit workspace contents
- Cannot manage members or settings

### Admin (Level 3)
- All Editor permissions
- Add/remove members
- Change member roles
- Update workspace settings
- Cannot delete workspace

### Owner (Level 4)
- All Admin permissions
- Delete workspace
- Full control
- At least one Owner required per workspace

---

## Integration Steps

### 1. Copy Files
Extract the endpoints to your API Web project:
```bash
unzip workspace-endpoints-complete.zip -d src/Nexus.API.Web/Endpoints/
```

### 2. Register Handlers (Already done in UseCases layer)
```csharp
// In Program.cs
builder.Services.AddWorkspaceHandlers();
```

### 3. FastEndpoints Auto-Discovery
FastEndpoints will automatically discover and register these endpoints.

### 4. Test Endpoints
```bash
# Create workspace
curl -X POST https://localhost:5001/api/v1/workspaces \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "My Workspace",
    "description": "Test workspace",
    "teamId": "550e8400-e29b-41d4-a716-446655440000"
  }'

# Get user workspaces
curl -X GET https://localhost:5001/api/v1/workspaces/my \
  -H "Authorization: Bearer <token>"

# Search workspaces
curl -X GET "https://localhost:5001/api/v1/workspaces/search?q=engineering" \
  -H "Authorization: Bearer <token>"
```

---

## Error Response Format

All error responses follow this format:
```json
{
  "error": "Error message describing what went wrong"
}
```

---

## Notes

1. **Soft Deletes**: The DELETE endpoint performs a soft delete (sets `IsDeleted = true`)
2. **Member Filtering**: All list/search endpoints automatically filter to show only workspaces where the user is an active member
3. **Automatic Owner**: When creating a workspace, the creator is automatically added as an Owner
4. **Last Owner Protection**: Cannot remove or demote the last Owner of a workspace
5. **Case-Insensitive Search**: Search is case-insensitive on workspace name and description

---

## Files Included

1. `CreateWorkspaceEndpoint.cs` - Create new workspace
2. `GetWorkspaceByIdEndpoint.cs` - Get workspace by ID
3. `UpdateWorkspaceEndpoint.cs` - Update workspace
4. `DeleteWorkspaceEndpoint.cs` - Delete workspace
5. `AddMemberEndpoint.cs` - Add member to workspace
6. `RemoveMemberEndpoint.cs` - Remove member from workspace
7. `ChangeMemberRoleEndpoint.cs` - Change member role
8. `GetUserWorkspacesEndpoint.cs` - Get user's workspaces
9. `GetTeamWorkspacesEndpoint.cs` - Get team workspaces
10. `SearchWorkspacesEndpoint.cs` - Search workspaces

---

## Version
**Version**: 1.0  
**Date**: February 7, 2026  
**Author**: NEXUS Development Team
