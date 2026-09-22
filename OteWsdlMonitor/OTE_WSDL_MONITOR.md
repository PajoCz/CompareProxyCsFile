# OTE WSDL monitor

`ote_wsdl_monitor.py` downloads the OTE documentation page, finds links whose
file name matches `wsdl*.zip`, and stores each first-seen link in
`wsdl_links.json` with its insertion time in UTC. A normal run sends one email
containing all links found since the previous successful run.

The script uses only the Python standard library. Python 3.13 is sufficient.

## SMTP setup

Configure an SMTP server in the process environment. For example, for an
authenticated Microsoft 365 SMTP submission:

```powershell
$env:OTE_SMTP_HOST = "smtp.office365.com"
$env:OTE_SMTP_PORT = "587"
$env:OTE_SMTP_USERNAME = "your-mailbox@example.com"
$env:OTE_SMTP_PASSWORD = "set-this-without-putting-it-in-the-file"
$env:OTE_SMTP_FROM = $env:OTE_SMTP_USERNAME
$env:OTE_SMTP_STARTTLS = "1"
```

The password is never written to `wsdl_links.json`. The SMTP account must be
allowed to send mail through the selected server; Microsoft 365 tenants may
require SMTP AUTH to be enabled or a different relay method.

## Windows launcher

`run-ote-wsdl-monitor.bat` starts the monitor from its own directory and
contains the non-secret SMTP configuration. Edit the SMTP and recipient values
near the top of that file. Set `OTE_SMTP_PASSWORD` as a Windows user or task
account environment variable; do not put the password in the BAT file.

The launcher passes any additional arguments to the Python script:

```powershell
.\run-ote-wsdl-monitor.bat
.\run-ote-wsdl-monitor.bat --no-email --data-file .\wsdl_links.test.json
.\run-ote-wsdl-monitor.bat --test-email
```

## Run

From the directory containing the script:

```powershell
python .\ote_wsdl_monitor.py
```

The first successful run reports every WSDL ZIP link currently on the page.
Later runs report and email only links not already present in the JSON file.
If SMTP is temporarily unavailable, the new links are not written, so the
notification can be retried on the next run.

To test the scraper and JSON state without sending mail:

```powershell
python .\ote_wsdl_monitor.py --no-email --data-file .\wsdl_links.test.json
python .\ote_wsdl_monitor.py --no-email --data-file .\wsdl_links.test.json
```

The second command should report that there are no new links. To send a mail
test without changing the state file:

```powershell
python .\ote_wsdl_monitor.py --test-email
```

## Daily run

Create a Windows Task Scheduler task that runs `python.exe` with the full path
to `ote_wsdl_monitor.py` once a day. Set the SMTP environment variables for
the task account as well; environment variables set only in an interactive
PowerShell window are not inherited by a scheduled task.