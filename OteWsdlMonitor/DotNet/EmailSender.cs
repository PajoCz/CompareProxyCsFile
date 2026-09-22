using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace OteWsdlMonitor;

public sealed class EmailSender
{
    private readonly HttpClient httpClient;

    public EmailSender(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task SendAsync(
        AppSettings settings,
        string recipient,
        string subject,
        string body,
        string configurationDirectory,
        CancellationToken cancellationToken)
    {
        var smtp = settings.Smtp ?? throw new InvalidOperationException("SMTP configuration is missing.");
        ValidateSmtp(smtp);

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(smtp.From));
        message.To.Add(MailboxAddress.Parse(recipient));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(
            smtp.Host,
            smtp.Port,
            GetSecurityOption(smtp.Security),
            cancellationToken);

        try
        {
            if (string.Equals(smtp.Authentication, "AppPassword", StringComparison.OrdinalIgnoreCase))
            {
                var password = smtp.AppPassword;
                if (string.IsNullOrWhiteSpace(password))
                {
                    var passwordPath = SecretStore.ResolvePath(smtp.AppPasswordFile, configurationDirectory);
                    password = SecretStore.Read(passwordPath);
                }

                await client.AuthenticateAsync(smtp.Username, password, cancellationToken);
            }
            else
            {
                var accessToken = await GetAccessTokenAsync(smtp, configurationDirectory, cancellationToken);
                var oauth2 = new SaslMechanismOAuth2(smtp.Username, accessToken);
                await client.AuthenticateAsync(oauth2, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
        }
        finally
        {
            if (client.IsConnected)
            {
                await client.DisconnectAsync(quit: true, cancellationToken);
            }
        }
    }

    private async Task<string> GetAccessTokenAsync(
        SmtpSettings smtp,
        string configurationDirectory,
        CancellationToken cancellationToken)
    {
        var oauth = smtp.OAuth ?? throw new InvalidOperationException("OAuth configuration is missing.");
        if (!string.IsNullOrWhiteSpace(oauth.AccessTokenEnvironmentVariable))
        {
            var environmentToken = Environment.GetEnvironmentVariable(oauth.AccessTokenEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(environmentToken))
            {
                return environmentToken.Trim();
            }
        }

        if (string.IsNullOrWhiteSpace(oauth.ClientId))
        {
            throw new InvalidOperationException("OAuth.ClientId is missing from appsettings.json.");
        }

        var clientSecretPath = SecretStore.ResolvePath(oauth.ClientSecretFile, configurationDirectory);
        var refreshTokenPath = SecretStore.ResolvePath(oauth.RefreshTokenFile, configurationDirectory);
        var clientSecret = string.IsNullOrWhiteSpace(oauth.ClientSecret)
            ? SecretStore.Read(clientSecretPath)
            : oauth.ClientSecret;
        var refreshToken = string.IsNullOrWhiteSpace(oauth.RefreshToken)
            ? SecretStore.Read(refreshTokenPath)
            : oauth.RefreshToken;

        using var request = new HttpRequestMessage(HttpMethod.Post, oauth.TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = oauth.ClientId,
                ["client_secret"] = clientSecret,
                ["refresh_token"] = refreshToken,
                ["grant_type"] = "refresh_token",
            }),
        };
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Google OAuth token request failed with HTTP {(int)response.StatusCode}.");
        }

        try
        {
            var tokenResponse = await response.Content.ReadFromJsonAsync<OAuthTokenResponse>(cancellationToken);
            if (string.IsNullOrWhiteSpace(tokenResponse?.AccessToken))
            {
                throw new InvalidOperationException("Google OAuth response did not contain an access token.");
            }

            return tokenResponse.AccessToken;
        }
        catch (JsonException error)
        {
            throw new InvalidOperationException("Google OAuth response was not valid JSON.", error);
        }
    }

    private static void ValidateSmtp(SmtpSettings smtp)
    {
        if (string.IsNullOrWhiteSpace(smtp.Host) || smtp.Port is < 1 or > 65535)
        {
            throw new InvalidOperationException("SMTP host and port are invalid.");
        }

        if (string.IsNullOrWhiteSpace(smtp.Username) || string.IsNullOrWhiteSpace(smtp.From))
        {
            throw new InvalidOperationException("SMTP Username and From are required.");
        }

        if (!string.Equals(smtp.Authentication, "AppPassword", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(smtp.Authentication, "OAuth2", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("SMTP.Authentication must be AppPassword or OAuth2.");
        }
    }

    private static SecureSocketOptions GetSecurityOption(string security)
    {
        if (string.Equals(security, "StartTls", StringComparison.OrdinalIgnoreCase))
        {
            return SecureSocketOptions.StartTls;
        }

        if (string.Equals(security, "SslOnConnect", StringComparison.OrdinalIgnoreCase))
        {
            return SecureSocketOptions.SslOnConnect;
        }

        throw new InvalidOperationException(
            "SMTP.Security must be StartTls or SslOnConnect; unencrypted SMTP is not allowed.");
    }

    private sealed class OAuthTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }
}
