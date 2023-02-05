using System;

namespace Sechat.Service.CustomExceptions;

public class ChatException : Exception
{
    public ChatException(string message) : base(message)
    {
    }
}
