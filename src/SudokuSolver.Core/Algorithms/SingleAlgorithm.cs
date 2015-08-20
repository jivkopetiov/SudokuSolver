using System.Linq;

namespace SudokuSolver.Engine
{
    public class SingleAlgorithm : Algorithm
    {
        public SingleAlgorithm(SolverEngine engine)
            : base(engine)
        {
        }

        public override Move Execute()
        {
            var values = _engine.values;
            var possibles = _engine.possibles;

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (values[i, j] > 0)
                        continue;

                    var array = possibles[i, j];
                    if (array.Count == 1)
                    {
                        var number = array.Single();

                        var move = new SolveNumberMove { Value = number, Row = i, Column = j, Algorithm = this };
                        return move;
                    }
                }
            }

            return null;
        }
    }
}
