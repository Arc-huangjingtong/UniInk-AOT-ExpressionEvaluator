# Changelog

All notable changes to **UniInk** are documented here.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and the project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.4.0] - 2026-11-28

### Added

- `Arc.UniInk.Sample.Sokoban/` : an Avalonia Sokoban where one UniInk expression per turn controls the player ,
  with a board protocol ( `px` / `py` / `WALL` / `BOX` / `GOAL` / `CAN_MOVE` … ) , a BFS solver and tests that prove
  every level solvable . Not shipped in the nupkg or the Unity package . The app sets `UseAppHost=false` , so
  `dotnet run` works on macOS where an unsigned apphost is killed by Gatekeeper
- the sample window is redesigned : a dark surface palette in `Skin.cs` , a header with the step / push / box
  counters , a board that scales to the card , a legend under it , the policy editor with example chips , a
  coloured status line and the board functions listed beside it , with a ripple when a box lands home ( every box
  ripples in a short stagger when the level is cleared )
- the sample gains a hard level — Microban #10 by David W. Skinner , three boxes , 89 moves — and a headless
  player ( `--list` , `--ask` , `--script` , `--solve` ) so an agent can read the board through UniInk and play it
  one expression per turn ; `docs/agent-play.md` is a recorded run and `docs/agent-play.mp4` is a 75 second
  recording of the window: the agent types each expression , plays it one move at a time with a `【。。。】`
  thinking beat , deletes the line quickly and types the next one ( `--replay` runs the same playback )
- the sample registers `U` `R` `D` `L` as **getters** : naming one appends to the move queue of the turn , so
  `R` is one move , `RR` two and `RL` right then left , and the host stops at the first blocked move . The
  `W` `A` `S` `D` scheme is selectable instead ( `D` conflicts with the arrow scheme , so only one is registered )
- the consumer skill now follows the Agent Skills specification completely : `compatibility` and `metadata`
  ( the version is kept in step with the package ) , a decision table and a capability matrix in `SKILL.md` ,
  `references/recipes.md` with twelve expressions checked against the real library , and
  `assets/UniInkSetup.cs` as a copy-paste template ( shipped in the UPM sample , not in the `.unitypackage` ,
  where a .cs under Assets would be compiled )
- `InkValue.SetSetter` : an assignment to a registered variable notifies its owner , so the value can be
  mirrored into a field ( `hp = hp - 10` ) ; the setter runs once per assignment , never on a read
- agent skills : `.agents/skills/uniink/` ( the consumer skill , shipped inside the nupkg at `skills/uniink/` per the
  NuGet convention , and as the **Agent Skill ( AI )** Unity sample , the tarball and the `.unitypackage` ) and
  `.agents/skills/uniink-repo/` ( how to work on this repository )
- `Tools/check-skills.py` validates the skill frontmatter , the links and the Unity copy ; CI runs it and proves
  that the nupkg carries the skill
- setter tests : called on assignment , sees the new type , not called on a read , once per assignment ,
  getter + setter round trip , every assignment of a script , reentrancy , no setter required

### Changed

- the lexer dispatches on the character class ( number / char / string / word / symbol ) instead of trying seven
  scanners for every character : the `ParsingMethodDelegate` list , `StartsWithBoolFromIndex` and
  `StartsWithInputStrFromIndex` are gone . `UniInk.cs` 2810 -> 2685 lines , `TEST_Arithmetic__UniInk`
  126.8 us -> 92.2 us , allocation still 0
- `ExecuteProcess` returns `InkValue` instead of `object` , so `Evaluate` forwards the result without a cast
- `RecoverResources` is now `ReleaseInkSyntaxList` ; it sits together with `ExecuteProcess` and `ClearCache` next
  to `CompileLexerAndFill`
- `InkOperator` drops `Equals` , `GetHashCode` , `Indexer` and `OperatorValue` : the operators are the static
  singletons , so reference equality is the same thing and one virtual call per comparison goes away
- `UniInk( IDictionary )` moved to `UniInk.Extensions.cs` ; `UniInk.cs` keeps a parameterless constructor , so the
  single file still stands alone
- `GetStringSliceHashCode` is internal and the whole-string overload is gone
- `RegisterVariable` keeps only the cell overload , `SetSetter` is the way to attach a setter
- the `// ReSharper disable` header comments are gone

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

[1.4.0]: https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator/compare/ver1.3.6...ver1.4
[1.3.6]: https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator/compare/ver1.3.5...ver1.3.6
[1.3.5]: https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator/compare/ver1.3...ver1.3.5
[1.3.0]: https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator/compare/ver1.2...ver1.3
[1.2.0]: https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator/compare/v0.1.3...ver1.2
[0.1.3]: https://github.com/Arc-huangjingtong/UniInk-AOT-ExpressionEvaluator/releases/tag/v0.1.3
