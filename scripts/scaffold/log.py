"""Colored logging helpers."""

import os
import sys

if sys.platform == "win32":
    os.system("")  # enable ANSI on Windows

CYAN = "\033[0;36m"
GREEN = "\033[0;32m"
YELLOW = "\033[0;33m"
RED = "\033[0;31m"
NC = "\033[0m"


def _print_safe(msg: str, file: object = None) -> None:
    try:
        print(msg, file=file)
    except UnicodeEncodeError:
        print(msg.encode("ascii", errors="replace").decode(), file=file)


def log(msg: str) -> None:
    _print_safe(f"{CYAN}>{NC} {msg}")


def ok(msg: str) -> None:
    _print_safe(f"{GREEN}OK{NC} {msg}")


def warn(msg: str) -> None:
    _print_safe(f"{YELLOW}WARN{NC} {msg}")


def fail(msg: str) -> None:
    _print_safe(f"{RED}FAIL{NC} {msg}")


def fatal(msg: str) -> None:
    _print_safe(f"{RED}FATAL:{NC} {msg}", file=sys.stderr)
    sys.exit(1)
