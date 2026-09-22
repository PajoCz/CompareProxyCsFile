using System.Text;

namespace OteWsdlMonitor;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        try
        {
            var options = CommandLineOptions.Parse(args);
            if (options.ShowHelp)
            {
                PrintHelp();
                return 0;
            }

            var loadedConfiguration = ConfigurationLoader.Load(options.ConfigPath);
            if (options.SecretKind != SecretKind.None)
            {
                SetSecret(options.SecretKind, loadedConfiguration);
                return 0;
            }

            var settings = loadedConfiguration.Settings;
            var pageUrl = options.PageUrl ?? settings.Monitor.PageUrl;
            var recipient = options.Recipient ?? settings.Monitor.Recipient;
            var timeoutSeconds = options.TimeoutSeconds ?? settings.Monitor.RequestTimeoutSeconds;
            if (timeoutSeconds <= 0)
            {
                throw new InvalidOperationException("Monitor.RequestTimeoutSeconds must be positive.");
            }

            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(timeoutSeconds),
            };
            var pageMonitor = new OtePageMonitor(httpClient);
            var currentLinks = await pageMonitor.FetchLinksAsync(pageUrl, CancellationToken.None);
            if (currentLinks.Count == 0)
            {
                throw new InvalidOperationException("No wsdl*.zip links were found on the page.");
            }

            if (options.TestEmail)
            {
                var sentAt = StateStore.UtcNow();
                var emailSender = new EmailSender(httpClient);
                await emailSender.SendAsync(
                    settings,
                    recipient,
                    "OTE WSDL monitor test",
                    BuildEmailBody(pageUrl, currentLinks.Take(1), sentAt, testMessage: true),
                    loadedConfiguration.DirectoryPath,
                    CancellationToken.None);
                Console.WriteLine($"Test email sent to {recipient}.");
                return 0;
            }

            var dataFile = ResolvePath(
                options.DataFile ?? settings.Monitor.DataFile,
                loadedConfiguration.DirectoryPath);
            var state = StateStore.Load(dataFile, pageUrl);
            var storedUrls = state.Links
                .Select(link => link.Url)
                .ToHashSet(StringComparer.Ordinal);
            var newLinks = currentLinks
                .Where(link => !storedUrls.Contains(link))
                .ToArray();

            if (newLinks.Length == 0)
            {
                Console.WriteLine($"No new WSDL links found. Stored links: {storedUrls.Count}.");
                return 0;
            }

            var insertedAt = StateStore.UtcNow();
            if (options.NoEmail)
            {
                Console.WriteLine("Email was skipped because --no-email was specified.");
            }
            else
            {
                var emailSender = new EmailSender(httpClient);
                await emailSender.SendAsync(
                    settings,
                    recipient,
                    $"OTE: {newLinks.Length} new WSDL ZIP link(s)",
                    BuildEmailBody(pageUrl, newLinks, insertedAt, testMessage: false),
                    loadedConfiguration.DirectoryPath,
                    CancellationToken.None);
                Console.WriteLine($"Notification email sent to {recipient}.");
            }

            var updatedLinks = state.Links.ToList();
            updatedLinks.AddRange(newLinks.Select(link => new LinkRecord
            {
                Url = link,
                AddedAt = insertedAt,
            }));
            StateStore.Save(dataFile, pageUrl, updatedLinks);
            Console.WriteLine($"Added {newLinks.Length} link(s) to {dataFile}.");
            return 0;
        }
        catch (Exception error) when (error is ArgumentException or InvalidOperationException or IOException or HttpRequestException or TimeoutException)
        {
            await Console.Error.WriteLineAsync($"Error: {error.Message}");
            return 1;
        }
        catch (Exception error) when (
            error is MailKit.Security.SslHandshakeException or
            MailKit.Security.AuthenticationException or
            MailKit.Net.Smtp.SmtpCommandException or
            MailKit.Net.Smtp.SmtpProtocolException or
            MailKit.ProtocolException)
        {
            await Console.Error.WriteLineAsync($"SMTP error: {error.Message}");
            return 1;
        }
    }

    private static void SetSecret(SecretKind kind, LoadedConfiguration configuration)
    {
        var settings = configuration.Settings;
        switch (kind)
        {
            case SecretKind.AppPassword:
                SecretStore.PromptAndSave(
                    SecretStore.ResolvePath(settings.Smtp.AppPasswordFile, configuration.DirectoryPath),
                    "Gmail app password (input is hidden): ",
                    removeSpaces: true);
                break;
            case SecretKind.OAuthClientSecret:
                SecretStore.PromptAndSave(
                    SecretStore.ResolvePath(settings.Smtp.OAuth.ClientSecretFile, configuration.DirectoryPath),
                    "Google OAuth client secret (input is hidden): ",
                    removeSpaces: false);
                break;
            case SecretKind.OAuthRefreshToken:
                SecretStore.PromptAndSave(
                    SecretStore.ResolvePath(settings.Smtp.OAuth.RefreshTokenFile, configuration.DirectoryPath),
                    "Google OAuth refresh token (input is hidden): ",
                    removeSpaces: false);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind));
        }
    }

    private static string BuildEmailBody(
        string pageUrl,
        IEnumerable<string> links,
        string insertedAt,
        bool testMessage)
    {
        var builder = new StringBuilder();
        builder.AppendLine(testMessage
            ? "This is a test message from the OTE WSDL monitor."
            : "New WSDL ZIP links were found on the OTE page.");
        builder.AppendLine();
        builder.AppendLine($"Page: {pageUrl}");
        builder.AppendLine();

        foreach (var link in links)
        {
            builder.AppendLine($"File: {GetFilename(link)}");
            builder.AppendLine($"URL: {link}");
            builder.AppendLine($"Inserted at (UTC): {insertedAt}");
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static string GetFilename(string link)
    {
        if (!Uri.TryCreate(link, UriKind.Absolute, out var uri))
        {
            return link;
        }

        var path = uri.AbsolutePath.TrimEnd('/');
        var slashIndex = path.LastIndexOf('/');
        return slashIndex >= 0 ? path[(slashIndex + 1)..] : path;
    }

    private static string ResolvePath(string configuredPath, string baseDirectory)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            throw new InvalidOperationException("A file path is missing from configuration.");
        }

        return Path.GetFullPath(
            Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(baseDirectory, configuredPath));
    }

    private static void PrintHelp()
    {
        Console.WriteLine("OTE WSDL monitor (.NET)");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  OteWsdlMonitor.exe [options]");
        Console.WriteLine();
        Console.WriteLine("Monitor options:");
        Console.WriteLine("  --config PATH       Configuration file (default: appsettings.json)");
        Console.WriteLine("  --data-file PATH    State JSON path, relative to the configuration file");
        Console.WriteLine("  --page-url URL      OTE page URL override");
        Console.WriteLine("  --recipient EMAIL   Recipient override");
        Console.WriteLine("  --timeout SECONDS   HTTP timeout override");
        Console.WriteLine("  --no-email          Save new links without sending mail");
        Console.WriteLine("  --test-email        Send one test email without changing state");
        Console.WriteLine();
        Console.WriteLine("Secret setup (input is hidden and stored with Windows DPAPI):");
        Console.WriteLine("  --set-app-password");
        Console.WriteLine("  --set-oauth-client-secret");
        Console.WriteLine("  --set-oauth-refresh-token");
    }
}
