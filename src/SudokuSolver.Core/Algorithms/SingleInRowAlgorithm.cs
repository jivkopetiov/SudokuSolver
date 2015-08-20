using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SudokuSolver.Engine
{
    public class SingleInRowAlgorithm : Algorithm
    {
        public SingleInRowAlgorithm(SolverEngine engine)
            : base(engine)
        {
        }

        public override Move Execute()
        {
            var possibles = _engine.possibles;
            var values = _engine.values;

            bool contains = false;
            bool containsTwo = false;
            int indexContaining = 0;

            // Check for a single possibility in row
            for (int number = 1; number < 10; number++)
            {
                for (int i = 0; i < 9; i++)
                {
                    contains = false;
                    containsTwo = false;
                    indexContaining = 0;

                    for (int j = 0; j < 9; j++)
                    {
                        if (values[i, j] > 0)
                            continue;

                        if (possibles[i, j].Contains(number))
                        {
                            if (!contains)
                            {
                                contains = true;
                                indexContaining = j;
                            }
                            else
                            {
                                containsTwo = true;
                                break;
                            }
                        }
                    }

                    if (contains && !containsTwo)
                    {
                        var move = new SolveNumberMove { Value = number, Row = i, Column = indexContaining, Algorithm = this };
                        return move;
                    }
                }
            }

            return null;
        }
    }
}
