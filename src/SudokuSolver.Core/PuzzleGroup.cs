using System.Collections.Generic;

namespace SudokuSolver.Engine
{
    public class PuzzleGroup
    {
        public string Name;
        public List<Puzzle> Puzzles;

        public PuzzleGroup(string name, List<string> puzzleValues)
        {
            Name = name;
            Puzzles = new List<Puzzle>();

            int counter = 1;
            foreach (string puzzleValue in puzzleValues)
            {
                var puzzle = new Puzzle(puzzleValue) { Group = name, Name = name + counter };
                Puzzles.Add(puzzle);
                counter++;
            }
        }
    }
}
