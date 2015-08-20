using System;
using NUnit.Framework;
using SudokuSolver.Engine;

namespace SudokuSolver.Tests
{
    [TestFixture]
    public class StandardPredefinedTests
    {
        [Test]
        public void Puzzles_SolvedInTime()
        {
            foreach (var puzzle in PredefinedPuzzles.GetAllValidPuzzles())
                RunPuzzle(puzzle);
        }

        [Test]
        public void GenerateRandomPuzzle()
        {
            RunPuzzle(PredefinedPuzzles.EmptyPuzzle, maxExecution: 200);
        }

        private static void RunPuzzle(Puzzle puzzle, int maxExecution  = 100)
        {
            var engine = new SolverEngine();
            //engine.WriteLog = (message) => { Console.WriteLine(message); };
            engine.LoadPuzzle(puzzle);

            long elapsed = engine.TrySolveToEnd();
            Console.WriteLine(puzzle.Name.PadRight(19) + " : " + 
                              "e " + elapsed.ToString().PadRight(3) + ", " +
                              "g " + engine.GuessedMoves().ToString().PadRight(3) + ", " + 
                              "u " + engine.Undos.ToString().PadRight(3));

            bool solved = engine.IsFullySolved();
            Assert.IsTrue(solved, "Could not solve puzzle " + puzzle.Name);
            Assert.LessOrEqual(elapsed, maxExecution, "Elapsed milliseconds must be less than " + maxExecution);
        }
    }
}
