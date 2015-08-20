using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SudokuSolver.Engine
{
    public class RefreshPossiblesAlgorithm : Algorithm
    {
        public RefreshPossiblesAlgorithm(SolverEngine engine)
            : base(engine)
        {
        }

        public override Move Execute()
        {
            var reduced = new List<Move>();

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (_engine.values[i, j] > 0)
                        continue;

                    CheckRowColumnAndSquarePossibilities(i, j, _engine.possibles[i, j], reduced);
                }
            }

            if (reduced.Any())
            {
                var move = new ReducePossibilitiesMove();
                move.Reduced = reduced;
                return move;
            }
            else
            {
                return null;
            }
        }


        private void CheckRowColumnAndSquarePossibilities(int numberI, int numberJ, List<int> poss, List<Move> reduced)
        {
            var values = _engine.values;

            for (int c = 0; c < 9; c++)
            {
                if (c == numberJ)
                    continue;

                int cellVal = values[numberI, c];
                if (poss.Contains(cellVal))
                {
                    poss.Remove(cellVal);
                    reduced.Add(new Move() { Row = numberI, Column = numberJ, Value = cellVal });
                }
            }

            for (int c = 0; c < 9; c++)
            {
                if (c == numberI)
                    continue;

                int cellVal = values[c, numberJ];
                if (poss.Contains(cellVal))
                {
                    poss.Remove(cellVal);
                    reduced.Add(new Move() { Row = numberI, Column = numberJ, Value = cellVal });
                }
            }

            int iStep = numberI - (numberI % 3);
            int jStep = numberJ - (numberJ % 3);

            for (int i = iStep; i < iStep + 3; i++)
            {
                for (int j = jStep; j < jStep + 3; j++)
                {
                    int cellVal = values[i, j];

                    if (i == numberI && j == numberJ)
                        continue;

                    if (poss.Contains(cellVal))
                    {
                        poss.Remove(cellVal);
                        reduced.Add(new Move() { Row = numberI, Column = numberJ, Value = cellVal });
                    }
                }
            }
        }
    }
}
