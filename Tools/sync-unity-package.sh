#!/usr/bin/env bash
# Copy the UniInk sources into the Unity Package Manager folder and keep the
# package.json version in sync with the csproj .
#
#   Tools/sync-unity-package.sh          # write
#   Tools/sync-unity-package.sh --check  # verify only , exit 1 on drift
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PACKAGE="$ROOT/Unity/Packages/com.arc.uniink"
CSPROJ="$ROOT/Arc.UniInk/Arc.UniInk.csproj"
CHECK=0
[[ "${1:-}" == "--check" ]] && CHECK=1

version="$(grep -o '<Version>[^<]*</Version>' "$CSPROJ" | head -1 | sed -E 's|</?Version>||g')"
package_version="$(grep -o '"version": *"[^"]*"' "$PACKAGE/package.json" | head -1 | sed -E 's/.*"([^"]*)"$/\1/')"

status=0

if [[ "$version" != "$package_version" ]]; then
    echo "version drift: csproj=$version package.json=$package_version"
    if [[ $CHECK -eq 1 ]]; then
        status=1
    else
        sed -i.bak -E "s|\"version\": *\"[^\"]*\"|\"version\": \"$version\"|" "$PACKAGE/package.json"
        rm -f "$PACKAGE/package.json.bak"
        echo "package.json version -> $version"
    fi
fi

for file in UniInk.cs UniInk.Extensions.cs; do
    if ! diff -q "$ROOT/Arc.UniInk/$file" "$PACKAGE/Runtime/$file" >/dev/null 2>&1; then
        echo "source drift: Runtime/$file"
        if [[ $CHECK -eq 1 ]]; then
            status=1
        else
            cp "$ROOT/Arc.UniInk/$file" "$PACKAGE/Runtime/$file"
            echo "copied $file"
        fi
    fi
done

if [[ $status -eq 0 ]]; then
    echo "unity package is in sync ( version $version )"
fi

exit $status
