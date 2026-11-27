# Release checklist ( copy into the PR / notes )

- [ ] `<Version>` bumped in `Arc.UniInk/Arc.UniInk.csproj`
- [ ] source headers ( `UniInk.cs` , `UniInk.Extensions.cs` ) and the test header updated
- [ ] README version badge updated
- [ ] `CHANGELOG.md` : `[Unreleased]` turned into `[X.Y.Z] - YYYY-MM-DD` , new empty `[Unreleased]` ,
      compare links added
- [ ] `bash Tools/sync-unity-package.sh` ( copies sources , bumps `package.json` , creates missing `.meta` )
- [ ] `bash Tools/sync-unity-package.sh --check` passes
- [ ] `dotnet build` + `dotnet test` Release and Debug pass
- [ ] `dotnet pack` produces the expected version , `skills/uniink/SKILL.md` inside
- [ ] commit , push , tag `verX.Y.Z` , push the tag
- [ ] workflow green : nupkg pushed , release has `nupkg` , `tgz` , `unitypackage`
- [ ] NuGet search API lists the version before announcing it
