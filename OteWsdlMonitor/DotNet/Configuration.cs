using Microsoft.Extensions.Configuration;

namespace OteWsdlMonitor;

public sealed class AppSettings
{
    public MonitorSettings Monitor { get; set; } = new();
    public SmtpSettings Smtp { get; set; } = new();
}

public sealed class MonitorSettings
{
    public string PageUrl { get; set; } = string.Empty;
    public string DataFile { get; set; } = "wsdl_links.json";
    public string Recipient { get; set; } = string.Empty;
    public int RequestTimeoutSeconds { get; set; } = 30;
}

public sealed class SmtpSettings
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string Security { get; set; } = "StartTls";
    public string Authentication { get; set; } = "AppPassword";
    public string Username { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string? AppPassword { get; set; }
    public string AppPasswordFile { get; set; } = "secrets\\smtp-app-password.dpapi";
    public OAuthSettings OAuth { get; set; } = new();
}

public sealed class OAuthSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string? ClientSecret { get; set; }
    public string? RefreshToken { get; set; }
    public string ClientSecretFile { get; set; } = "secrets\\gmail-oauth-client-secret.dpapi";
    public string RefreshTokenFile { get; set; } = "secrets\\gmail-oauth-refresh-token.dpapi";
    public string AccessTokenEnvironmentVariable { get; set; } = "OTE_GMAIL_ACCESS_TOKEN";
    public string TokenEndpoint { get; set; } = "https://oauth2.googleapis.com/token";
    public string Scope { get; set; } = "https://mail.google.com/";
}

public sealed record LoadedConfiguration(AppSettings Settings, string FilePath)
{
    public string DirectoryPath => Path.GetDirectoryName(FilePath) ?? Directory.GetCurrentDirectory();
}

public static class ConfigurationLoader
{
    public static LoadedConfiguration Load(string? requestedPath)
    {
        var filePath = ResolveConfigurationPath(requestedPath);
        if (!File.Exists(filePath))
        {
            throw new InvalidOperationException($"Configuration file was not found: {filePath}");
        }

        try
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(filePath) ?? Directory.GetCurrentDirectory())
                .AddJsonFile(Path.GetFileName(filePath), optional: false, reloadOnChange: false)
                .AddUserSecrets(typeof(Program).Assembly, optional: true)
                .Build();
            var settings = new AppSettings();
            configuration.Bind(settings);
            return new LoadedConfiguration(settings, filePath);
        }
        catch (FormatException error)
        {
            throw new InvalidOperationException($"Configuration file is not valid JSON: {filePath}", error);
        }
    }

    private static string ResolveConfigurationPath(string? requestedPath)
    {
        if (!string.IsNullOrWhiteSpace(requestedPath))
        {
            return Path.GetFullPath(requestedPath);
        }

        var currentDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        if (File.Exists(currentDirectoryPath))
        {
            return Path.GetFullPath(currentDirectoryPath);
        }

        return Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    }
}

public enum SecretKind
{
    None,
    AppPassword,
    OAuthClientSecret,
    OAuthRefreshToken,
}

public sealed class CommandLineOptions
{
    public string? ConfigPath { get; private set; }
    public string? DataFile { get; private set; }
    public string? PageUrl { get; private set; }
    public string? Recipient { get; private set; }
    public int? TimeoutSeconds { get; private set; }
    public bool NoEmail { get; private set; }
    public bool TestEmail { get; private set; }
    public bool ShowHelp { get; private set; }
    public SecretKind SecretKind { get; private set; }

    public static CommandLineOptions Parse(string[] args)
    {
        var options = new CommandLineOptions();

        for (var index = 0; index < args.Length; index++)
        {
            switch (args[index].ToLowerInvariant())
            {
                case "--help":
                case "-h":
                    options.ShowHelp = true;
                    break;
                case "--config":
                    options.ConfigPath = ReadValue(args, ref index, "--config");
                    break;
                case "--data-file":
                    options.DataFile = ReadValue(args, ref index, "--data-file");
                    break;
                case "--page-url":
                    options.PageUrl = ReadValue(args, ref index, "--page-url");
                    break;
                case "--recipient":
                    options.Recipient = ReadValue(args, ref index, "--recipient");
                    break;
                case "--timeout":
                    var timeout = ReadValue(args, ref index, "--timeout");
                    if (!int.TryParse(timeout, out var timeoutSeconds) || timeoutSeconds <= 0)
                    {
                        throw new ArgumentException("--timeout must be a positive integer.");
                    }

                    options.TimeoutSeconds = timeoutSeconds;
                    break;
                case "--no-email":
                    options.NoEmail = true;
                    break;
                case "--test-email":
                    options.TestEmail = true;
                    break;
                case "--set-app-password":
                    SetSecretKind(options, SecretKind.AppPassword);
                    break;
                case "--set-oauth-client-secret":
                    SetSecretKind(options, SecretKind.OAuthClientSecret);
                    break;
                case "--set-oauth-refresh-token":
                    SetSecretKind(options, SecretKind.OAuthRefreshToken);
                    break;
                default:
                    throw new ArgumentException($"Unknown argument: {args[index]}");
            }
        }

        if (options.NoEmail && options.TestEmail)
        {
            throw new ArgumentException("--no-email and --test-email cannot be used together.");
        }

        if (options.SecretKind != SecretKind.None &&
            (options.NoEmail || options.TestEmail || options.DataFile is not null || options.PageUrl is not null || options.Recipient is not null))
        {
            throw new ArgumentException("Secret setup options cannot be combined with monitor options.");
        }

        return options;
    }

    private static string ReadValue(string[] args, ref int index, string optionName)
    {
        if (++index >= args.Length || string.IsNullOrWhiteSpace(args[index]))
        {
            throw new ArgumentException($"{optionName} requires a value.");
        }

        return args[index];
    }

    private static void SetSecretKind(CommandLineOptions options, SecretKind kind)
    {
        if (options.SecretKind != SecretKind.None && options.SecretKind != kind)
        {
            throw new ArgumentException("Only one secret setup option can be used at a time.");
        }

        options.SecretKind = kind;
    }
}
