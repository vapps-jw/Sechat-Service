using FluentValidation;
using Sechat.Service.Configuration;

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
        _ = RuleFor(x => x.Username).NotEmpty().MaximumLength(AppConstants.StringLength.UserNameMax);
        _ = RuleFor(x => x.Password).NotEmpty().MaximumLength(AppConstants.StringLength.PasswordMax);
    }
}
