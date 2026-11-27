# Syntax that parses

Everything below is accepted by the lexer in `UniInk.cs` ( `CompileLexerAndFill` ).

## Literals

| Kind | Forms | Notes |
|---|---|---|
| int | `0`, `42`, `-42` | `-` is the unary minus operator, not part of the literal |
| single | `1.5f`, `0.5F` | suffix `f` / `F` |
| double | `1.5`, `0.5d` | suffix `d` / `D` optional |
| char | `'a'`, `'\n'`, `'\''` | escapes: `\\ \' \0 \a \b \f \n \r \t \v` |
| string | `"hi"`, `"a\nb"` | same escapes, `"a" + "b"` concatenates two strings only |
| bool | `true`, `false` | |

## Operators, highest priority first

| Priority | Operators |
|---|---|
| 1 | `(` `)` |
| 3 | `!` |
| 5 | `*` `/` `%` |
| 6 | `+` `-` |
| 8 | `<` `>` `<=` `>=` |
| 9 | `==` `!=` |
| 13 | `&&` |
| 14 | `\|\|` |
| 16 | `=` , `,` |
| 17 | `=>` |
| 20 | `{` `}` `;` , keywords `if` `else` `var` `return` |

Reserved but **not implemented** : `.` `[` `]` `++` `--` `~` `<<` `>>` `&` `^` `\|` `?:` `:` `?` `@"`.
`Cast` is a placeholder (`😊()`) — there is no cast syntax.

## Statements

A **script** is a sequence of statements separated by `;` :

```
var a = 1; var b = 2; a + b
```

`if` ( `UniInk.Extensions.cs` ) :

```
if ( condition ) { return value }
if ( condition ) { return value } else { return value }
if ( condition ) { return value } else if ( other ) { return value } else { return value }
```

- the body must be wrapped in `{ }`
- use `return` when you need the value; a bare expression body is evaluated and discarded
- the last executed `return` wins
- malformed forms ( `else` without `{ }` ) throw `InkSyntaxException`

## Lambdas

Only as a function argument, and only with the `var` keyword for the parameter :

```
FLT( Config , var c => GET( c , Rarity ) == 2 )
```

The predicate is invoked by your own registered function while the same `Evaluate` is running.
It is not a value you can store.

## Comments

`//` and `/* */` are stripped by `UniInkExtensions.RemoveComments`, which `Evaluate_IfStatement`
calls for you. `Evaluate` itself does **not** strip them.
