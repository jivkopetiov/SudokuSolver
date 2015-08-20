using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SudokuSolver.Engine
{
    public class SingleInSquareAlgorithm : Algorithm
    {
        public SingleInSquareAlgorithm(SolverEngine engine)
            : base(engine)
        {
        }

        public override Move Execute()
        {
            var possibles = _engine.possibles;
            var values = _engine.values;

            bool contains = false;
            bool containsTwo = false;

            for (int number = 1; number < 10; number++)
            {
                for (int squareIndex = 0; squareIndex < 9; squareIndex++)
                {
                    int minX = (squareIndex / 3) * 3;
                    int maxX = minX + 3;

                    int minY = (squareIndex % 3) * 3;
                    int maxY = minY + 3;

                    contains = false;
                    containsTwo = false;
                    int indexI = 0;
                    int indexJ = 0;

                    for (int i = minX; i < maxX; i++)
                    {
                        for (int j = minY; j < maxY; j++)
                        {
                            if (values[i, j] > 0)
                                continue;

                            if (possibles[i, j].Contains(number))
                            {
                                if (!contains)
                                {
                                    contains = true;
                                    indexI = i;
                                    indexJ = j;
                                }
                                else
                                {
                                    containsTwo = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (contains && !containsTwo)
                    {
                        var move = new SolveNumberMove { Value = number, Row = indexI, Column = indexJ, Algorithm = this };
                        return move;
                    }
                }
            }

            return null;
        }
    }
}
