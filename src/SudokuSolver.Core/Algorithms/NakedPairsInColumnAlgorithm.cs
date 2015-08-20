using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SudokuSolver.Engine
{
    public class NakedPairsInColumnAlgorithm : Algorithm
    {
        public NakedPairsInColumnAlgorithm(SolverEngine engine)
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

                for (int j = 0; j < 9; j++)
                {
                    var possiblesWithPairs = new List<int>();
                    for (int i = 0; i < 9; i++)
                    {
                        if (possibles[i, j].Count == 2)
                            possiblesWithPairs.Add(i);
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

                            if (a1 == b1)
                                continue;

                            var first = possibles[a1, j];
                            var second = possibles[b1, j];

                            if (first[0] == second[0] && first[1] == second[1])
                            {
                                for (int i = 0; i < 9; i++)
                                {
                                    if (i == a1 || i == b1 || values[i, j] > 0)
                                        continue;

                                    var possible = possibles[i, j];

                                    if (possible.Contains(first[0]))
                                    {
                                        possible.Remove(first[0]);
                                        possiblesRemoved.Add(new Move { Row = i, Column = j, Value = first[0] });
                                        _engine.WriteLog(string.Format("performing naked pairs vertical removal of value {0} at {1}", first[0], _engine.CellFormat(i, j)));
                                        performedNakedPairRemoval = true;
                                        goto beforeend;
                                    }

                                    if (possible.Contains(first[1]))
                                    {
                                        possible.Remove(first[1]);
                                        possiblesRemoved.Add(new Move { Row = i, Column = j, Value = first[0] });
                                        _engine.WriteLog(string.Format("performing naked pairs vertical removal of value {0} at {1}", first[1], _engine.CellFormat(i, j)));
                                        performedNakedPairRemoval = true;
                                        goto beforeend;
                                    }
                                }
                            }
                        }
                    }
                }

            beforeend:
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
