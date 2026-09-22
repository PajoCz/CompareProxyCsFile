using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace OteWsdlMonitor;

public sealed class OtePageMonitor
{
    private static readonly Regex HrefPattern = new(
        @"<a\b[^>]*?\bhref\s*=\s*(?:""(?<href>[^""]*)""|'(?<href>[^']*)'|(?<href>[^\s>]+))",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex WsdlFilenamePattern = new(
        @"^wsdl.*\.zip$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private readonly HttpClient httpClient;

    public OtePageMonitor(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<IReadOnlyList<string>> FetchLinksAsync(
        string pageUrl,
        CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(pageUrl, UriKind.Absolute, out var pageUri) ||
            (pageUri.Scheme != Uri.UriSchemeHttp && pageUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException($"Invalid page URL: {pageUrl}");
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, pageUri);
        request.Headers.UserAgent.ParseAdd("ote-wsdl-monitor-dotnet/1.0");
        using var response = await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseContentRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        var page = await response.Content.ReadAsStringAsync(cancellationToken);
        var links = new HashSet<string>(StringComparer.Ordinal);

        foreach (Match match in HrefPattern.Matches(page))
        {
            var href = WebUtility.HtmlDecode(match.Groups["href"].Value).Trim();
            if (string.IsNullOrWhiteSpace(href) ||
                !Uri.TryCreate(pageUri, href, out var absoluteUri) ||
                absoluteUri is null ||
                (absoluteUri.Scheme != Uri.UriSchemeHttp && absoluteUri.Scheme != Uri.UriSchemeHttps))
            {
                continue;
            }

            var cleanUri = new UriBuilder(absoluteUri) { Fragment = string.Empty }.Uri;
            var path = cleanUri.AbsolutePath.TrimEnd('/');
            var slashIndex = path.LastIndexOf('/');
            var filename = slashIndex >= 0 ? path[(slashIndex + 1)..] : path;

            try
            {
                filename = Uri.UnescapeDataString(filename);
            }
            catch (UriFormatException)
            {
                continue;
            }

            if (WsdlFilenamePattern.IsMatch(filename))
            {
                links.Add(cleanUri.AbsoluteUri);
            }
        }

        return links.OrderBy(link => link, StringComparer.OrdinalIgnoreCase).ToArray();
    }
}

public sealed class MonitorState
{
    public string PageUrl { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
    public List<LinkRecord> Links { get; set; } = [];
}

public sealed class LinkRecord
{
    public string Url { get; set; } = string.Empty;
    public string AddedAt { get; set; } = string.Empty;
}

public static class StateStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static MonitorState Load(string path, string pageUrl)
    {
        if (!File.Exists(path))
        {
            return new MonitorState { PageUrl = pageUrl };
        }

        try
        {
            var state = JsonSerializer.Deserialize<MonitorState>(File.ReadAllText(path), JsonOptions)
                ?? throw new InvalidOperationException("State file is empty.");
            if (state.Links.Any(link => link is null || string.IsNullOrWhiteSpace(link.Url)))
            {
                throw new InvalidOperationException("State file contains an invalid link record.");
            }

            return state;
        }
        catch (JsonException error)
        {
            throw new InvalidOperationException($"State file is not valid JSON: {path}", error);
        }
    }

    public static void Save(string path, string pageUrl, IEnumerable<LinkRecord> links)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var state = new MonitorState
        {
            PageUrl = pageUrl,
            UpdatedAt = UtcNow(),
            Links = links.ToList(),
        };
        var temporaryPath = $"{path}.{Guid.NewGuid():N}.tmp";

        try
        {
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(state, JsonOptions));
            File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    public static string UtcNow() =>
        DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss'Z'");
}
