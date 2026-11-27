# AGENTS.md

Instructions for AI coding agents working in this repository.

## Skills

Two skills live in `.agents/skills/` ( the shared Agent Skills location ) :

| Skill | Use it for |
|---|---|
| `uniink` | writing , reviewing and debugging UniInk expressions ; integrating `Arc.UniInk` into .NET or Unity |
| `uniink-repo` | changing this repository : build , tests , benchmark , Unity package , release |

Load `uniink` before you write an expression , `uniink-repo` before you touch the library or the release process .

`.github/skills` is a symlink to the same tree , so Copilot / VS Code find the skills as well :

```
.github/skills  ->  ../.agents/skills     GitHub Copilot / VS Code
```

A client that only reads its own directory ( Claude Code reads `.claude/skills` , Cursor `.cursor/skills` ) can
point one symlink at `.agents/skills` — nothing needs to be copied .

## Quick commands

```bash
dotnet build Arc.UniInk.sln -c Release
dotnet test  Arc.UniInk.Tests/Arc.UniInk.Tests.csproj -c Release
dotnet test  Arc.UniInk.Tests/Arc.UniInk.Tests.csproj -c Debug     # pool contract assertions
bash Tools/sync-unity-package.sh --check                          # the UPM copy , its version and its .meta files
python3 Tools/check-skills.py                                     # the skills and their discovery links
```

## Rules

- edit `Arc.UniInk/*.cs` , never `Unity/Packages/com.arc.uniink/Runtime/*.cs` — that directory is a copy
- no reflection , no `Reflection.Emit` , no `RegexOptions.Compiled` : the target is IL2CPP
- keep the evaluation path at 0 B per evaluation ; release every `InkValue` you take
- commits and pushes need the owner's explicit approval
