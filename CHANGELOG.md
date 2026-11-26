# Changelog

All notable changes to **UniInk** are documented here.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and the project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- agent skills : `skills/uniink/` ( consumer , shipped inside the nupkg at `skills/uniink/` per the NuGet
  convention , and as the **Agent Skill ( AI )** Unity sample , the tarball and the `.unitypackage` ) and
  `.github/skills/uniink-repo/` ( how to work on this repository )
- `Tools/check-skills.py` validates the skill frontmatter , the links and the Unity copy , and runs in CI

- `InkValue.SetSetter` and a `RegisterVariable( name , value , setter )` overload : an assignment to a registered
  variable now notifies its owner , so the value can be mirrored into a field ( `hp = hp - 10` )
- setter tests : type changes , once per assignment , not called on read , getter + setter round trip , reentrancy

### Fixed

- `InkValue.Release` cleared the getter / setter flags but kept the delegates , so a pooled value could hold the
  owner's lambda alive ( and a stale delegate travelled with the instance )

## [1.3.6] - 2026-11-26

### Fixed

- the `if` statement path leaked one pooled `InkValue` per executed branch ( 392 B per `if` ) and cloned every
  `return` value instead of only the last one , so a four branch script allocated 2744 B per evaluation ;
  both are released now and the `if` benchmark reports 0 B

### Added

- the `if` benchmark now runs the same four arithmetic expressions wrapped in `if` statements , so it is directly
  comparable with the arithmetic benchmark instead of measuring a handful of literals
- every release now attaches `com.arc.uniink-<version>.tgz` ( `npm pack` layout ) next to the nupkg , so the Unity
  package can be installed offline with **Add package from tarball**
- CI runs `npm pack --dry-run` to make sure the package still packs
- every release also attaches `Arc.UniInk-<version>.unitypackage` , built by `Tools/make-unitypackage.py`
  without Unity ( a gzipped tar of `asset` / `asset.meta` / `pathname` entries ) , for users who prefer
  `Assets > Import Package` over the package manager

## [1.3.5] - 2026-11-25

### Added

- Unity Package Manager package (`com.arc.uniink`) : sources , asmdef , a Basic Evaluation sample , `.meta` files
  and a package README — install it with
  `https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator.git?path=Unity/Packages/com.arc.uniink`
- `Tools/sync-unity-package.sh` copies the sources into the Unity package , keeps `package.json` in step with
  `<Version>` and owns the `.meta` files ; `--check` fails the CI on drift
- Regression tests for the pool contract : a malformed unary operator and a function that returns its own argument
  must not hand the same pooled `InkValue` to two computations , plus tests for the still supported unary forms

### Fixed

- An operator without a unary meaning ( `*1` , `/1` , `==1` , `<1` , `>1` , `<=1` , `>=1` , `!=1` , `||1` ) returned its
  operand , so the syntax list released the same `InkValue` twice : RELEASE builds then handed one instance to two
  different computations ( a following `1 + 2 * 3` could evaluate to `0` ) and DEBUG builds threw
  `InvalidOperationException` from `InkValue.Release` . Those forms throw `InkSyntaxException` now
- A registered function that returned one of its own arguments released that argument twice ( the temporary parameter
  list and the caller both owned it ) ; the result is copied when the parameter list still owns it
- Unity refused to load `package.json` , `README.md` and `CHANGELOG.md` of the package ( "no meta file , but it is in
  an immutable folder" ) ; every file in the package has a `.meta` now and the script creates one when missing

## [1.3.0] - 2026-11-25

### Added

- `Arc.UniInk.Tests` project, NUnit based, separate from the benchmark project
- Tests for the literal scanner, the script path, `if` / `else if` / `else` and the `EvaluateInt` / `EvaluateBool` helpers
- `CHANGELOG.md`

### Changed

- **Breaking** : division by zero and a duplicate `var` declaration throw `InkSyntaxException` now , not
  `DivideByZeroException` / `ArgumentException`
- `InkSyntaxException` gained `Position` and `Expression` , and `ToString()` adds a caret under the failing character ;
  `InkSyntaxList` records the source position of every token so the caret also works inside nested expressions
- `InkValue.IsCalculate` , `DontRelease` , `IsGetter` , `IsIsSetter` and `IsReturner` are now read-only outside the assembly
- `InkOperator.MaxOperatorLen` is internal
- `AssemblyVersion` and `FileVersion` follow `<Version>`
- The solution is flat : `Arc.UniInk/` , `Arc.UniInk.Benchmark/` , `Arc.UniInk.Tests/` and `Arc.UniInk.Assets/` at the repository root
- Comments are English only

### Fixed

- Evaluating a malformed `if` statement ( an `else` without `{ ... }` ) looped forever
- `ExecuteProcess` results leaked the pooled `InkValue` when the caller never released them ( the benchmark allocated 1568 B per operation )
- `RegexOptions.Compiled` in `UniInk.Extensions` broke AOT builds, IL2CPP cannot generate code at runtime

## [1.2.0] - 2026-11-22

### Added

- Implicit numeric promotion, C# style : `int` -> `single` -> `double` , and `char` promotes to `int`
- `InkEvalContext` : one evaluation context per thread ( object pools , temp state , frame stack )
- `InkSyntaxException.Position` and `InkSyntaxException.Expression` , plus a caret in `ToString()`
- `netstandard2.0` in addition to `netstandard2.1`

### Changed

- **Breaking** : the global function table is gone, `RegisterGlobalFunction` was removed . Functions belong to the instance that registered them
- **Breaking** : `InkValue.Equals` keeps identity semantics ( documented ), `operator ==` is null safe
- The operator dispatch collapsed into a single core (`Apply` -> `Binary` -> `Arithmetic` / `Equality` / `Relational` / `Logical` )
- Every evaluation failure throws `InkSyntaxException` instead of leaking the runtime exceptions

### Fixed

- `(1 + 2)` evaluated to `null` when the expression started with a parenthesis
- Comparison operators failed for `bool` , `float` , `double` and `char` , and `<=` compared the wrong way
- A char literal swallowed the token that followed it ( `'a'=='a'` returned `null` )
- Concurrent evaluation corrupted the shared pools ( 15,932 exceptions over 16,000 evaluations with 8 threads )
- A registered function that called `Evaluate` again wiped the temp variables of the outer call
- The lambda path allocated a closure even when the expression had no lambda ( 32 B per evaluation )

## [0.1.3] - 2024-06

### Added

- NuGet packaging and the first published workflow

[Unreleased]: https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator/compare/ver1.3.6...HEAD
[1.3.6]: https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator/compare/ver1.3.5...ver1.3.6
[1.3.5]: https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator/compare/ver1.3...ver1.3.5
[1.3.0]: https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator/compare/ver1.2...ver1.3
[1.2.0]: https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator/compare/v0.1.3...ver1.2
[0.1.3]: https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator/releases/tag/v0.1.3
