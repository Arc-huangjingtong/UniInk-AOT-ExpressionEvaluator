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

## 2 . Play it

Nine turns are letter runs ( the action getters queue them ) , six are decisions written with the board functions ,
and the last two are the reactive rule that pushes the third box home .

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
expr : RDD
moves : right down down  ( action letters )
###########
#######.  #
#######.# #
#######.# #
#   $ $ $ #
# # # # ###
#  @    ###
###########

[ px 3 py 6 · steps 3 · pushes 0 · boxes 0/3 ]
── turn 2 ─────────────────────────────────────────────
expr : if ( ! WALL( px + 1 , py ) ) { return right } else { return left }
moves : right  ( direction code )
###########
#######.  #
#######.# #
#######.# #
#   $ $ $ #
# # # # ###
#   @   ###
###########

[ px 4 py 6 · steps 4 · pushes 0 · boxes 0/3 ]
── turn 3 ─────────────────────────────────────────────
expr : RUULLRRD
moves : right up up left left right right down  ( action letters )
###########
#######.  #
#######.# #
#######.# #
# $   $ $ #
# # #@# ###
#       ###
###########

[ px 5 py 5 · steps 12 · pushes 2 · boxes 0/3 ]
── turn 4 ─────────────────────────────────────────────
expr : if ( CAN_MOVE( down ) ) { return down } else { return up }
moves : down  ( direction code )
###########
#######.  #
#######.# #
#######.# #
# $   $ $ #
# # # # ###
#    @  ###
###########

[ px 5 py 6 · steps 13 · pushes 2 · boxes 0/3 ]
── turn 5 ─────────────────────────────────────────────
expr : RRUUUUURRDDDL
moves : right right up up up up up right right down down down left  ( action letters )
###########
#######.  #
#######.# #
#######.# #
# $   $$@ #
# # # # ###
#       ###
###########

[ px 8 py 4 · steps 26 · pushes 3 · boxes 0/3 ]
── turn 6 ─────────────────────────────────────────────
expr : if ( ! WALL( px + 1 , py ) ) { return right } else { return left }
moves : right  ( direction code )
###########
#######.  #
#######.# #
#######.# #
# $   $$ @#
# # # # ###
#       ###
###########

[ px 9 py 4 · steps 27 · pushes 3 · boxes 0/3 ]
── turn 7 ─────────────────────────────────────────────
expr : UUULLDDDLLDDRRUUUU
moves : up up up left left down down down left left down down right right up up up up  ( action letters )
###########
#######*  #
#######+# #
#######.# #
# $ $     #
# # # # ###
#       ###
###########

[ px 7 py 2 · steps 45 · pushes 10 · boxes 1/3 ]
── turn 8 ─────────────────────────────────────────────
expr : if ( CAN_MOVE( down ) ) { return down } else { return up }
moves : down  ( direction code )
###########
#######*  #
#######.# #
#######+# #
# $ $     #
# # # # ###
#       ###
###########

[ px 7 py 3 · steps 46 · pushes 10 · boxes 1/3 ]
── turn 9 ─────────────────────────────────────────────
expr : DDDLLLLUURRRLDDRRUUU
moves : down down down left left left left up up right right right left down down right right up up up  ( action letters )
###########
#######*  #
#######*# #
#######+# #
# $       #
# # # # ###
#       ###
###########

[ px 7 py 3 · steps 66 · pushes 15 · boxes 2/3 ]
── turn 10 ─────────────────────────────────────────────
expr : if ( CAN_MOVE( down ) ) { return down } else { return up }
moves : down  ( direction code )
###########
#######*  #
#######*# #
#######.# #
# $    @  #
# # # # ###
#       ###
###########

[ px 7 py 4 · steps 67 · pushes 15 · boxes 2/3 ]
── turn 11 ─────────────────────────────────────────────
expr : DDLLLLLLUURRRRRLD
moves : down down left left left left left left up up right right right right right left down  ( action letters )
###########
#######*  #
#######*# #
#######.# #
#      $  #
# # #@# ###
#       ###
###########

[ px 5 py 5 · steps 84 · pushes 20 · boxes 2/3 ]
── turn 12 ─────────────────────────────────────────────
expr : if ( CAN_MOVE( down ) ) { return down } else { return up }
moves : down  ( direction code )
###########
#######*  #
#######*# #
#######.# #
#      $  #
# # # # ###
#    @  ###
###########

[ px 5 py 6 · steps 85 · pushes 20 · boxes 2/3 ]
── turn 13 ─────────────────────────────────────────────
expr : RR
moves : right right  ( action letters )
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
| `FREE( px , py - 1 )` | walkability with arithmetic on the player position |
| `BOX( px , py - 1 )` | the same square already occupied by a box |
| `! WALL( px + 1 , py )` | negation and a computed coordinate |
| `CAN_MOVE( down )` | the push rules as a function |
| `RDDRRUU` | the four action getters , queued in a single turn |
| `if ( CAN_MOVE( up ) ) { return up } else { return left }` | a conditional that returns a direction code |

No reflection , no code generation , no boxing on the evaluation path : the same single file that ships in the NuGet
package answers every turn above .
