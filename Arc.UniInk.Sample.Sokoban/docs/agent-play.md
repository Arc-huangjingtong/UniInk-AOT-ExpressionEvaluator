# An agent plays Microban 10

Level 5 in the sample is [Microban #10](https://cdn.jsdelivr.net/gh/OMerkel/Sokoban@master/3rdParty/Levels/Microban.txt)
by David W. Skinner : three boxes , three goals , 89 moves . The agent below never touches the game code — it reads
the board through UniInk and answers with UniInk expressions , one evaluation per turn .

The loop is the same as the window's :

1. the host builds a fresh interpreter and registers the board as variables and functions ,
2. it evaluates one expression ,
3. it applies the direction the expression answers with ( or the letters it queued ) .

## 1 . Ask the board

Each expression is evaluated , nothing moves .

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

## 2 . Program the moves

Every policy line is one turn . The letters go through the four action getters , so a line like `RDDRRUU` plays
seven moves from one expression .

```bash
dotnet run --project Arc.UniInk.Sample.Sokoban/App -c Release -- \
  --script 4 Arc.UniInk.Sample.Sokoban/docs/agent-policies.txt
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
expr : DRRUU
moves : down right right up up  ( action letters )
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

## What the expressions used

| Expression | What it exercised |
|---|---|
| `px` , `py` | a registered variable |
| `BOX( 4 , 4 )` , `GOAL( 7 , 1 )` | a registered function with two arguments |
| `GOAL( 7 , 1 ) && GOAL( 7 , 2 )` | `&&` over two calls |
| `FREE( 7 , 5 )` | a function that reports walkability |
| `CAN_MOVE( right )` | a function that reads the push rules |
| `RDDRRUU` | the four action getters , queued in one turn |

No reflection , no boxing on the evaluation path , and no code generated at runtime : the same 2 700 line file that
ships in the NuGet package runs every one of these turns .
