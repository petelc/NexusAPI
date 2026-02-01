using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Nexus.API.Core.Interfaces;
using Nexus.API.Core.Models;

namespace Nexus.API.Infrastructure.Services;

/// <summary>
/// Azure Blob Storage service for managing file uploads and downloads.
/// Implements the IStorageService interface from the Core layer.
/// </summary>
public class BlobStorageService : IStorageService
{
  private readonly BlobServiceClient _blobServiceClient;
  private readonly ILogger<BlobStorageService> _logger;
  private readonly string _containerName;

  public BlobStorageService(
    BlobServiceClient blobServiceClient,
    ILogger<BlobStorageService> logger,
    IConfiguration configuration)
  {
    _blobServiceClient = blobServiceClient;
    _logger = logger;
    _containerName = configuration["AzureStorage:ContainerName"] ?? "nexus-files";
  }

  /// <summary>
  /// Upload a file to blob storage
  /// </summary>
  public async Task<string> UploadFileAsync(
    string fileName,
    Stream fileStream,
    string contentType,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
      await containerClient.CreateIfNotExistsAsync(
        PublicAccessType.None,
        cancellationToken: cancellationToken);

      // Generate unique blob name
      var blobName = $"{Guid.NewGuid()}/{fileName}";
      var blobClient = containerClient.GetBlobClient(blobName);

      // Upload with metadata
      var blobHttpHeaders = new BlobHttpHeaders
      {
        ContentType = contentType
      };

      await blobClient.UploadAsync(
        fileStream,
        new BlobUploadOptions
        {
          HttpHeaders = blobHttpHeaders
        },
        cancellationToken);

      _logger.LogInformation("File uploaded successfully: {BlobName}", blobName);
      return blobClient.Uri.ToString();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error uploading file {FileName}", fileName);
      throw;
    }
  }

  /// <summary>
  /// Download a file from blob storage
  /// </summary>
  public async Task<Stream> DownloadFileAsync(
    string blobUrl,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var blobUri = new Uri(blobUrl);
      var blobClient = new BlobClient(blobUri);

      var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
      _logger.LogInformation("File downloaded successfully from: {BlobUrl}", blobUrl);

      return response.Value.Content;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error downloading file from {BlobUrl}", blobUrl);
      throw;
    }
  }

  /// <summary>
  /// Delete a file from blob storage
  /// </summary>
  public async Task<bool> DeleteFileAsync(
    string blobUrl,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var blobUri = new Uri(blobUrl);
      var blobClient = new BlobClient(blobUri);

      var response = await blobClient.DeleteIfExistsAsync(
        DeleteSnapshotsOption.IncludeSnapshots,
        cancellationToken: cancellationToken);

      if (response.Value)
      {
        _logger.LogInformation("File deleted successfully: {BlobUrl}", blobUrl);
      }

      return response.Value;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error deleting file from {BlobUrl}", blobUrl);
      throw;
    }
  }

  /// <summary>
  /// Get file metadata without downloading
  /// </summary>
  public async Task<FileMetadata> GetFileMetadataAsync(
    string blobUrl,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var blobUri = new Uri(blobUrl);
      var blobClient = new BlobClient(blobUri);

      var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

      return new FileMetadata
      {
        FileName = blobClient.Name,
        ContentType = properties.Value.ContentType,
        SizeInBytes = properties.Value.ContentLength,
        CreatedAt = properties.Value.CreatedOn.DateTime,
        LastModified = properties.Value.LastModified.DateTime
      };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error getting metadata for {BlobUrl}", blobUrl);
      throw;
    }
  }

  /// <summary>
  /// Generate a SAS URL for temporary access
  /// </summary>
  public async Task<string> GenerateTemporaryAccessUrlAsync(
    string blobUrl,
    TimeSpan expirationTime,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var blobUri = new Uri(blobUrl);
      var blobClient = new BlobClient(blobUri);

      // Check if blob exists
      if (!await blobClient.ExistsAsync(cancellationToken))
      {
        throw new FileNotFoundException($"Blob not found: {blobUrl}");
      }

      // Generate SAS token
      var sasBuilder = new Azure.Storage.Sas.BlobSasBuilder
      {
        BlobContainerName = blobClient.BlobContainerName,
        BlobName = blobClient.Name,
        Resource = "b",
        StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
        ExpiresOn = DateTimeOffset.UtcNow.Add(expirationTime)
      };

      sasBuilder.SetPermissions(Azure.Storage.Sas.BlobSasPermissions.Read);

      var sasUri = blobClient.GenerateSasUri(sasBuilder);
      _logger.LogInformation("Generated SAS URL for {BlobUrl}, expires in {Duration}",
        blobUrl, expirationTime);

      return sasUri.ToString();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error generating SAS URL for {BlobUrl}", blobUrl);
      throw;
    }
  }
}
