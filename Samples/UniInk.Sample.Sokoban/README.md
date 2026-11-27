# UniInk.Sample.Sokoban

A small **Sokoban that a string controls** : every turn the game evaluates one UniInk expression and applies the
direction it answers with . The point is to show the evaluator driving real behaviour — and to let an AI agent
play by writing one expression per turn .

```
dotnet run --project Samples/UniInk.Sample.Sokoban/App -c Release
```

- Arrow keys move the player by hand .
- **Run one turn** evaluates the policy once . **Auto run** repeats it . **Solve ( BFS )** plays the shortest
  solution found by the solver . **Reset** restarts the level .

## The protocol ( v1 )

Every turn the app builds a fresh `UniInk` interpreter , registers the board and evaluates the policy . The
expression must evaluate to an `int` : `0` up , `1` right , `2` down , `3` left .

| Name | Kind | Meaning |
|---|---|---|
| `px` , `py` | variable | the player column / row |
| `steps` , `pushes` | variable | the counters of the level |
| `solved` | variable | `1` when every box is on a goal |
| `up` `right` `down` `left` | variable | the direction codes , so a policy can `return right` |
| `WALL( x , y )` | function | the square is a wall ( outside the board counts as a wall ) |
| `BOX( x , y )` | function | a box stands there |
| `GOAL( x , y )` | function | the square is a goal |
| `FREE( x , y )` | function | the player can step there ( no wall , no box ) |
| `CAN_MOVE( d )` | function | the move is legal , a box may be pushed |
| `BOX_AT( d )` | function | the square in direction `d` holds a box |
| `GOAL_AT( d )` | function | the square in direction `d` is a goal |

A policy is **one expression** : there is no loop , the loop is the turn .

## Example policies

Push the box in front of you , otherwise walk down :

```
if ( BOX_AT( right ) && ! WALL( px + 2 , py ) ) { return right } else { return down }
```

Only step somewhere free , otherwise back off :

```
if ( FREE( px + 1 , py ) ) { return right } else { return left }
```

Prefer a push towards a goal :

```
if ( BOX_AT( right ) && GOAL( px + 2 , py ) ) { return right } else { return down }
```

The first level is solved by the first policy . The later levels need a real strategy — which is the interesting
part for an agent : it can read the board through the functions above and decide one move at a time .

## Structure

```
Core/     the rules ( Board , Game , Levels ) , the protocol ( UniInk names ) and a BFS solver
App/      the Avalonia window : the board , the policy box , one evaluation per turn
Tests/    rules , protocol and "every level is solvable" ( the BFS replays the path )
```

The sample is **not** part of the NuGet package or the Unity package : it is a playground for the evaluator .
