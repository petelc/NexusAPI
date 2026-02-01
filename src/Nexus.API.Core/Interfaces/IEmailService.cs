
namespace Nexus.API.Core.Interfaces;
/// <summary>
/// Interface for email operations
/// </summary>
public interface IEmailService : IDisposable
{
    Task SendEmailAsync(
      string toEmail,
      string subject,
      string body,
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

    Task SendWelcomeEmailAsync(
      string toEmail,
      string userName,
      CancellationToken cancellationToken = default);

    Task SendPasswordResetEmailAsync(
      string toEmail,
      string resetToken,
      CancellationToken cancellationToken = default);

    Task SendDocumentSharedEmailAsync(
      string toEmail,
      string documentTitle,
      string sharedBy,
      string documentUrl,
      CancellationToken cancellationToken = default);
}