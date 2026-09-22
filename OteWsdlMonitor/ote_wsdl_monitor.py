#!/usr/bin/env python3
"""Monitor OTE documentation for new wsdl*.zip links."""

from __future__ import annotations

import argparse
import json
import os
import posixpath
import re
import smtplib
import ssl
import sys
import tempfile
from datetime import datetime, timezone
from email.message import EmailMessage
from html.parser import HTMLParser
from pathlib import Path
from urllib.parse import unquote, urldefrag, urljoin, urlsplit
from urllib.request import Request, urlopen


PAGE_URL = "https://www.ote-cr.cz/cs/dokumentace/xsd-wsdl-manual-pubweb"
RECIPIENT = "pavel.balas@seyfor.com"
DEFAULT_DATA_FILE = Path(__file__).with_name("wsdl_links.json")
WSDL_FILENAME = re.compile(r"^wsdl.*\.zip$", re.IGNORECASE)


class LinkParser(HTMLParser):
    def __init__(self) -> None:
        super().__init__(convert_charrefs=True)
        self.hrefs: list[str] = []

    def handle_starttag(self, tag: str, attrs: list[tuple[str, str | None]]) -> None:
        if tag.lower() != "a":
            return

        href = dict(attrs).get("href")
        if href:
            self.hrefs.append(href)


def utc_now() -> str:
    return datetime.now(timezone.utc).isoformat(timespec="seconds").replace("+00:00", "Z")


def fetch_wsdl_links(page_url: str, timeout: int) -> list[str]:
    request = Request(
        page_url,
        headers={"User-Agent": "ote-wsdl-monitor/1.0"},
    )

    with urlopen(request, timeout=timeout) as response:
        content_type = response.headers.get_content_charset() or "utf-8"
        page = response.read().decode(content_type, errors="replace")

    parser = LinkParser()
    parser.feed(page)

    links: set[str] = set()
    for href in parser.hrefs:
        absolute_url = urldefrag(urljoin(page_url, href.strip()))[0]
        parsed_url = urlsplit(absolute_url)
        filename = unquote(posixpath.basename(parsed_url.path))

        if (
            parsed_url.scheme in {"http", "https"}
            and WSDL_FILENAME.fullmatch(filename)
        ):
            links.add(absolute_url)

    return sorted(links, key=str.casefold)


def load_state(data_file: Path, page_url: str) -> dict[str, object]:
    if not data_file.exists():
        return {"page_url": page_url, "links": []}

    try:
        with data_file.open("r", encoding="utf-8") as file:
            state = json.load(file)
    except json.JSONDecodeError as error:
        raise RuntimeError(f"Data file is not valid JSON: {data_file}") from error

    if not isinstance(state, dict) or not isinstance(state.get("links"), list):
        raise RuntimeError(f"Data file has an unexpected format: {data_file}")

    for entry in state["links"]:
        if not isinstance(entry, dict) or not isinstance(entry.get("url"), str):
            raise RuntimeError(f"Data file contains an invalid link record: {data_file}")

    return state


def save_state(data_file: Path, page_url: str, links: list[dict[str, str]]) -> None:
    data_file.parent.mkdir(parents=True, exist_ok=True)
    state = {
        "page_url": page_url,
        "updated_at": utc_now(),
        "links": links,
    }

    temporary_name: str | None = None
    try:
        with tempfile.NamedTemporaryFile(
            mode="w",
            encoding="utf-8",
            dir=data_file.parent,
            prefix=f".{data_file.name}.",
            suffix=".tmp",
            delete=False,
        ) as temporary_file:
            temporary_name = temporary_file.name
            json.dump(state, temporary_file, ensure_ascii=True, indent=2)
            temporary_file.write("\n")
        os.replace(temporary_name, data_file)
    finally:
        if temporary_name and os.path.exists(temporary_name):
            os.unlink(temporary_name)


def env_value(*names: str, default: str | None = None) -> str | None:
    for name in names:
        value = os.getenv(name)
        if value:
            return value
    return default


def env_bool(*names: str, default: bool) -> bool:
    value = env_value(*names)
    if value is None:
        return default
    return value.strip().lower() in {"1", "true", "yes", "on"}


