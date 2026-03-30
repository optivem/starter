"""File manipulation helpers: replace, rename, walk."""

from __future__ import annotations

import os
import shutil


def is_git_dir(dirpath: str) -> bool:
    return ".git" in dirpath.split(os.sep)


def replace_in_file(filepath: str, old: str, new: str) -> bool:
    """Replace all occurrences of old with new in a file."""
    try:
        with open(filepath, "r", encoding="utf-8", errors="ignore") as f:
            content = f.read()
    except (OSError, UnicodeDecodeError):
        return False
    if old not in content:
        return False
    with open(filepath, "w", encoding="utf-8", newline="\n") as f:
        f.write(content.replace(old, new))
    return True


def replace_in_tree(
    root: str, old: str, new: str, extensions: list[str] | None = None,
) -> int:
    """Replace in all text files under root, optionally filtered by extension."""
    count = 0
    for dirpath, _dirnames, filenames in os.walk(root):
        if is_git_dir(dirpath):
            continue
        for fname in filenames:
            if extensions and not any(fname.endswith(ext) for ext in extensions):
                continue
            if replace_in_file(os.path.join(dirpath, fname), old, new):
                count += 1
    return count


def replace_in_dockerfiles(root: str, old: str, new: str) -> int:
    """Replace in all Dockerfile files under root."""
    count = 0
    for dirpath, _dirnames, filenames in os.walk(root):
        if is_git_dir(dirpath):
            continue
        if "Dockerfile" in filenames:
            if replace_in_file(os.path.join(dirpath, "Dockerfile"), old, new):
                count += 1
    return count


def rename_java_dirs(root: str, old_parts: list[str], new_parts: list[str]) -> None:
    """Rename Java package directories: com/optivem/starter -> com/owner/repo."""
    old_path = os.path.join(*old_parts)
    new_path = os.path.join(*new_parts)
    for dirpath, _dirnames, _filenames in os.walk(root):
        if old_path in dirpath:
            new_dirpath = dirpath.replace(old_path, new_path)
            os.makedirs(os.path.dirname(new_dirpath), exist_ok=True)
            if os.path.exists(dirpath) and not os.path.exists(new_dirpath):
                shutil.move(dirpath, new_dirpath)
            break
    # Clean up empty old directories
    for dirpath, dirnames, filenames in os.walk(root, topdown=False):
        if old_parts[1] in dirpath.split(os.sep) and not filenames and not dirnames:
            try:
                os.rmdir(dirpath)
            except OSError:
                pass


def rename_dotnet_files(root: str, old_prefix: str, new_prefix: str) -> None:
    """Rename .NET files: Optivem.Starter.X.csproj -> NewNs.X.csproj etc."""
    for dirpath, _dirnames, filenames in os.walk(root):
        for fname in filenames:
            if old_prefix in fname:
                old_path = os.path.join(dirpath, fname)
                new_path = os.path.join(dirpath, fname.replace(old_prefix, new_prefix))
                os.rename(old_path, new_path)
