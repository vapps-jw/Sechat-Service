using Microsoft.Extensions.Options;
using Sechat.Service.Settings;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace Sechat.Service.Services;

public class SendGridEmailClient : IEmailClient
{
    private readonly IOptions<SechatEmails> _sechatEmails;
    private readonly IOptionsMonitor<EmailSenderSettings> _optionsMonitor;
    private readonly SendGridClient _client;

    public record ExceptionData(string Message, string StackTrace);

    public SendGridEmailClient(
        IOptions<SechatEmails> sechatEmails,
        IOptionsMonitor<EmailSenderSettings> optionsMonitor)
    {
        _sechatEmails = sechatEmails;
        _optionsMonitor = optionsMonitor;
        _client = new SendGridClient(_optionsMonitor.CurrentValue.ApiKey);
    }

    public Task<Response> SendPasswordResetAsync(string recipient, string url)
    {
        var msg = MailHelper.CreateSingleTemplateEmail(
            new EmailAddress(_optionsMonitor.CurrentValue.From),
            new EmailAddress(recipient),
            _optionsMonitor.CurrentValue.ResetPasswordTemplate,
            new { url }
        );

        return _client.SendEmailAsync(msg);
    }

    public Task<Response> SendEmailConfirmationAsync(string recipient, string url)
    {
        var msg = MailHelper.CreateSingleTemplateEmail(
            new EmailAddress(_optionsMonitor.CurrentValue.From),
            new EmailAddress(recipient),
               _optionsMonitor.CurrentValue.ConfirmEmailTemplate,
            new { url }
        );

        return _client.SendEmailAsync(msg);
    }

    public Task<Response> SendExceptionNotificationAsync(Exception ex)
    {
        var msg = MailHelper.CreateSingleTemplateEmail(
            new EmailAddress(_sechatEmails.Value.System),
            new EmailAddress(_optionsMonitor.CurrentValue.From),
               _optionsMonitor.CurrentValue.AdminNotificationEmailTemplate,
            new ExceptionData(ex.Message, ex.StackTrace)
        );

        return _client.SendEmailAsync(msg);
    }
}
