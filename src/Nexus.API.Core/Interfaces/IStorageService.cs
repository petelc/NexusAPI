using Nexus.API.Core.Models;

namespace Nexus.API.Core.Interfaces;

/// <summary>
/// Interface for file storage operations (Azure Blob Storage, AWS S3, etc.)
/// </summary>
public interface IStorageService
{
  Task<string> UploadFileAsync(
    string fileName,
    Stream fileStream,
    string contentType,
    CancellationToken cancellationToken = default);

  Task<Stream> DownloadFileAsync(
    string blobUrl,
    CancellationToken cancellationToken = default);

  Task<bool> DeleteFileAsync(
    string blobUrl,
    CancellationToken cancellationToken = default);

  Task<FileMetadata> GetFileMetadataAsync(
    string blobUrl,
    CancellationToken cancellationToken = default);

  Task<string> GenerateTemporaryAccessUrlAsync(
    string blobUrl,
    TimeSpan expirationTime,
    CancellationToken cancellationToken = default);
}