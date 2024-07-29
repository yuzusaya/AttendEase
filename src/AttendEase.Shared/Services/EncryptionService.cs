using System.Security.Cryptography;
using System.Text;

namespace AttendEase.Shared.Services;

public static class EncryptionService
{
    private static byte[] DeriveKeyFromPassword(string password)
    {
        var emptySalt = Array.Empty<byte>();
        var iterations = 1000;
        var desiredKeyLength = 16; // 16 bytes equal 128 bits.
        var hashMethod = HashAlgorithmName.SHA384;
        return Rfc2898DeriveBytes.Pbkdf2(Encoding.Unicode.GetBytes(password),
                                         emptySalt,
                                         iterations,
                                         hashMethod,
                                         desiredKeyLength);
    }
    private static byte[] IV =
    {
    0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
    0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
};
    public static async Task<string> EncryptAsync(string clearText, string passphrase)
    {
        using Aes aes = Aes.Create();
        aes.Key = DeriveKeyFromPassword(passphrase);
        aes.IV = IV;

        using MemoryStream output = new();
        using CryptoStream cryptoStream = new(output, aes.CreateEncryptor(), CryptoStreamMode.Write);

        byte[] clearTextBytes = Encoding.Unicode.GetBytes(clearText);
        await cryptoStream.WriteAsync(clearTextBytes, 0, clearTextBytes.Length);
        await cryptoStream.FlushFinalBlockAsync();

        // Convert the encrypted byte array to a base64-encoded string
        return Convert.ToBase64String(output.ToArray());
    }

    public static async Task<string> DecryptAsync(string encryptedBase64, string passphrase)
    {
        using Aes aes = Aes.Create();
        aes.Key = DeriveKeyFromPassword(passphrase);
        aes.IV = IV;

        // Convert the base64-encoded string to a byte array
        byte[] encryptedBytes = Convert.FromBase64String(encryptedBase64);

        using MemoryStream input = new(encryptedBytes);
        using CryptoStream cryptoStream = new(input, aes.CreateDecryptor(), CryptoStreamMode.Read);

        using MemoryStream output = new();
        await cryptoStream.CopyToAsync(output);

        // Convert the decrypted byte array to a string
        return Encoding.Unicode.GetString(output.ToArray());
    }

}