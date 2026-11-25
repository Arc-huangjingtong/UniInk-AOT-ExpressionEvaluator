#!/usr/bin/env bash
# Keep the Unity Package Manager folder in shape :
#   - copy the library sources into Runtime/
#   - keep package.json's version in step with the csproj
#   - make sure every file / folder inside the package has a .meta file
#
#   Tools/sync-unity-package.sh          # write
#   Tools/sync-unity-package.sh --check  # verify only , exit 1 on drift
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PACKAGE="$ROOT/Unity/Packages/com.arc.uniink"
CSPROJ="$ROOT/Arc.UniInk/Arc.UniInk.csproj"
CHECK=0
[[ "${1:-}" == "--check" ]] && CHECK=1

status=0

# ---------- version ----------
version="$(grep -o '<Version>[^<]*</Version>' "$CSPROJ" | head -1 | sed -E 's|</?Version>||g')"
package_version="$(grep -o '"version": *"[^"]*"' "$PACKAGE/package.json" | head -1 | sed -E 's/.*"([^"]*)"$/\1/')"

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

# ---------- sources ----------
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

# ---------- .meta ----------
# Unity refuses to load an asset inside an immutable package ("Packages/...") when
# it has no .meta , so every file and folder below the package root needs one .
guid_for() {
    local key="com.arc.uniink:$1"
    if printf 'x' | md5 -q >/dev/null 2>&1; then
        printf '%s' "$key" | md5 -q
    else
        printf '%s' "$key" | md5sum | cut -d' ' -f1
    fi
}

write_meta() {
    local rel="$1" out="$PACKAGE/${1}.meta"
    local guid; guid="$(guid_for "$rel")"

    if [[ -d "$PACKAGE/$rel" ]]; then
        cat > "$out" <<META
fileFormatVersion: 2
guid: $guid
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
META
    elif [[ "$rel" == *.cs ]]; then
        cat > "$out" <<META
fileFormatVersion: 2
guid: $guid
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
META
    elif [[ "$rel" == *.asmdef ]]; then
        cat > "$out" <<META
fileFormatVersion: 2
guid: $guid
AssemblyDefinitionImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
META
    else
        cat > "$out" <<META
fileFormatVersion: 2
guid: $guid
TextScriptImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
META
    fi
}

while IFS= read -r -d '' path; do
    rel="${path#"$PACKAGE/"}"
    [[ "$rel" == *.meta ]] && continue
    if [[ ! -f "$path.meta" ]]; then
        echo "missing meta: $rel"
        if [[ $CHECK -eq 1 ]]; then
            status=1
        else
            write_meta "$rel"
            echo "created $rel.meta"
        fi
    fi
done < <(find "$PACKAGE" -mindepth 1 \( -type d -o -type f \) -print0 | sort -z)

if [[ $status -eq 0 ]]; then
    echo "unity package is in sync ( version $version )"
fi

exit $status
