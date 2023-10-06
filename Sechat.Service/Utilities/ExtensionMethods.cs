using System.IO;

namespace Sechat.Service.Utilities;

public static class ExtensionMethods
{
    public static byte[] ToByteArray(this Stream stream)
    {
        if (stream is MemoryStream result)
        {
            return result.ToArray();
        }
        else
        {
            using MemoryStream ms = new();
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
