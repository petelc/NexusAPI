# Nexus API - Endpoint Reference

## Authentication (9 endpoints)

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| POST | `/auth/register` | Register a new user | Anonymous |
| POST | `/auth/login` | Authenticate with email/password | Anonymous |
| POST | `/auth/logout` | Invalidate refresh token | Anonymous |
| POST | `/auth/refresh` | Refresh access token | Anonymous |
| POST | `/auth/forgot-password` | Request password reset email | Anonymous |
| POST | `/auth/reset-password` | Reset password using token | Anonymous |
| POST | `/auth/2fa/enable` | Generate QR code for 2FA setup | Viewer+ |
| POST | `/auth/2fa/verify` | Verify 2FA code and complete setup | Viewer+ |
| POST | `/auth/2fa/disable` | Disable 2FA for user account | Viewer+ |

---

## Documents (11 endpoints)

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| POST | `/documents` | Create a new document | Anonymous |
| GET | `/documents` | List documents with pagination/filtering | Anonymous |
| GET | `/documents/{id}` | Get document by ID | Anonymous |
| PUT | `/documents/{id}` | Update document title, content, status | Editor+ |
| DELETE | `/documents/{id}` | Soft/hard delete a document (owner only) | Editor+ |
| POST | `/documents/{id}/publish` | Publish a draft document | Anonymous |
| POST | `/documents/{id}/tags` | Add tags to a document | Editor+ |
| DELETE | `/documents/{id}/tags/{tagName}` | Remove a tag from a document | Editor+ |
| GET | `/documents/{id}/versions` | List document version history | Viewer+ |
| GET | `/documents/{id}/versions/{versionNumber}` | Get a specific version | Viewer+ |
| POST | `/documents/{id}/versions/{versionNumber}/restore` | Restore to a previous version | Editor+ |

---

## Diagrams (14 endpoints)

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| POST | `/diagrams` | Create a new diagram | Editor+ |
| GET | `/diagrams/{diagramId}` | Get diagram with elements, connections, layers | Viewer+ |
| PUT | `/diagrams/{diagramId}` | Update diagram title and/or canvas | Editor+ |
| DELETE | `/diagrams/{diagramId}` | Soft delete a diagram | Editor+ |
| GET | `/diagrams/my` | Get current user's diagrams (paginated) | Viewer+ |
| POST | `/diagrams/{diagramId}/elements` | Add an element | Editor+ |
| PUT | `/diagrams/{diagramId}/elements/{elementId}` | Update element position/size/style | Editor+ |
| DELETE | `/diagrams/{diagramId}/elements/{elementId}` | Delete an element | Editor+ |
| POST | `/diagrams/{diagramId}/connections` | Add a connection between elements | Editor+ |
| PUT | `/diagrams/{diagramId}/connections/{connectionId}` | Update connection label/style | Editor+ |
| DELETE | `/diagrams/{diagramId}/connections/{connectionId}` | Delete a connection | Editor+ |
| POST | `/diagrams/{diagramId}/layers` | Add a layer | Editor+ |
| PUT | `/diagrams/{diagramId}/layers/{layerId}` | Update layer properties | Editor+ |
| DELETE | `/diagrams/{diagramId}/layers/{layerId}` | Delete a layer | Editor+ |

---

## Code Snippets (12 endpoints)

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| POST | `/snippets` | Create a new code snippet | Editor+ |
| GET | `/snippets/{id}` | Get snippet by ID (increments view count) | Viewer+ |
| PUT | `/snippets/{id}` | Update a snippet (owner only) | Editor+ |
| DELETE | `/snippets/{id}` | Soft delete a snippet (owner only) | Viewer+ |
| GET | `/snippets/my` | Get current user's snippets | Viewer+ |
| GET | `/snippets/public` | Get public snippets (paginated) | Viewer+ |
| POST | `/snippets/{id}/publish` | Make a snippet public | Viewer+ |
| POST | `/snippets/{id}/unpublish` | Make a snippet private | Viewer+ |
| POST | `/snippets/{id}/fork` | Fork a public snippet | Viewer+ |
| GET | `/snippets/search` | Search snippets by keyword | Viewer+ |
| GET | `/snippets/by-language/{language}` | Filter snippets by language | Viewer+ |
| GET | `/snippets/by-tag/{tagName}` | Filter snippets by tag | Viewer+ |

---

