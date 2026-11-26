#!/usr/bin/env python3
"""Build a classic .unitypackage from the UniInk sources, without Unity.

A .unitypackage is a gzipped tar where every asset gets a folder named after its
GUID, holding:
    <guid>/asset        the file bytes          ( folders have no 'asset' )
    <guid>/asset.meta   the .meta file           ( same format Unity writes )
    <guid>/pathname     the path inside the project, e.g. Assets/Arc.UniInk/UniInk.cs

The GUIDs are derived from a separate namespace on purpose : importing this
package next to the UPM package must not produce two assets with one GUID .

Usage: Tools/make-unitypackage.py [--out DIR]
"""
import argparse
import gzip
import hashlib
import io
import json
import os
import pathlib
import tarfile

ROOT = pathlib.Path(__file__).resolve().parent.parent
SRC = ROOT / 'Arc.UniInk'
SAMPLE = ROOT / 'Unity/Packages/com.arc.uniink/Samples~/BasicEvaluation/UniInkSample.cs'
SKILL  = ROOT / 'skills/uniink'
CSPROJ = SRC / 'Arc.UniInk.csproj'
PREFIX = 'Assets/Arc.UniInk'
GUID_NS = 'com.arc.uniink.unitypackage:'

FOLDER_META = """fileFormatVersion: 2
guid: {guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

MONO_META = """fileFormatVersion: 2
guid: {guid}
MonoImporter:
  externalObjects: {{}}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {{instanceID: 0}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

TEXT_META = """fileFormatVersion: 2
guid: {guid}
TextScriptImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def guid_of(path: str) -> str:
    return hashlib.md5((GUID_NS + path).encode('utf-8')).hexdigest()


def meta_for(path: str, is_folder: bool) -> str:
    guid = guid_of(path)
    if is_folder:
        return FOLDER_META.format(guid=guid)
    if path.endswith('.cs'):
        return MONO_META.format(guid=guid)
    return TEXT_META.format(guid=guid)


def readme_text(version: str) -> str:
    return f"""# UniInk {version}

AOT-friendly, zero-allocation C# expression & mini-script evaluator for Unity IL2CPP.

- `UniInk.cs` — the evaluator ( required )
- `UniInk.Extensions.cs` — `if` / `else` statement support and the typed helpers ( optional )
- `Samples/BasicEvaluation/UniInkSample.cs` — a MonoBehaviour example
- `Skills/uniink/SKILL.md` — a skill for AI coding agents ( optional )

Both sources live in the `Arc.UniInk` namespace : `using Arc.UniInk;`

```csharp
var ink = new UniInk();
var value = ink.Evaluate("1 + 2 * 3");
var answer = value.GetResult_Int();   // reads it and returns the instance to the pool
```

Project page : https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator
"""


def collect() -> list[tuple[str, bytes | None]]:
    """( path inside the project , bytes or None for a folder ) in import order"""
    version = read_project_version()
    entries: list[tuple[str, bytes | None]] = []

    entries.append((PREFIX, None))
    entries.append((f'{PREFIX}/UniInk.cs', (SRC / 'UniInk.cs').read_bytes()))
    entries.append((f'{PREFIX}/UniInk.Extensions.cs', (SRC / 'UniInk.Extensions.cs').read_bytes()))
    entries.append((f'{PREFIX}/README.md', readme_text(version).encode('utf-8')))
    entries.append((f'{PREFIX}/Samples', None))
    entries.append((f'{PREFIX}/Samples/BasicEvaluation', None))
    entries.append((f'{PREFIX}/Samples/BasicEvaluation/UniInkSample.cs', SAMPLE.read_bytes()))

    # the agent skill : SKILL.md plus the references it links to
    entries.append((f'{PREFIX}/Skills', None))
    entries.append((f'{PREFIX}/Skills/uniink', None))

    for path in sorted(SKILL.rglob('*')):
        rel = path.relative_to(SKILL)

        if path.is_dir():
            entries.append((f'{PREFIX}/Skills/uniink/{rel.as_posix()}', None))
        else:
            entries.append((f'{PREFIX}/Skills/uniink/{rel.as_posix()}', path.read_bytes()))

    return entries


def read_project_version() -> str:
    text = CSPROJ.read_text(encoding='utf-8')
    start = text.index('<Version>') + len('<Version>')
    return text[start:text.index('</Version>', start)].strip()


def build(out_dir: pathlib.Path) -> pathlib.Path:
    version = read_project_version()
    out_dir.mkdir(parents=True, exist_ok=True)
    target = out_dir / f'Arc.UniInk-{version}.unitypackage'

    buffer = io.BytesIO()
    with tarfile.open(fileobj=buffer, mode='w') as tar:
        for path, data in collect():
            is_folder = data is None
            guid = guid_of(path)

            def add(name: str, payload: bytes, mode: int = 0o644) -> None:
                info = tarfile.TarInfo(name)
                info.size = len(payload)
                info.mode = mode
                info.mtime = 0
                info.uid = info.gid = 0
                info.uname = info.gname = ''
                tar.addfile(info, io.BytesIO(payload))

            # Unity reads the pathname verbatim : no trailing newline
            add(f'{guid}/pathname', path.encode('utf-8'))
            add(f'{guid}/asset.meta', meta_for(path, is_folder).encode('utf-8'))
            if not is_folder:
                add(f'{guid}/asset', data)

    with gzip.GzipFile(filename=str(target), mode='wb', mtime=0, compresslevel=9) as gz:
        gz.write(buffer.getvalue())

    return target


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument('--out', default=str(ROOT / 'artifacts'))
    args = parser.parse_args()
    target = build(pathlib.Path(args.out))
    print(f'wrote {target} ({target.stat().st_size} bytes)')


if __name__ == '__main__':
    main()
