namespace Sechat.Data;
public enum KeyType
{
    EmailUpdate = 0,
    PasswordReset = 1,
    UserDirectMessageKey = 2,
}
public enum VideoCallType
{
    Incoming = 0,
    Outgoing = 1,
}
public enum VideoCallResult
{
    Answered = 0,
    Rejected = 1,
    Unanswered = 2,
}