## Collections (11 endpoints)

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| POST | `/collections` | Create a collection (root or nested) | Editor+ |
| GET | `/collections/{id}` | Get collection with items | Viewer+ |
| PUT | `/collections/{id}` | Update collection properties | Editor+ |
| DELETE | `/collections/{id}` | Soft delete a collection | Admin |
| GET | `/collections/{id}/breadcrumb` | Get ancestor chain | Viewer+ |
| GET | `/collections/{parentId}/children` | Get child collections | Viewer+ |
| GET | `/workspaces/{workspaceId}/collections/roots` | Get root collections in workspace | Viewer+ |
| GET | `/workspaces/{workspaceId}/collections/search` | Search collections | Viewer+ |
| POST | `/collections/{collectionId}/items` | Add item to collection | Editor+ |
| DELETE | `/collections/{collectionId}/items/{itemReferenceId}` | Remove item from collection | Editor+ |
| PUT | `/collections/{collectionId}/items/{itemReferenceId}/order` | Reorder item | Editor+ |

---

## Teams (9 endpoints)

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| POST | `/teams` | Create a team (creator becomes owner) | Editor+ |
| GET | `/teams/{id}` | Get team by ID | Viewer+ |
| PUT | `/teams/{id}` | Update team name/description | Editor+ |
| DELETE | `/teams/{id}` | Soft delete a team (owner only) | Admin |
| GET | `/teams/my` | Get current user's teams | Viewer+ |
| GET | `/teams/search` | Search teams by name | Viewer+ |
| POST | `/teams/{id}/members` | Add a member | Editor+ |
| DELETE | `/teams/{id}/members/{userId}` | Remove a member | Viewer+ |
| PUT | `/teams/{id}/members/{userId}/role` | Change member role | Editor+ |

---

## Workspaces (10 endpoints)

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| POST | `/workspaces` | Create a workspace for a team | Editor+ |
| GET | `/workspaces/{id}` | Get workspace by ID | Viewer+ |
| PUT | `/workspaces/{id}` | Update workspace name/description | Editor+ |
| DELETE | `/workspaces/{id}` | Soft delete a workspace (owner only) | Admin |
| GET | `/workspaces/my` | Get current user's workspaces | Viewer+ |
| GET | `/workspaces/search` | Search workspaces by name | Viewer+ |
| GET | `/teams/{teamId}/workspaces` | Get all workspaces for a team | Viewer+ |
| POST | `/workspaces/{id}/members` | Add a member | Editor+ |
| DELETE | `/workspaces/{id}/members/{userId}` | Remove a member | Editor+ |
| PUT | `/workspaces/{id}/members/{userId}/role` | Change member role | Editor+ |

---

## Collaboration - Sessions (7 endpoints)

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| POST | `/collaboration/sessions` | Start a collaboration session | Editor+ |
| GET | `/collaboration/sessions/{id}` | Get session details | Viewer+ |
| GET | `/collaboration/sessions/my` | Get user's sessions | Viewer+ |
| GET | `/collaboration/sessions/active` | Get active sessions for a resource | Viewer+ |
| POST | `/collaboration/sessions/{id}/join` | Join a session | Viewer+ |
| POST | `/collaboration/sessions/{id}/leave` | Leave a session | Viewer+ |
| POST | `/collaboration/sessions/{id}/end` | End a session | Editor+ |

---

## Collaboration - Comments (6 endpoints)

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| POST | `/collaboration/comments` | Add a comment to a resource | Viewer+ |
| GET | `/collaboration/comments` | Get all comments for a resource | Viewer+ |
| GET | `/collaboration/comments/{id}` | Get comment with replies | Viewer+ |
| PUT | `/collaboration/comments/{id}` | Update a comment (owner only) | Viewer+ |
| DELETE | `/collaboration/comments/{id}` | Soft delete a comment (owner only) | Viewer+ |
| POST | `/collaboration/comments/{id}/replies` | Reply to a comment | Viewer+ |

---

## Permissions (3 endpoints)

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/permissions` | List permissions for a resource | Viewer+ |
| POST | `/permissions` | Grant permission to a user | Editor+ |
| DELETE | `/permissions/{id}` | Revoke a permission | Editor+ |

---

## Tags (2 endpoints)

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/tags` | Get all tags | Viewer+ |
| GET | `/tags/search` | Search tags by keyword | Viewer+ |

---

## Search (1 endpoint)

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/search` | Global search across documents, diagrams, snippets | Anonymous |

---

## Summary

| Feature | Endpoints |
|---------|-----------|
| Authentication | 9 |
| Documents | 11 |
| Diagrams | 14 |
| Code Snippets | 12 |
| Collections | 11 |
| Teams | 9 |
| Workspaces | 10 |
| Collaboration - Sessions | 7 |
| Collaboration - Comments | 6 |
| Permissions | 3 |
| Tags | 2 |
| Search | 1 |
| **Total** | **95** |

### Auth Legend
- **Anonymous** — No authentication required
- **Viewer+** — Requires Viewer, Editor, or Admin role
- **Editor+** — Requires Editor or Admin role
- **Admin** — Requires Admin role
