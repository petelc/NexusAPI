# Nexus Infrastructure Layer

This directory contains the Infrastructure layer implementation for the Nexus Knowledge Management System, built on the Ardalis Clean Architecture template.

## 📁 Structure

```
Nexus.Infrastructure/
├── Data/
│   ├── Config/                    # EF Core entity configurations
│   │   ├── DocumentConfiguration.cs
│   │   ├── DocumentVersionConfiguration.cs
│   │   └── TagConfiguration.cs
│   ├── Repositories/              # Repository implementations
│   │   └── DocumentRepository.cs
│   ├── AppDbContext.cs           # Main database context
│   └── RepositoryBase.cs         # Generic repository base
├── Services/                      # External service implementations
│   ├── BlobStorageService.cs     # Azure Blob Storage
│   ├── EmailService.cs           # SMTP Email
│   ├── ElasticsearchService.cs   # Full-text search
│   └── RedisCacheService.cs      # Distributed caching
├── GlobalUsings.cs               # Global using statements
├── InfrastructureServiceExtensions.cs  # DI registration
└── Nexus.Infrastructure.csproj   # Project file
```

## 🚀 Features

### Database (SQL Server)
- Entity Framework Core 10
- SQL Server 2022
- Automatic migrations
- Connection retry logic
- Value object conversions
- Optimized indexes
- Soft delete support

### Caching (Redis)
- Distributed caching with StackExchange.Redis
- Get/Set with expiration
- Sliding expiration
- Pattern-based invalidation
- Pub/Sub messaging
- Cache statistics

### Search (Elasticsearch)
- Full-text search
- Fuzzy matching
- Highlighting
- Multi-field search
- Autocomplete/suggestions
- Tag filtering

### File Storage (Azure Blob Storage)
- File upload/download
- SAS token generation
- Metadata retrieval
- Local development with Azurite

### Email (SMTP)
- HTML and plain text emails
- Template support
- Bulk sending
- Welcome/password reset flows

## 🛠️ Setup

### 1. Start Infrastructure Services

```bash
# Start all services (SQL Server, Redis, Elasticsearch, etc.)
docker-compose up -d

# Check service health
docker-compose ps

# View logs
docker-compose logs -f
```

### 2. Configure Connection Strings

Update `appsettings.json` in your Web project:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=NexusDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True",
    "Redis": "localhost:6379",
    "AzureStorage": "UseDevelopmentStorage=true"
  },
  "Elasticsearch": {
    "Uri": "http://localhost:9200"
  },
  "Email": {
    "SmtpHost": "localhost",
    "SmtpPort": "1025"
  }
}
```

### 3. Create Migration

```bash
dotnet ef migrations add InitialCreate \
  --project src/Nexus.Infrastructure \
  --startup-project src/Nexus.Web \
  --context AppDbContext \
  --output-dir Data/Migrations
```

### 4. Apply Migration

```bash
dotnet ef database update \
  --project src/Nexus.Infrastructure \
  --startup-project src/Nexus.Web \
  --context AppDbContext
```

## 📊 Infrastructure Services

### SQL Server
- **Port**: 1433
- **Username**: sa
- **Password**: YourStrong@Passw0rd
- **Database**: NexusDB

### Redis
- **Port**: 6379
- **UI (RedisInsight)**: http://localhost:5540

### Elasticsearch
- **HTTP**: http://localhost:9200
- **Transport**: 9300
- **UI (Kibana)**: http://localhost:5601

### Azurite (Local Azure Storage)
- **Blob**: http://localhost:10000
- **Queue**: http://localhost:10001
- **Table**: http://localhost:10002

## 🔧 Service Registration

Services are registered in `Program.cs`:

```csharp
builder.Services.AddInfrastructureServices(
    builder.Configuration,
    builder.Logging.CreateLogger("Startup"));

// After building the app
await app.Services.ApplyMigrationsAsync();
await app.Services.InitializeExternalServicesAsync();
```

## 📦 NuGet Packages

```xml
<!-- Database -->
<PackageReference Include="Microsoft.EntityFrameworkCore" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" />

<!-- Caching -->
<PackageReference Include="StackExchange.Redis" Version="2.8.16" />

<!-- Search -->
<PackageReference Include="Elastic.Clients.Elasticsearch" Version="8.16.2" />

<!-- Storage -->
<PackageReference Include="Azure.Storage.Blobs" Version="12.23.0" />

<!-- Base Repository -->
<PackageReference Include="Traxs.SharedKernel" Version="0.1.3" />
```

## 🎯 Service Interfaces

All services implement interfaces defined in `Nexus.Core/Interfaces/`:

- `IRepository<T>` - Generic repository
- `IDocumentRepository` - Document-specific operations
- `IStorageService` - File storage operations
- `IEmailService` - Email operations
- `ISearchService` - Search operations
- `ICacheService` - Caching operations

See `CORE_INTERFACES.md` for interface definitions.

## 📝 Usage Examples

### Using Cache Service
```csharp
public class MyService
{
    private readonly ICacheService _cache;

    public async Task<Document> GetDocumentAsync(Guid id)
    {
        return await _cache.GetOrCreateAsync(
            $"document:{id}",
            async () => await _repository.GetByIdAsync(id),
            TimeSpan.FromMinutes(15));
    }
}
```

### Using Search Service
```csharp
public class SearchService
{
    private readonly ISearchService _search;

    public async Task<SearchResults> SearchAsync(string query)
    {
        return await _search.SearchDocumentsAsync(
            query,
            page: 1,
            pageSize: 20,
            tags: new[] { "api", "documentation" });
    }
}
```

## 🔄 Migration Commands

### Create Migration
```bash
dotnet ef migrations add <MigrationName> \
  --project src/Nexus.Infrastructure \
  --startup-project src/Nexus.Web \
  --context AppDbContext \
  --output-dir Data/Migrations
```

### Apply Migration
```bash
dotnet ef database update \
  --project src/Nexus.Infrastructure \
  --startup-project src/Nexus.Web
```

### Generate SQL Script
```bash
dotnet ef migrations script \
  --project src/Nexus.Infrastructure \
  --startup-project src/Nexus.Web \
  --output migration.sql
```

## 🧹 Cleanup

```bash
# Stop all services
docker-compose down

# Remove all data (CAUTION!)
docker-compose down -v
```

## 🔐 Production Considerations

1. **Connection Strings**: Use Azure Key Vault
2. **Redis**: Use Azure Redis Cache
3. **Elasticsearch**: Use Elastic Cloud
4. **Blob Storage**: Use Azure Blob Storage
5. **Email**: Use SendGrid or AWS SES
6. **SSL/TLS**: Enable for all connections
7. **Monitoring**: Add Application Insights

## 📚 Documentation

- [Integration Guide](INTEGRATION_GUIDE.md)
- [Checklist](CHECKLIST.md)
- [Core Interfaces](CORE_INTERFACES.md)

---

**Status**: ✅ Ready for Integration  
**Last Updated**: January 31, 2026
