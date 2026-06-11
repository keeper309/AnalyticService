using System;
using System.Security.Cryptography;
using System.Text;

public static class NonceUtils
{
    private const string Base36Chars = "abcdefghijklmnopqrstuvwxyz0123456789";

    public static string GenerateNonce(int length = 9)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));

        return ConvertToBase36(hash).Substring(0, length);
    }

    private static string ConvertToBase36(byte[] bytes)
    {
        // Берем 12 байт (96 бит) для избежания переполнения ulong
        ulong value = BitConverter.ToUInt64(bytes, 0) ^ BitConverter.ToUInt64(bytes, 8);
        string result = "";

        do
        {
            result = Base36Chars[(int)(value % 36)] + result;
            value /= 36;
        }
        while (value > 0);

        return result.PadLeft(9, Base36Chars[0]); // Выравниваем длину
    }
}
