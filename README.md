# Nexus API - Knowledge Management System

**Version:** 1.0  
**Framework:** .NET 10  
**Architecture:** Clean Architecture with Domain-Driven Design  
**Shared Kernel:** Traxs.SharedKernel

---

## 🏗️ Project Structure

```
Nexus/
├── Nexus.sln                         # Solution file
├── src/
│   ├── Nexus.Core/                   # Domain Layer
│   │   ├── Aggregates/
│   │   │   └── DocumentAggregate/
│   │   │       ├── Document.cs       # ✅ Aggregate Root
│   │   │       ├── DocumentId.cs     # ✅ Strongly-typed ID
│   │   │       ├── DocumentVersion.cs # ✅ Entity
│   │   │       └── Tag.cs            # ✅ Entity
│   │   ├── ValueObjects/
│   │   │   ├── Title.cs              # ✅ Value Object
│   │   │   └── DocumentContent.cs    # ✅ Value Object
│   │   ├── Events/
│   │   │   ├── DocumentCreatedEvent.cs # ✅ Domain Event
│   │   │   └── DocumentEvents.cs     # ✅ Domain Events
│   │   ├── Enums/
│   │   │   └── DocumentStatus.cs     # ✅ Enum
│   │   └── Interfaces/
│   │       └── IDocumentRepository.cs # ✅ Repository Interface
│   │
│   ├── Nexus.UseCases/               # Application Layer (CQRS + MediatR)
│   │
│   ├── Nexus.Infrastructure/         # Infrastructure Layer (EF Core, etc.)
│   │
│   └── Nexus.Web/                    # Presentation Layer (API)
│
└── tests/
    ├── Nexus.Core.Tests/
    ├── Nexus.UseCases.Tests/
    ├── Nexus.Infrastructure.Tests/
    └── Nexus.FunctionalTests/
```

---

## ✅ Completed Components

### Domain Layer (Nexus.Core)

#### Document Aggregate ✅
- **Document** - Aggregate root with Traxs.SharedKernel base classes
  - Create, Update, Publish, Archive, Delete, Restore operations
  - Version management
  - Tag management
  - Business rule enforcement
  - Domain events

- **DocumentId** - Strongly-typed identifier
  - Type-safe ID handling
  - Implicit conversion to Guid

- **DocumentVersion** - Entity for version snapshots
  - Content versioning
  - Change tracking
  - Content hash for deduplication

- **Tag** - Entity for categorization
  - Name and color properties
  - Validation

#### Value Objects ✅
- **Title** - Validated title (1-200 characters)
- **DocumentContent** - Rich text + plain text with word count

#### Domain Events ✅
- DocumentCreatedEvent
- DocumentUpdatedEvent
- DocumentPublishedEvent
- DocumentArchivedEvent
- DocumentDeletedEvent
- DocumentRestoredEvent

#### Enums ✅
- DocumentStatus (Draft, Published, Archived)

#### Repository Interface ✅
- IDocumentRepository with full CRUD + search operations

---

## 📦 NuGet Packages

### Nexus.Core
```xml
<PackageReference Include="Traxs.SharedKernel" Version="10.0.0" />
<PackageReference Include="Ardalis.GuardClauses" Version="5.0.0" />
```

### Nexus.UseCases
```xml
<PackageReference Include="MediatR" Version="13.0.0" />
<PackageReference Include="FluentValidation" Version="11.10.0" />
<PackageReference Include="AutoMapper" Version="13.0.1" />
```

### Nexus.Infrastructure
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.0" />
<PackageReference Include="StackExchange.Redis" Version="2.8.16" />
<PackageReference Include="Azure.Storage.Blobs" Version="12.22.2" />
<PackageReference Include="Elastic.Clients.Elasticsearch" Version="8.16.0" />
```

### Nexus.Web
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="7.2.0" />
<PackageReference Include="Serilog.AspNetCore" Version="10.0.0" />
<PackageReference Include="MediatR" Version="13.0.0" />
```

---

## 🎯 Traxs.SharedKernel Integration

The project uses **Traxs.SharedKernel** (your custom shared kernel) instead of Ardalis.SharedKernel.

### Base Classes Used:
- `EntityBase<TId>` - Base entity class
- `ValueObject` - Base value object class
- `DomainEventBase` - Base domain event class
- `IRepository<T>` - Repository interface
- `IAggregateRoot` - Aggregate root marker interface

### Example Usage:
```csharp
// Aggregate Root
public class Document : EntityBase<DocumentId>, IAggregateRoot
{
    // ... implementation
}

// Value Object
public class Title : ValueObject
{
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}

// Domain Event
public record DocumentCreatedEvent(DocumentId DocumentId, Guid CreatedBy) : DomainEventBase;
```

---

## 🚀 Next Steps

### Phase 1: Complete Application Layer (UseCases)
- [ ] Create Commands (CreateDocument, UpdateDocument, DeleteDocument)
- [ ] Create Command Handlers (MediatR)
- [ ] Create Queries (GetDocumentById, ListDocuments, SearchDocuments)
- [ ] Create Query Handlers (MediatR)
- [ ] Create DTOs and Mapping Profiles (AutoMapper)
- [ ] Add FluentValidation validators

### Phase 2: Infrastructure Layer
- [ ] Create ApplicationDbContext (EF Core)
- [ ] Create Entity Type Configurations
- [ ] Implement DocumentRepository
- [ ] Add Database Migrations
- [ ] Configure Value Object conversions
- [ ] Set up Redis caching
- [ ] Integrate Elasticsearch

### Phase 3: API Layer (Web)
- [ ] Create DocumentsController
- [ ] Configure JWT authentication
- [ ] Set up Swagger/OpenAPI
- [ ] Add middleware (exception handling, logging)
- [ ] Configure CORS
- [ ] Set up SignalR hubs

### Phase 4: Testing
- [ ] Unit tests for Domain
- [ ] Integration tests for UseCases
- [ ] Infrastructure tests
- [ ] Functional API tests

---

## 🛠️ Development Commands

```bash
# Build the solution
dotnet build

# Restore packages
dotnet restore

# Run tests
dotnet test

# Create migration (after setting up Infrastructure)
dotnet ef migrations add InitialCreate --project src/Nexus.Infrastructure --startup-project src/Nexus.Web

# Update database
dotnet ef database update --project src/Nexus.Infrastructure --startup-project src/Nexus.Web

# Run the API
dotnet run --project src/Nexus.Web
```

---

## 📝 Notes

- All timestamps use UTC
- Soft delete pattern implemented for documents
- Version snapshots created automatically on updates
- Domain events raised for all state changes
- Guard clauses used for validation
- Strongly-typed IDs for type safety

---

## 📖 References

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design by Eric Evans](https://www.domainlanguage.com/ddd/)
- [Traxs.SharedKernel NuGet Package](https://www.nuget.org/packages/Traxs.SharedKernel)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)

---

**Created:** January 31, 2026  
**Author:** Nexus Development Team
