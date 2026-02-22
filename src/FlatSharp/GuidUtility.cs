using System.Security.Cryptography;
using System.Text;

namespace FlatSharp;

public static class GuidUtility
{
    public static string CreateMD5Guid(string? input)
    {
        using MD5 md5 = MD5.Create();

        byte[] inputBytes = Encoding.ASCII.GetBytes(input ?? string.Empty);
        byte[] hashBytes = md5.ComputeHash(inputBytes);

        return Convert.ToHexString(hashBytes);
    }
}