# An agent plays Microban 10

Level 5 of the sample is [Microban #10](https://cdn.jsdelivr.net/gh/OMerkel/Sokoban@master/3rdParty/Levels/Microban.txt)
by David W. Skinner : three boxes , three goals in a narrow corridor , 89 moves . Everything below goes through
UniInk : the questions are expressions , the turns are expressions , and the game code never learns what a box is
beyond the functions the protocol registers .

The loop , exactly as the window runs it :

1. the host builds a fresh interpreter and registers the board ( `px` , `py` , `steps` , `pushes` , `BOX` , `GOAL` ,
   `WALL` , `FREE` , `CAN_MOVE` , `BOX_AT` , `GOAL_AT` ) ,
2. it evaluates **one** expression ,
3. it applies the direction the expression answers with — or, when the expression names action letters , every move
   they queued , in order .

## 1 . Ask the board first

Nothing moves here : the expressions only answer .

```
level 4 : Microban 10 ( 11 x 8 )
###########
#######.  #
#######.# #
#######.# #
# @ $ $ $ #
# # # # ###
#       ###
###########

  ask  px
  back 2
  ask  py
  back 4
  ask  BOX( 4 , 4 )
  back True
  ask  BOX( 6 , 4 )
  back True
  ask  BOX( 8 , 4 )
  back True
  ask  GOAL( 7 , 1 ) && GOAL( 7 , 2 ) && GOAL( 7 , 3 )
  back True
  ask  FREE( 7 , 5 )
  back True
  ask  CAN_MOVE( right )
  back True
```

## 2 . Program the route

The route comes from the solver in `Core/Solver.cs` ( breadth first over player + boxes ) , then it is written as
UniInk : plain letter strings while the path is forced , and a **reactive rule** at the end , where reading the board
is cheaper than replaying a fixed path .

```bash
dotnet run --project Arc.UniInk.Sample.Sokoban/App -c Release -- \
  --script 4 Arc.UniInk.Sample.Sokoban/docs/agent-policies.txt 400
```

```
level 4 : Microban 10 ( 11 x 8 )
###########
#######.  #
#######.# #
#######.# #
# @ $ $ $ #
# # # # ###
#       ###
###########

── turn 1 ─────────────────────────────────────────────
expr : RDDRRUU
moves : right down down right right up up  ( action letters )
###########
#######.  #
#######.# #
#######.# #
#   $@$ $ #
# # # # ###
#       ###
###########

[ px 5 py 4 · steps 7 · pushes 0 · boxes 0/3 ]
── turn 2 ─────────────────────────────────────────────
expr : LLRRDDR
moves : left left right right down down right  ( action letters )
###########
#######.  #
#######.# #
#######.# #
# $   $ $ #
# # # # ###
#     @ ###
###########

[ px 6 py 6 · steps 14 · pushes 2 · boxes 0/3 ]
── turn 3 ─────────────────────────────────────────────
expr : RUUUUUR
moves : right up up up up up right  ( action letters )
###########
#######.@ #
#######.# #
#######.# #
# $   $ $ #
# # # # ###
#       ###
###########

[ px 8 py 1 · steps 21 · pushes 2 · boxes 0/3 ]
── turn 4 ─────────────────────────────────────────────
expr : RDDDLRU
moves : right down down down left right up  ( action letters )
###########
#######.  #
#######.# #
#######.#@#
# $   $$  #
# # # # ###
#       ###
###########

[ px 9 py 3 · steps 28 · pushes 3 · boxes 0/3 ]
── turn 5 ─────────────────────────────────────────────
expr : UULLDDD
moves : up up left left down down down  ( action letters )
###########
#######.  #
#######.# #
#######.# #
# $   $@  #
# # # #$###
#       ###
###########

[ px 7 py 4 · steps 35 · pushes 4 · boxes 0/3 ]
── turn 6 ─────────────────────────────────────────────
expr : LLDDRRU
moves : left left down down right right up  ( action letters )
###########
#######.  #
#######.# #
#######.# #
# $ $  $  #
# # # #@###
#       ###
###########

[ px 7 py 5 · steps 42 · pushes 7 · boxes 0/3 ]
── turn 7 ─────────────────────────────────────────────
expr : UUUDDDD
moves : up up up down down down down  ( action letters )
###########
#######*  #
#######.# #
#######.# #
# $ $     #
# # # # ###
#      @###
###########

[ px 7 py 6 · steps 49 · pushes 10 · boxes 1/3 ]
── turn 8 ─────────────────────────────────────────────
expr : LLLLUUR
moves : left left left left up up right  ( action letters )
###########
#######*  #
#######.# #
#######.# #
# $ @$    #
# # # # ###
#       ###
###########

[ px 4 py 4 · steps 56 · pushes 11 · boxes 1/3 ]
── turn 9 ─────────────────────────────────────────────
expr : RRLDDRR
moves : right right left down down right right  ( action letters )
###########
#######*  #
#######.# #
#######.# #
# $    $  #
# # # # ###
#      @###
###########

[ px 7 py 6 · steps 63 · pushes 13 · boxes 1/3 ]
── turn 10 ─────────────────────────────────────────────
expr : UUUDDDL
moves : up up up down down down left  ( action letters )
###########
#######*  #
#######*# #
#######.# #
# $       #
# # # # ###
#     @ ###
###########

[ px 6 py 6 · steps 70 · pushes 15 · boxes 2/3 ]
── turn 11 ─────────────────────────────────────────────
expr : LLLLLUU
moves : left left left left left up up  ( action letters )
###########
#######*  #
#######*# #
#######.# #
#@$       #
# # # # ###
#       ###
###########

[ px 1 py 4 · steps 77 · pushes 15 · boxes 2/3 ]
── turn 12 ─────────────────────────────────────────────
expr : RRRRRLD
moves : right right right right right left down  ( action letters )
###########
#######*  #
#######*# #
#######.# #
#      $  #
# # #@# ###
#       ###
###########

[ px 5 py 5 · steps 84 · pushes 20 · boxes 2/3 ]
── turn 13 ─────────────────────────────────────────────
expr : DRR
moves : down right right  ( action letters )
###########
#######*  #
#######*# #
#######.# #
#      $  #
# # # # ###
#      @###
###########

[ px 7 py 6 · steps 87 · pushes 20 · boxes 2/3 ]
── turn 14 ─────────────────────────────────────────────
expr : if ( CAN_MOVE( up ) ) { return up } else { return left }
moves : up  ( direction code )
###########
#######*  #
#######*# #
#######.# #
#      $  #
# # # #@###
#       ###
###########

[ px 7 py 5 · steps 88 · pushes 20 · boxes 2/3 ]
── turn 15 ─────────────────────────────────────────────
expr : if ( CAN_MOVE( up ) ) { return up } else { return left }
moves : up  ( direction code )
###########
#######*  #
#######*# #
#######*# #
#      @  #
# # # # ###
#       ###
###########

[ px 7 py 4 · steps 89 · pushes 21 · boxes 3/3 ]
solved in 89 steps with 21 pushes
```

## What each expression exercised

| Expression | Feature |
|---|---|
| `px` , `py` , `steps` , `pushes` | registered variables |
| `BOX( 4 , 4 )` | a registered function , two arguments |
| `GOAL( 7 , 1 ) && GOAL( 7 , 2 ) && GOAL( 7 , 3 )` | `&&` over three calls , bool results |
| `FREE( 7 , 5 )` | walkability , the push rules live in the host |
| `CAN_MOVE( right )` | the move rule as a function |
| `RDDRRUU` | the four action getters , queued in a single turn |
| `if ( CAN_MOVE( up ) ) { return up } else { return left }` | a conditional that returns a direction code |

No reflection , no code generation , no boxing on the evaluation path : the same single file that ships in the NuGet
package answers every turn above .
