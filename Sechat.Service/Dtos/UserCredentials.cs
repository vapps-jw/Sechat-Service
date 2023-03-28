using FluentValidation;
using Sechat.Service.Utilities;

namespace Sechat.Service.Dtos;

public class UserCredentials
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class UserCredentialsFormValidation : AbstractValidator<UserCredentials>
{
    public UserCredentialsFormValidation()
    {
        _ = RuleFor(x => x.Username).NotEmpty().MaximumLength(AppConstants.StringLengths.UserNameMax);
        _ = RuleFor(x => x.Password).NotEmpty().MinimumLength(AppConstants.StringLengths.PasswordMin).MaximumLength(AppConstants.StringLengths.PasswordMax);
    }
}
