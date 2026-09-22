# OTE WSDL monitor - .NET

This is a new, separate .NET console application. The existing Python monitor
and the original `CompareProxyCsFile` application are unchanged.

The application downloads the OTE documentation page, finds links whose file
name matches `wsdl*.zip`, stores first-seen links in `wsdl_links.json`, and
emails only links that were not stored by a previous successful run.

## Requirements

- Windows
- .NET 8 SDK or runtime
- A Gmail account
- MailKit is used for SMTP and XOAUTH2

## Configuration

Edit `appsettings.json`. It contains non-secret values only:

```json
{
  "Monitor": {
    "Recipient": "your-recipient@gmail.com"
  },
  "Smtp": {
    "Authentication": "AppPassword",
    "Username": "your-account@gmail.com",
    "From": "your-account@gmail.com"
  }
}
```

For Gmail use:

- `Host`: `smtp.gmail.com`
- `Port`: `587`
- `Security`: `StartTls`
- `From`: the same Gmail address as `Username`

Unencrypted SMTP is rejected. Port 465 can be used with `Security` set to
`SslOnConnect`.

## Gmail app password

1. Enable 2-Step Verification in the Google account.
2. Open <https://myaccount.google.com/apppasswords>.
3. Create an app password, for example `OTE WSDL Monitor`.
4. In PowerShell, from this directory, run:

   ```powershell
   .\run-ote-wsdl-monitor-dotnet.bat --set-app-password
   ```

5. Enter the 16-character app password when prompted. The input is hidden.

The application encrypts the value with Windows DPAPI for the current Windows
user and stores it at the configured `Smtp.AppPasswordFile`, normally under
`secrets\\smtp-app-password.dpapi`. The password is never written to
`appsettings.json`, source code, the command line, or email state.

The encrypted file is tied to the Windows user who created it. A scheduled task
must run as that same user (and have access to that user profile). The file is
already ignored by the repository rules, but it should still never be uploaded
to GitHub.

### Optional User Secrets for local development

The application also loads .NET User Secrets after `appsettings.json`, so a
value in the user profile overrides the JSON configuration. From this project
directory, set the app password with:

```powershell
dotnet user-secrets set "Smtp:AppPassword" "your-16-character-app-password"
```

User Secrets are stored under
`%APPDATA%\\Microsoft\\UserSecrets\\CompareProxyCsFile.OteWsdlMonitor\\secrets.json`.
The file is outside the repository, but it is not a DPAPI-encrypted vault; any
process running as the same Windows user can read it. Use this option for local
development or a personal task only. DPAPI is preferred for a scheduled task
that should keep the secret encrypted at rest.

The same override pattern is available for OAuth:

```powershell
dotnet user-secrets set "Smtp:OAuth:ClientSecret" "your-client-secret"
dotnet user-secrets set "Smtp:OAuth:RefreshToken" "your-refresh-token"
```

Do not put these values in `appsettings.json` or commit `secrets.json`.

Set this in `appsettings.json`:

```json
"Authentication": "AppPassword"
```

Then test the SMTP connection without changing the state file:

```powershell
.\run-ote-wsdl-monitor-dotnet.bat --test-email
```

## Gmail OAuth 2.0

OAuth mode uses Gmail SMTP XOAUTH2. It exchanges a long-lived refresh token
for a short-lived access token at `https://oauth2.googleapis.com/token`.
The access token is used only in memory and is not written to disk.

1. In Google Cloud Console, create or select a project.
2. Configure the OAuth consent screen.
3. Create an OAuth client ID of type **Desktop app**.
4. Put the client ID in `Smtp.OAuth.ClientId` in `appsettings.json`.
5. Keep the client secret outside the repository. Store it with:

   ```powershell
   .\run-ote-wsdl-monitor-dotnet.bat --set-oauth-client-secret
   ```

6. Obtain a refresh token for the Gmail scope `https://mail.google.com/`.
   Google OAuth Playground can be used when configured with your own OAuth
   client credentials: <https://developers.google.com/oauthplayground>.
7. Store the refresh token without displaying it in chat or putting it in a
   file tracked by Git:

   ```powershell
   .\run-ote-wsdl-monitor-dotnet.bat --set-oauth-refresh-token
   ```

8. Change the authentication mode:

   ```json
   "Authentication": "OAuth2"
   ```

The OAuth client secret and refresh token are encrypted with Windows DPAPI and
are usable only by the Windows user who stored them. Do not paste either value
into `appsettings.json`, a `.bat` file, GitHub, or the chat.

For a temporary access token instead of the refresh-token files, set the
configured environment variable `OTE_GMAIL_ACCESS_TOKEN`. This is less suitable
for a daily task because access tokens expire.

## Run and test

From this directory:

```powershell
.\run-ote-wsdl-monitor-dotnet.bat --help
.\run-ote-wsdl-monitor-dotnet.bat --no-email --data-file .\wsdl_links.test.json
.\run-ote-wsdl-monitor-dotnet.bat --test-email
```

The first `--no-email` run stores the current links. A second run with the same
test file should report no new links. The test file is ignored by Git.

To build a release executable:

```powershell
dotnet publish .\OteWsdlMonitor.csproj -c Release --self-contained false
```

The launcher uses the release DLL when it exists; otherwise it uses
`dotnet run`.

## Task Scheduler

Create a daily task that starts
`run-ote-wsdl-monitor-dotnet.bat`. Configure it to run under the same Windows
user that created the DPAPI secrets. Environment variables are not needed for
the normal AppPassword or OAuth refresh-token setup.
