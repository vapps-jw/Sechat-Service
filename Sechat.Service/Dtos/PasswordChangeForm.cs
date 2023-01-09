namespace Sechat.Service.Dtos;

public class PasswordChangeForm
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
