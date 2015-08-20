using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SudokuSolver.Engine
{
    public class Puzzle
    {
        public int[,] Values { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }

        public Puzzle(string state)
        {
            Values = new int[9, 9];

            int counter = 0;
            foreach (string line in state.Split(new string[] { " " }, StringSplitOptions.None).Select(l => l.Trim()))
            {
                for (int i = 0; i < 9; i++)
                {
                    char ch = line[i];
                    int val = int.Parse(ch.ToString());
                    if (val > 0)
                        Values[counter, i] = val;
                }
                counter++;
            }
        }
    }
}
