namespace Sechat.Service.Settings;

public class EmailSenderSettings
{
    public string ApiKey { get; set; }
    public string From { get; set; }
    public string ResetPasswordTemplate { get; set; }
    public string ConfirmEmailTemplate { get; set; }
    public string AdminNotificationEmailTemplate { get; set; }
}
