using System.Net;
using System.Net.Mail;
using Nexus.API.Core.Interfaces;
using SmtpClient = System.Net.Mail.SmtpClient;

namespace Nexus.Infrastructure.Services;

/// <summary>
/// Email service for sending notifications and alerts.
/// Implements the IEmailService interface from the Core layer.
/// </summary>
public class EmailService : IEmailService
{
  private readonly ILogger<EmailService> _logger;
  private readonly SmtpClient _smtpClient;
  private readonly string _fromEmail;
  private readonly string _fromName;

  public EmailService(
    ILogger<EmailService> logger,
    IConfiguration configuration)
  {
    _logger = logger;

    var smtpHost = configuration["Email:SmtpHost"] ?? "localhost";
    var smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
    var smtpUser = configuration["Email:SmtpUser"];
    var smtpPassword = configuration["Email:SmtpPassword"];
    var enableSsl = bool.Parse(configuration["Email:EnableSsl"] ?? "true");

    _fromEmail = configuration["Email:FromEmail"] ?? "noreply@nexus.local";
    _fromName = configuration["Email:FromName"] ?? "Nexus Knowledge Management";

    _smtpClient = new SmtpClient(smtpHost, smtpPort)
    {
      EnableSsl = enableSsl,
      UseDefaultCredentials = false,
      Credentials = new NetworkCredential(smtpUser, smtpPassword)
    };
  }

  /// <summary>
  /// Send a simple text email
  /// </summary>
  public async Task SendEmailAsync(
    string toEmail,
    string subject,
    string body,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var mailMessage = new MailMessage
      {
        From = new MailAddress(_fromEmail, _fromName),
        Subject = subject,
        Body = body,
        IsBodyHtml = false
      };

      mailMessage.To.Add(toEmail);

      await _smtpClient.SendMailAsync(mailMessage, cancellationToken);

      _logger.LogInformation("Email sent successfully to {ToEmail}: {Subject}", toEmail, subject);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error sending email to {ToEmail}", toEmail);
      throw;
    }
  }

  /// <summary>
  /// Send an HTML email
  /// </summary>
  public async Task SendHtmlEmailAsync(
    string toEmail,
    string subject,
    string htmlBody,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var mailMessage = new MailMessage
      {
        From = new MailAddress(_fromEmail, _fromName),
        Subject = subject,
        Body = htmlBody,
        IsBodyHtml = true
      };

      mailMessage.To.Add(toEmail);

      await _smtpClient.SendMailAsync(mailMessage, cancellationToken);

      _logger.LogInformation("HTML email sent successfully to {ToEmail}: {Subject}", toEmail, subject);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error sending HTML email to {ToEmail}", toEmail);
      throw;
    }
  }

  /// <summary>
  /// Send email to multiple recipients
  /// </summary>
  public async Task SendBulkEmailAsync(
    IEnumerable<string> toEmails,
    string subject,
    string body,
    bool isHtml = false,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var mailMessage = new MailMessage
      {
        From = new MailAddress(_fromEmail, _fromName),
        Subject = subject,
        Body = body,
        IsBodyHtml = isHtml
      };

      foreach (var email in toEmails)
      {
        mailMessage.To.Add(email);
      }

      await _smtpClient.SendMailAsync(mailMessage, cancellationToken);

      _logger.LogInformation("Bulk email sent successfully to {Count} recipients: {Subject}",
        toEmails.Count(), subject);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error sending bulk email");
      throw;
    }
  }

  /// <summary>
  /// Send email with template
  /// </summary>
  public async Task SendTemplatedEmailAsync(
    string toEmail,
    string templateName,
    Dictionary<string, string> templateData,
    CancellationToken cancellationToken = default)
  {
    try
    {
      // Load template (this could be from file system, database, etc.)
      var template = await LoadTemplateAsync(templateName, cancellationToken);

      // Replace placeholders
      var body = template;
      foreach (var kvp in templateData)
      {
        body = body.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
      }

      await SendHtmlEmailAsync(toEmail, templateData.GetValueOrDefault("Subject", "Notification"), body, cancellationToken);

      _logger.LogInformation("Templated email sent successfully to {ToEmail} using template {TemplateName}",
        toEmail, templateName);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error sending templated email to {ToEmail}", toEmail);
      throw;
    }
  }

  /// <summary>
  /// Send welcome email to new users
  /// </summary>
  public async Task SendWelcomeEmailAsync(
    string toEmail,
    string userName,
    CancellationToken cancellationToken = default)
  {
    var templateData = new Dictionary<string, string>
    {
      { "Subject", "Welcome to Nexus" },
      { "UserName", userName },
      { "Year", DateTime.UtcNow.Year.ToString() }
    };

    await SendTemplatedEmailAsync(toEmail, "welcome", templateData, cancellationToken);
  }

  /// <summary>
  /// Send password reset email
  /// </summary>
  public async Task SendPasswordResetEmailAsync(
    string toEmail,
    string resetToken,
    CancellationToken cancellationToken = default)
  {
    var templateData = new Dictionary<string, string>
    {
      { "Subject", "Password Reset Request" },
      { "ResetToken", resetToken },
      { "ResetUrl", $"https://nexus.local/reset-password?token={resetToken}" }
    };

    await SendTemplatedEmailAsync(toEmail, "password-reset", templateData, cancellationToken);
  }

  /// <summary>
  /// Send document shared notification
  /// </summary>
  public async Task SendDocumentSharedEmailAsync(
    string toEmail,
    string documentTitle,
    string sharedBy,
    string documentUrl,
    CancellationToken cancellationToken = default)
  {
    var templateData = new Dictionary<string, string>
    {
      { "Subject", $"{sharedBy} shared a document with you" },
      { "DocumentTitle", documentTitle },
      { "SharedBy", sharedBy },
      { "DocumentUrl", documentUrl }
    };

    await SendTemplatedEmailAsync(toEmail, "document-shared", templateData, cancellationToken);
  }

  /// <summary>
  /// Load email template from embedded resources or file system
  /// </summary>
  private async Task<string> LoadTemplateAsync(string templateName, CancellationToken cancellationToken)
  {
    // TODO: Implement template loading from file system or embedded resources
    // For now, return a simple template
    return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>{{{{Subject}}}}</title>
</head>
<body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 20px; text-align: center;'>
        <h1 style='color: white; margin: 0;'>Nexus</h1>
        <p style='color: white; margin: 5px 0 0 0;'>Where Knowledge Connects</p>
    </div>
    <div style='padding: 20px;'>
        <h2>{{{{Subject}}}}</h2>
        <p>Hello {{{{UserName}}}},</p>
        <p>This is a notification from Nexus Knowledge Management System.</p>
        <p style='margin-top: 30px; color: #666; font-size: 12px;'>
            &copy; {{{{Year}}}} Nexus. All rights reserved.
        </p>
    </div>
</body>
</html>";
  }

  public void Dispose()
  {
    _smtpClient?.Dispose();
  }
}
