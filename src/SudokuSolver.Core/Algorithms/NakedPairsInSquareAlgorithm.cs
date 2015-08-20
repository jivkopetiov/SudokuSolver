using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolver.Engine
{
    public class NakedPairsInSquareAlgorithm : Algorithm
    {
        public NakedPairsInSquareAlgorithm(SolverEngine engine)
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

                for (int squareIndex = 0; squareIndex < 9; squareIndex++)
                {
                    int minX = (squareIndex / 3) * 3;
                    int maxX = minX + 3;

                    int minY = (squareIndex % 3) * 3;
                    int maxY = minY + 3;

                    var possiblesWithPairs = new List<Tuple<int, int>>();

                    for (int i = minX; i < maxX; i++)
                    {
                        for (int j = minY; j < maxY; j++)
                        {
                            if (values[i, j] > 0)
                                continue;

                            if (possibles[i, j].Count == 2)
                                possiblesWithPairs.Add(Tuple.Create(i, j));
                        }
                    }

                    for (int a = 0; a < possiblesWithPairs.Count - 1; a++)
                    {
                        for (int b = 1; b < possiblesWithPairs.Count; b++)
                        {
                            if (a == b)
                                continue;

                            var a1 = possiblesWithPairs[a];
                            var b1 = possiblesWithPairs[b];

                            if (a1.Item1 == b1.Item1 && a1.Item2 == b1.Item2)
                                continue;

                            var first = possibles[a1.Item1, a1.Item2];
                            var second = possibles[b1.Item1, b1.Item2];

                            if (first[0] == second[0] && first[1] == second[1])
                            {
                                for (int i = minX; i < maxX; i++)
                                {
                                    for (int j = minY; j < maxY; j++)
                                    {
                                        if (values[i, j] > 0)
                                            continue;

                                        if ((i == a1.Item1 && j == a1.Item2) || (i == b1.Item1 && j == b1.Item2))
                                            continue;

                                        var possible = possibles[i, j];

                                        if (possible.Contains(first[0]))
                                        {
                                            possible.Remove(first[0]);
                                            possiblesRemoved.Add(new Move { Row = i, Column = j, Value = first[0] });
                                            _engine.WriteLog(string.Format("performing naked pairs square removal of value {0} at {1}", first[0], _engine.CellFormat(i, j)));
                                            performedNakedPairRemoval = true;
                                            goto beforeend;
                                        }

                                        if (possible.Contains(first[1]))
                                        {
                                            possible.Remove(first[1]);
                                            possiblesRemoved.Add(new Move { Row = i, Column = j, Value = first[0] });
                                            _engine.WriteLog(string.Format("performing naked pairs square removal of value {0} at {1}", first[1], _engine.CellFormat(i, j)));
                                            performedNakedPairRemoval = true;
                                            goto beforeend;
                                        }
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
