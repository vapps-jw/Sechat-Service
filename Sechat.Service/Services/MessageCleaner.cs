using Microsoft.Extensions.Logging;
using Sechat.Data.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sechat.Service.Services;

public class MessageCleaner
{
    private readonly ILogger<MessageCleaner> _logger;
    private readonly ChatRepository _chatRepository;

    public MessageCleaner(ILogger<MessageCleaner> logger, ChatRepository chatRepository)
    {
        _logger = logger;
        _chatRepository = chatRepository;
    }

    public async Task DoWork(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation($"Deleting messages ...");
            await Task.Delay(1000 * 2, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError("Message cleaner exception", ex);
        }
    }
}
