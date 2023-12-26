using System.Security.Cryptography;
using System.Text;

namespace Chimp;

public static class ProtectedFileHelper
{
    private static readonly byte[] AppSpecificEntropy = "AAMLMY1mdIujFEvUscsl"u8.ToArray();

    /// <summary>
    /// If on Windows OS, DPAPI is used to encrypt the data.
    /// Otherwise, we rely on OS filesystem protection (chmod).
    /// </summary>
    public static string ReadProtectedFile(string filePath)
    {
        var bytes = File.ReadAllBytes(filePath);
        
        if (OperatingSystem.IsWindows())
        {
            bytes = ProtectedData.Unprotect(bytes, AppSpecificEntropy, DataProtectionScope.CurrentUser);
        }
        
        return Encoding.Default.GetString(bytes);
    }
    
    /// <summary>
    /// If on Windows OS, DPAPI is used to encrypt the data.
    /// Otherwise, we rely on OS filesystem protection (chmod).
    /// </summary>
    public static void WriteProtectedFile(string filePath, string data)
    {
        var bytes = Encoding.Default.GetBytes(data);

        if (OperatingSystem.IsWindows())
        {
            bytes = ProtectedData.Protect(bytes, AppSpecificEntropy, DataProtectionScope.CurrentUser);
        }

        File.WriteAllBytes(filePath, bytes);

        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode(filePath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }
}
