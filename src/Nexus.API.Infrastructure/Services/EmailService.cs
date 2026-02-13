using System.Net;
using System.Net.Mail;
using Nexus.API.Core.Interfaces;
using SmtpClient = System.Net.Mail.SmtpClient;

namespace Nexus.API.Infrastructure.Services;

/// <summary>
/// Email service for sending notifications and alerts.
/// Implements the IEmailService interface from the Core layer.
/// </summary>
public class EmailService : IEmailService
{
  private readonly ILogger<EmailService> _logger;
  private readonly IConfiguration _configuration;
  private readonly SmtpClient _smtpClient;
  private readonly string _fromEmail;
  private readonly string _fromName;

  public EmailService(
    ILogger<EmailService> logger,
    IConfiguration configuration)
  {
    _logger = logger;

    _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

    var smtpHost = _configuration["Email:SmtpHost"] ?? "localhost";
    var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
    var smtpUser = _configuration["Email:SmtpUser"];
    var smtpPassword = _configuration["Email:SmtpPassword"];
    var enableSsl = bool.Parse(_configuration["Email:EnableSsl"] ?? "true");

    _fromEmail = _configuration["Email:FromEmail"] ?? "noreply@nexus.local";
    _fromName = _configuration["Email:FromName"] ?? "Nexus Knowledge Management";

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
        string htmlBody,
        CancellationToken cancellationToken = default)
  {
    // TODO: In production, replace this with actual email sending
    // Examples:
    // - SendGrid: https://sendgrid.com/
    // - AWS SES: https://aws.amazon.com/ses/
    // - MailKit: https://github.com/jstedfast/MailKit
    // - Azure Communication Services: https://azure.microsoft.com/en-us/services/communication-services/

    _logger.LogInformation(
        "EMAIL SENT (Development Mode)\n" +
        "To: {ToEmail}\n" +
        "Subject: {Subject}\n" +
        "Body:\n{Body}",
        toEmail,
        subject,
        htmlBody);

    // Simulate async operation
    await Task.CompletedTask;
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
        string userName,
        string resetToken,
        CancellationToken cancellationToken = default)
  {
    // Build the reset URL
    var baseUrl = _configuration["App:BaseUrl"] ?? "http://localhost:5000";
    var resetUrl = $"{baseUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}";

    var subject = "Reset Your Password";
    var htmlBody = $@"
          <!DOCTYPE html>
          <html>
          <head>
              <meta charset='utf-8'>
              <title>Reset Your Password</title>
          </head>
          <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
              <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                  <h2 style='color: #2c3e50;'>Reset Your Password</h2>
                  <p>Hello {userName},</p>
                  <p>You recently requested to reset your password. Click the button below to reset it:</p>
                  <div style='text-align: center; margin: 30px 0;'>
                      <a href='{resetUrl}' 
                        style='background-color: #3498db; color: white; padding: 12px 30px; 
                                text-decoration: none; border-radius: 5px; display: inline-block;'>
                          Reset Password
                      </a>
                  </div>
                  <p>Or copy and paste this link into your browser:</p>
                  <p style='background-color: #f8f9fa; padding: 10px; border-radius: 5px; word-break: break-all;'>
                      {resetUrl}
                  </p>
                  <p style='color: #7f8c8d; font-size: 14px; margin-top: 30px;'>
                      This link will expire in 1 hour.<br>
                      If you didn't request a password reset, you can safely ignore this email.
                  </p>
                  <hr style='border: 0; border-top: 1px solid #eee; margin: 30px 0;'>
                  <p style='color: #7f8c8d; font-size: 12px;'>
                      This is an automated message from NEXUS Knowledge Management System.
                  </p>
              </div>
          </body>
          </html>";

    await SendEmailAsync(toEmail, subject, htmlBody, cancellationToken);
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
