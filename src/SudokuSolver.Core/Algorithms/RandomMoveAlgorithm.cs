using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolver.Engine
{
    public class RandomMoveAlgorithm : Algorithm
    {
        private Random _random = new Random();

        public RandomMoveAlgorithm(SolverEngine engine)
            : base(engine) { }

        public override Move Execute()
        {
            var possibles = _engine.possibles;
            var values = _engine.values;

            int minPossibles = int.MaxValue;
            int numberJ = -1;
            int numberI = -1;

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (values[i, j] > 0)
                        continue;

                    int possiblesCount = possibles[i, j].Count;
                    if (possiblesCount < minPossibles)
                    {
                        minPossibles = possiblesCount;
                        numberI = i;
                        numberJ = j;
                    }
                }
            }

            if (numberI == -1 || numberJ == -1)
            {
                _engine.WriteLog("Bug in the algorithm. Couldn't find a minimum cell to randomize");
                return null;
            }

            var poss = possibles[numberI, numberJ];

            if (poss.Count == 0)
            {
                _engine.WriteLog(string.Format("Cannot continue further. Found a spot with no possibles: {0}", _engine.CellFormat(numberI, numberJ)));
                return null;
            }
            else if (poss.Count == 1)
            {
                int val = poss.Single();
                return new SolveNumberMove { Value = val, Row = numberI, Column = numberJ, Algorithm = this };
            }
            else
            {
                var guess = _engine.guesses[numberI, numberJ];
                if (guess == null)
                {
                    int val = GetRandom(poss);
                    _engine.guesses[numberI, numberJ] = new List<int> { val };
                    var move = new SolveNumberMove { Value = val, Row = numberI, Column = numberJ, Algorithm = this };
                    _engine.lastRandomMove = move;
                    return move;
                }
                else
                {
                    while (poss.Any())
                    {
                        int val = GetRandom(poss);

                        if (guess.Contains(val))
                        {
                            poss.Remove(val);
                            continue;
                        }
                        else
                        {
                            guess.Add(val);
                            var move = new SolveNumberMove { Value = val, Row = numberI, Column = numberJ, Algorithm = this };
                            _engine.lastRandomMove = move;
                            return move;
                        }
                    }

                    _engine.WriteLog("All guesses were exhausted on " + _engine.CellFormat(numberI, numberJ));
                    return null;
                }
            }
        }

        private T GetRandom<T>(List<T> items)
        {
            int number = _random.Next(1, 100000) % items.Count;
            return items[number];
        }
    }
}
