using System;

namespace Sechat.Domain.CustomExceptions;

public class ChatException : Exception
{
    public ChatException(string message) : base(message)
    {
    }
}
