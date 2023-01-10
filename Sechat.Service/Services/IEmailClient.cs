using SendGrid;
using System.Threading.Tasks;

namespace Sechat.Service.Services;
public interface IEmailClient
{
    Task<Response> SendEmailConfirmationAsync(string recipient, string url);
    Task<Response> SendPasswordResetAsync(string recipient, string url);
}