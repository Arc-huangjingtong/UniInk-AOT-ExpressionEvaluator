---
name: uniink-repo
description: Develop the UniInk repository itself — build , test , benchmark , update the Unity package , release. Use when a task changes Arc.UniInk.cs , UniInk.Extensions.cs , the tests , the benchmarks , the UPM package under Unity/ , the skills , the CI workflows or the release process.
license: MIT
compatibility: Needs the .NET 10 SDK ( the benchmark targets net10.0 ) , bash , python3 and npm on the PATH. No network for the build itself , NuGet access for restore.
metadata:
  package: Arc.UniInk
  version: "1.3.6"
  repository: https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator
  skill: repo-dev
---

# Working on the UniInk repository

UniInk is **one source file plus one optional extension file** that are shipped through three
channels ( NuGet , UPM , a classic `.unitypackage` ). Most mistakes in this repo come from updating
one channel and forgetting the others, so the rules below are about keeping them in sync.

## Layout

```
Arc.UniInk/               the library ( netstandard2.0 + netstandard2.1 )
  UniInk.cs               2 750 lines , the whole evaluator , hard limit ~3 000 lines
  UniInk.Extensions.cs    if / else , RemoveComments , EvaluateInt / EvaluateBool
Arc.UniInk.Tests/         NUnit , the only test project
Arc.UniInk.Benchmark/     BenchmarkDotNet , NOT a test project anymore
Unity/Packages/com.arc.uniink/   the UPM package ( sources are a COPY )
.agents/skills/          the shared Agent Skills tree ( .claude / .cursor / .github link to it )
  uniink/                 the consumer skill ( shipped in the nupkg and the Unity package )
    SKILL.md              entry : decision table , capability matrix , the two rules
    references/           syntax , types , errors , interop , recipes
    assets/               a copy-paste MonoBehaviour
  uniink-repo/            this skill
Tools/                    sync-unity-package.sh , make-unitypackage.py , check-skills.py
```

## Everyday commands

```bash
dotnet build Arc.UniInk.sln -c Release
dotnet test  Arc.UniInk.Tests/Arc.UniInk.Tests.csproj -c Release
dotnet test  Arc.UniInk.Tests/Arc.UniInk.Tests.csproj -c Debug     # pool contract assertions
bash Tools/sync-unity-package.sh --check                          # UPM copy + version + .meta
python3 Tools/check-skills.py                                     # skills , links and the UPM copy
dotnet run -c Release --project Arc.UniInk.Benchmark              # full BDN suite , ~2 min
```

**Always run Release *and* Debug tests** : the double release detection and the pool contract
assertions are `#if DEBUG`.

## Rules

1. **The Unity Runtime/ files are copies.** Never edit `Unity/Packages/com.arc.uniink/Runtime/*.cs`
   directly : edit `Arc.UniInk/*.cs` and run `Tools/sync-unity-package.sh` . CI fails on drift.
2. **Every file in the UPM package needs a `.meta`** with a deterministic GUID
   ( `md5("com.arc.uniink:" + relative path)` ) . The sync script creates missing ones in write mode
   and fails in `--check` mode.
3. **Version lives in one place** : `<Version>` in `Arc.UniInk/Arc.UniInk.csproj` . The sync script
   copies it into `package.json` , and the release workflow rewrites it from the tag.
4. **Line budget** : `UniInk.cs` must stay under 3 000 lines including comments. Prefer deleting
   noise over compressing documentation.
5. **Zero allocation is a contract.** Any change on the evaluation path must keep the benchmark at
   `Allocated -` . `InkValue` comes from a per-thread pool ; anything you take must be released
   ( `GetResult_*` , `InkValue.Release` ) , including copies that only exist to be discarded.
6. **No reflection, no `Reflection.Emit`, no `RegexOptions.Compiled`** — the target is IL2CPP.
7. **Errors are `InkSyntaxException`** with `Position` and `Expression` . Do not let
   `DivideByZeroException` / `ArgumentException` / `NullReferenceException` escape the evaluator.
8. **Comments are English and short.** The code base already carries a lot of XML docs ; match the style.
9. **Never commit without explicit approval.** The owner approves every commit and every push.
10. **Keep the three channels in step** : nupkg ( skills/uniink + README + CHANGELOG ) ,
    UPM ( Sources + `Samples~/BasicEvaluation` + `Samples~/AgentSkill` ) , `.unitypackage` ( sources + samples ).

## Adding a feature

1. Write the failing test first ( `Arc.UniInk.Tests/UniInk_NUnitTest.cs` , numbered region ) .
2. Implement in `Arc.UniInk/UniInk.cs` ( or `UniInk.Extensions.cs` ) .
3. `dotnet test` Release + Debug .
4. `bash Tools/sync-unity-package.sh` and commit both copies together .
5. Update `skills/uniink/` if the public API or syntax changed , `README.md` and `CHANGELOG.md` .
6. If the benchmark covers it , run the filtered benchmark and refresh the README table.

## Releasing

1. bump `<Version>` ( minor for a new API , patch for a fix ) , the source headers and the README badge
2. move the `[Unreleased]` section of `CHANGELOG.md` into a dated version section , add the compare links
3. `bash Tools/sync-unity-package.sh` , commit , push
4. `git tag -a verX.Y.Z -m "..." && git push origin verX.Y.Z`
5. the workflow publishes the nupkg + snupkg to NuGet , and attaches `com.arc.uniink-X.Y.Z.tgz` and
   `Arc.UniInk-X.Y.Z.unitypackage` to the GitHub release
6. NuGet indexing lags ~10–20 minutes : verify with the search API
   `https://azuresearch-usnc.nuget.org/query?q=packageid:Arc.UniInk` , not with `dotnet restore` alone

## Traps found the hard way

- a pooled `InkValue` discarded without `Release` shows up as `Allocated` in BDN days later
- `ProcessList` **copies** the result ; the copy belongs to the caller
- the `if` path once leaked one value per branch and cloned every `return`
- `InkValue.Release` did not clear the getter / setter delegates , keeping owners alive
- Unity refuses assets in a package without `.meta` ( "immutable folder" ) , including `package.json`
- a git URL dependency in Unity caches : after a fix the user must `Reinstall` the package
