namespace Arc.UniInk.Sample.Sokoban.Core
{
    /// <summary> The built in levels , small enough for a policy written as an expression </summary>
    public static class Levels
    {
        /// <summary> `#` wall , `.` goal , ` ` floor , `@` player , `$` box , `*` box on goal </summary>
        private static readonly (string Name, string[] Rows, string Hint)[] Data =
        {
            ("First push" , new[]
                                 {
                                     "#####"
                                   , "#@$.#"
                                   , "#####"
                                 } , "walk right once : the box slides onto the goal"
            )
          , ("Turn the corner" , new[]
                                     {
                                         "#######"
                                       , "#     #"
                                       , "# $ . #"
                                       , "#  @  #"
                                       , "#######"
                                     } , "the box must go right , then the player must get behind it"
            )
          , ("Two boxes" , new[]
                              {
                                  "#########"
                                , "#..  $  #"
                                , "#   $   #"
                                , "#   @   #"
                                , "#########"
                              } , "two goals , two boxes : bring each one home"
            )
          , ("Corridor" , new[]
                             {
                                 "########"
                               , "#@$... #"
                               , "########"
                             } , "one box , three goals — only one of them matters for the win"
            )
            // Microban 10 by David W. Skinner ( revised April 2000 ) , used here as the hard level
          , ("Microban 10" , new[]
                                {
                                    "###########"
                                  , "#######.  #"
                                  , "#######.# #"
                                  , "#######.# #"
                                  , "# @ $ $ $ #"
                                  , "# # # # ###"
                                  , "#       ###"
                                  , "###########"
                                } , "three boxes , three goals in a narrow corridor : the order of the pushes is the puzzle ( Microban 10 , David W. Skinner )"
            )
        };

        public static int Count => Data.Length;

        public static Level Get(int index) => Level.Parse(Data[index].Name, Data[index].Rows, Data[index].Hint);

        public static Level[] All()
        {
            var levels = new Level[Data.Length];

            for (var i = 0 ; i < Data.Length ; i++) levels[i] = Get(i);

            return levels;
        }
    }
}
