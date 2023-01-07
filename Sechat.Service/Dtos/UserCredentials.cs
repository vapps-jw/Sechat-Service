using FluentValidation;

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
        _ = RuleFor(x => x.Username).NotEmpty().MinimumLength(5).MaximumLength(12);
        _ = RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(30);
    }
}
