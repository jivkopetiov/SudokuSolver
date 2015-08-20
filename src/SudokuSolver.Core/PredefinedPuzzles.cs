using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolver.Engine
{
    public class PredefinedPuzzles
    {
        public static List<Puzzle> GetPuzzlesByGroup(string name)
        {
            var group = puzzleGroups.Single(p => p.Name == name);
            return group.Puzzles;
        }

        public static List<Puzzle> GetAllValidPuzzles()
        {
            var puzzles = new List<Puzzle>();
            foreach (var group in puzzleGroups)
                puzzles.AddRange(group.Puzzles);

            return puzzles;
        }

        public static Puzzle GetRandomPuzzle()
        {
            var all = PredefinedPuzzles.GetAllValidPuzzles();
            return GetRandom(all);
        }

        public static Puzzle GetPuzzleByGroupAndNumber(string groupName, int counter)
        {
            var group = puzzleGroups.Single(p => p.Name == groupName);
            var puzzle = group.Puzzles[counter - 1];
            return puzzle;
        }

        private static readonly List<PuzzleGroup> puzzleGroups = new List<PuzzleGroup> {

            new PuzzleGroup("Guessing", new List<string> {
                { "000105000 140000070 080002400 060070010 900000003 010090020 007000080 026000035 000409000" },     
                { "000145090 145900070 089702451 060070019 900001043 010090020 097000184 426817935 801409060" },
                { "070145090 145900072 089702451 060070019 950001743 710090020 597000184 426817935 831459267" },
            }),

            new PuzzleGroup("Common", new List<string> {
                { "000000040 100300900 560208000 905060800 010000060 002070109 000402091 003005002 050000000" },
                { "000001240 120300900 560208010 905160820 010020060 602070109 006432591 493015082 251080000" },
                { "000105000 140000670 080002400 063070010 900000003 010090520 007200080 026000035 000409000" },
            }),

            new PuzzleGroup("websudoku.com-Evil", new List<string> {
                { "000000040 100300900 560208000 905060800 010000060 002070109 000402091 003005002 050000000" },
                { "003000000 504028093 800000001 002004000 000531000 000600400 600000007 780290305 000000200" },
                { "000900010 000048000 890000370 600300100 400050006 001009008 015000067 000710000 020005000" },
                { "094000300 201700000 000008000 000036090 900105004 050920000 000800000 000009201 003000760" },
                { "000092560 000007000 502400009 607080000 040000010 000010603 300006901 000800000 025340000" },
                { "760920100 000806200 300000000 000002907 010000080 409600000 000000009 005308000 004017028" },
                { "094007000 560000720 003060010 000073000 900000008 000140000 020080100 057000039 000700480" },
                { "400630050 070040300 000008004 000000209 006704800 109000000 300900000 002050080 090083005" },
                { "090050000 004001007 300000920 000700068 000284000 780005000 072000005 900300800 000020010" }
            }),

            new PuzzleGroup("extremesudoku.info", new List<string> {
                { "700000002 040203070 002070600 010305040 008000900 030408060 004050700 090706080 600000005" }, // 22 September 2011 - xwing and then swordfish
                { "008000700 010602090 900050002 020908030 006000100 090501080 700090003 050107060 002000900" }, // 17 October 2011 - xwing and then swordfish
                { "700800903 001002000 090050006 200300060 005000300 070001002 300060080 000200400 108009007" }, // 16 October 2011 - xwing
            }),

            new PuzzleGroup("nakedpairs", new List<string> {
                { "000000000 904607000 076804100 309701080 708000301 051308702 007502610 005403208 000000000" },
            })
        };

        private static readonly List<PuzzleGroup> invalidPuzzleGroups = new List<PuzzleGroup>
        {
            new PuzzleGroup("Invalid", new List<string> {
                { "000105000 100003070 080002400 060070010 904000003 010080020 007000080 026050035 000408000" },    
            }),
        };

        private static Random _random = new Random();

        private static T GetRandom<T>(List<T> items)
        {
            int number = _random.Next(1, 100000) % items.Count;
            return items[number];
        }

        public static Puzzle EmptyPuzzle
        {
            get
            {
                return new Puzzle("000000000 000000000 000000000 000000000 000000000 000000000 000000000 000000000 000000000") { Name = "EmptyPuzzle" };
            }
        }
    }
}
