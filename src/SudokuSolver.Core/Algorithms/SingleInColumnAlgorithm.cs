using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SudokuSolver.Engine
{
    public class SingleInColumnAlgorithm : Algorithm
    {
        public SingleInColumnAlgorithm(SolverEngine engine)
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

            for (int number = 1; number < 10; number++)
            {
                for (int j = 0; j < 9; j++)
                {
                    contains = false;
                    containsTwo = false;
                    indexContaining = 0;

                    for (int i = 0; i < 9; i++)
                    {
                        if (values[i, j] > 0)
                            continue;

                        if (possibles[i, j].Contains(number))
                        {
                            if (!contains)
                            {
                                contains = true;
                                indexContaining = i;
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
                        var move = new SolveNumberMove { Value = number, Row = indexContaining, Column = j, Algorithm = this };
                        return move;
                    }
                }
            }

            return null;
        }
    }
}
