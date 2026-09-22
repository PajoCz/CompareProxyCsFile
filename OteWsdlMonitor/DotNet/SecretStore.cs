using System.Security.Cryptography;
using System.Text;

namespace OteWsdlMonitor;

public static class SecretStore
{
    public static string ResolvePath(string configuredPath, string configurationDirectory)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            throw new InvalidOperationException("A secret file path is missing from configuration.");
        }

        return Path.GetFullPath(
            Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(configurationDirectory, configuredPath));
    }

    public static void PromptAndSave(string path, string prompt, bool removeSpaces)
    {
        EnsureWindows();
        Console.Write(prompt);
        var secret = ReadHiddenLine();
        Console.WriteLine();

        if (removeSpaces)
        {
            secret = secret.Replace(" ", string.Empty, StringComparison.Ordinal);
        }

        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException("The entered secret is empty.");
        }

        Save(path, secret);
        Console.WriteLine($"Encrypted secret saved for the current Windows user: {path}");
    }

    public static string Read(string path)
    {
        EnsureWindows();
        if (!File.Exists(path))
        {
            throw new InvalidOperationException(
                $"Encrypted secret was not found: {path}. Run the matching --set-* command first.");
        }

        try
        {
            var encrypted = File.ReadAllBytes(path);
            var clearBytes = ProtectedData.Unprotect(
                encrypted,
                optionalEntropy: null,
                scope: DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(clearBytes).Trim();
        }
        catch (CryptographicException error)
        {
            throw new InvalidOperationException(
                "The secret cannot be decrypted. Use the same Windows user account that created it.",
                error);
        }
    }

    private static void Save(string path, string secret)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var clearBytes = Encoding.UTF8.GetBytes(secret);
        try
        {
            var encrypted = ProtectedData.Protect(
                clearBytes,
                optionalEntropy: null,
                scope: DataProtectionScope.CurrentUser);
            File.WriteAllBytes(path, encrypted);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(clearBytes);
        }
    }

    private static string ReadHiddenLine()
    {
        var builder = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                break;
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (builder.Length > 0)
                {
                    builder.Length--;
                }

                continue;
            }

            if (!char.IsControl(key.KeyChar))
            {
                builder.Append(key.KeyChar);
            }
        }

        return builder.ToString();
    }

    private static void EnsureWindows()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(
                "The DPAPI secret store is available only on Windows.");
        }
    }
}