def smtp_settings() -> dict[str, object]:
    host = env_value("OTE_SMTP_HOST", "SMTP_HOST")
    if not host:
        raise RuntimeError(
            "SMTP is not configured. Set OTE_SMTP_HOST (and credentials if required)."
        )

    port_text = env_value("OTE_SMTP_PORT", "SMTP_PORT", default="587")
    try:
        port = int(port_text or "587")
    except ValueError as error:
        raise RuntimeError(f"Invalid SMTP port: {port_text}") from error

    username = env_value("OTE_SMTP_USERNAME", "SMTP_USERNAME")
    password = env_value("OTE_SMTP_PASSWORD", "SMTP_PASSWORD")
    if username and not password:
        raise RuntimeError(
            "SMTP username is set but the password is missing in OTE_SMTP_PASSWORD."
        )

    use_ssl = env_bool("OTE_SMTP_SSL", "SMTP_SSL", default=False)
    starttls = env_bool("OTE_SMTP_STARTTLS", "SMTP_STARTTLS", default=not use_ssl)
    if use_ssl and starttls:
        raise RuntimeError("Do not enable both OTE_SMTP_SSL and OTE_SMTP_STARTTLS.")
    if not use_ssl and not starttls:
        raise RuntimeError("SMTP must use SSL or STARTTLS.")

    return {
        "host": host,
        "port": port,
        "username": username,
        "password": password,
        "sender": env_value("OTE_SMTP_FROM", "SMTP_FROM", default=username or RECIPIENT),
        "use_ssl": use_ssl,
        "starttls": starttls,
    }


def tls_context() -> ssl.SSLContext:
    context = ssl.SSLContext(ssl.PROTOCOL_TLS_CLIENT)
    context.minimum_version = ssl.TLSVersion.TLSv1_2
    context.check_hostname = True
    context.verify_mode = ssl.CERT_REQUIRED
    context.load_default_certs(ssl.Purpose.SERVER_AUTH)
    return context


def send_email(recipient: str, subject: str, body: str) -> None:
    settings = smtp_settings()
    message = EmailMessage()
    message["From"] = str(settings["sender"])
    message["To"] = recipient
    message["Subject"] = subject
    message.set_content(body)

    if bool(settings["use_ssl"]):
        smtp: smtplib.SMTP = smtplib.SMTP_SSL(
            str(settings["host"]),
            int(settings["port"]),
            timeout=30,
            context=tls_context(),
        )
    else:
        smtp = smtplib.SMTP(
            str(settings["host"]),
            int(settings["port"]),
            timeout=30,
        )

    with smtp:
        if not bool(settings["use_ssl"]):
            smtp.starttls(context=tls_context())
        if settings["username"]:
            smtp.login(str(settings["username"]), str(settings["password"]))
        smtp.send_message(message)


def filename_from_url(link: str) -> str:
    return unquote(posixpath.basename(urlsplit(link).path))


def build_email_body(
    page_url: str,
    links: list[str],
    inserted_at: str,
    test_message: bool = False,
) -> str:
    lines = [
        "This is a test message from the OTE WSDL monitor." if test_message else "New WSDL ZIP links were found on the OTE page.",
        "",
        f"Page: {page_url}",
        "",
    ]

    for link in links:
        lines.extend(
            [
                f"File: {filename_from_url(link)}",
                f"URL: {link}",
                f"Inserted at (UTC): {inserted_at}",
                "",
            ]
        )

    return "\n".join(lines)


def parse_arguments() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--page-url", default=PAGE_URL)
    parser.add_argument("--data-file", type=Path, default=DEFAULT_DATA_FILE)
    parser.add_argument("--recipient", default=RECIPIENT)
    parser.add_argument("--timeout", type=int, default=30)
    parser.add_argument(
        "--no-email",
        action="store_true",
        help="Save newly found links without sending an email.",
    )
    parser.add_argument(
        "--test-email",
        action="store_true",
        help="Send a test email containing the first current WSDL link without changing the data file.",
    )
    return parser.parse_args()


def main() -> int:
    arguments = parse_arguments()

    try:
        current_links = fetch_wsdl_links(arguments.page_url, arguments.timeout)
        if not current_links:
            raise RuntimeError("No wsdl*.zip links were found on the page.")

        if arguments.test_email:
            sent_at = utc_now()
            send_email(
                arguments.recipient,
                "OTE WSDL monitor test",
                build_email_body(
                    arguments.page_url,
                    current_links[:1],
                    sent_at,
                    test_message=True,
                ),
            )
            print(f"Test email sent to {arguments.recipient}.")
            return 0

        state = load_state(arguments.data_file, arguments.page_url)
        stored_records = [entry for entry in state["links"] if isinstance(entry, dict)]
        stored_urls = {str(entry["url"]) for entry in stored_records}
        new_links = [link for link in current_links if link not in stored_urls]

        if not new_links:
            print(f"No new WSDL links found. Stored links: {len(stored_urls)}.")
            return 0

        inserted_at = utc_now()
        if arguments.no_email:
            print("Email was skipped because --no-email was specified.")
        else:
            send_email(
                arguments.recipient,
                f"OTE: {len(new_links)} new WSDL ZIP link(s)",
                build_email_body(arguments.page_url, new_links, inserted_at),
            )
            print(f"Notification email sent to {arguments.recipient}.")

        updated_records = list(stored_records)
        updated_records.extend(
            {"url": link, "added_at": inserted_at} for link in new_links
        )
        save_state(arguments.data_file, arguments.page_url, updated_records)
        print(f"Added {len(new_links)} link(s) to {arguments.data_file}.")
        return 0
    except (OSError, RuntimeError, ValueError) as error:
        print(f"Error: {error}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())