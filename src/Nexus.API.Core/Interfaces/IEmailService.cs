
namespace Nexus.API.Core.Interfaces;
/// <summary>
/// Interface for email operations
/// </summary>
public interface IEmailService : IDisposable
{
  /// <summary>
  /// Sends a generic email.
  /// </summary>
  Task SendEmailAsync(
      string toEmail,
      string subject,
      string htmlBody,
      CancellationToken cancellationToken = default);

  Task SendHtmlEmailAsync(
    string toEmail,
    string subject,
    string htmlBody,
    CancellationToken cancellationToken = default);

  Task SendBulkEmailAsync(
    IEnumerable<string> toEmails,
    string subject,
    string body,
    bool isHtml = false,
    CancellationToken cancellationToken = default);

  Task SendTemplatedEmailAsync(
    string toEmail,
    string templateName,
    Dictionary<string, string> templateData,
    CancellationToken cancellationToken = default);

  /// <summary>
  /// Sends a password reset email with the reset link.
  /// </summary>
  Task SendPasswordResetEmailAsync(
      string toEmail,
      string userName,
      string resetToken,
      CancellationToken cancellationToken = default);

  Task SendDocumentSharedEmailAsync(
    string toEmail,
    string documentTitle,
    string sharedBy,
    string documentUrl,
    CancellationToken cancellationToken = default);
}