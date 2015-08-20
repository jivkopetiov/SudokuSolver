using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SudokuSolver.Engine
{
    public class NakedPairsInRowAlgorithm : Algorithm
    {
        public NakedPairsInRowAlgorithm(SolverEngine engine)
            : base(engine)
        {
        }

        public override Move Execute()
        {
            var possiblesRemoved = new List<Move>();

            var possibles = _engine.possibles;
            var values = _engine.values;

            while (true)
            {
                bool performedNakedPairRemoval = false;

                for (int i = 0; i < 9; i++)
                {
                    var possiblesWithPairs = new List<int>();
                    for (int j = 0; j < 9; j++)
                    {
                        if (possibles[i, j].Count == 2)
                            possiblesWithPairs.Add(j);
                    }

                    if (possiblesWithPairs.Count < 2)
                        continue;

                    for (int a = 0; a < possiblesWithPairs.Count - 1; a++)
                    {
                        for (int b = 1; b < possiblesWithPairs.Count; b++)
                        {
                            if (a == b)
                                continue;

                            var a1 = possiblesWithPairs[a];
                            var b1 = possiblesWithPairs[b];

                            var first = possibles[i, a1];
                            var second = possibles[i, b1];

                            if (first[0] == second[0] && first[1] == second[1])
                            {

                                for (int j = 0; j < 9; j++)
                                {
                                    if (j == a1 || j == b1 || values[i, j] > 0)
                                        continue;

                                    var possible = possibles[i, j];

                                    if (possible.Contains(first[0]))
                                    {
                                        _engine.WriteLog(string.Format("performing naked pairs removal of {0} at {1}", first[0], _engine.CellFormat(i, j)));
                                        possible.Remove(first[0]);
                                        possiblesRemoved.Add(new Move { Row = i, Column = j, Value = first[0] });
                                        performedNakedPairRemoval = true;
                                    }

                                    if (possible.Contains(first[1]))
                                    {
                                        _engine.WriteLog(string.Format("performing naked pairs removal of {0} at {1}", first[1], _engine.CellFormat(i, j)));
                                        possible.Remove(first[1]);
                                        possiblesRemoved.Add(new Move { Row = i, Column = j, Value = first[0] });
                                        performedNakedPairRemoval = true;
                                    }
                                }
                            }
                        }
                    }
                }

                if (!performedNakedPairRemoval)
                {
                    if (possiblesRemoved.Any())
                        return new ReducePossibilitiesMove { Reduced = possiblesRemoved };
                    else
                        return null;
                }
            }
        }
    }
}
