using System;

namespace Sechat.Service.Configuration;

public class ChatException : Exception
{
    public ChatException(string message) : base(message)
    {
    }
}
