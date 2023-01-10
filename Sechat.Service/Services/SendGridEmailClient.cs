
using FluentValidation.Validators;
using Microsoft.Extensions.Options;
using Sechat.Service.Settings;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace Sechat.Service.Services;

public class SendGridEmailClient : IEmailClient
{
    private readonly IOptionsMonitor<EmailSenderSettings> _optionsMonitor;
    private readonly SendGridClient _client;

    public SendGridEmailClient(IOptionsMonitor<EmailSenderSettings> optionsMonitor)
    {
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
}
