using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolver.Engine
{
    public class NakedTriplesInSquareAlgorithm : Algorithm
    {
        public NakedTriplesInSquareAlgorithm(SolverEngine engine)
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

                    var possiblesWithTriples = new List<Tuple<int, int>>();

                    for (int i = minX; i < maxX; i++)
                    {
                        for (int j = minY; j < maxY; j++)
                        {
                            if (values[i, j] > 0)
                                continue;

                            int count = possibles[i, j].Count;
                            if (count == 2 || count == 3)
                                possiblesWithTriples.Add(Tuple.Create(i, j));
                        }
                    }

                    for (int a = 0; a < possiblesWithTriples.Count - 2; a++)
                    {
                        for (int b = 1; b < possiblesWithTriples.Count - 1; b++)
                        {
                            for (int c = 2; c < possiblesWithTriples.Count; c++)
                            {

                                if (a == b || a == c || b == c)
                                    continue;

                                var a1 = possiblesWithTriples[a];
                                var b1 = possiblesWithTriples[b];
                                var c1 = possiblesWithTriples[c];

                                if ((a1.Item1 == b1.Item1 && a1.Item2 == b1.Item2) ||
                                    (a1.Item1 == c1.Item1 && a1.Item2 == c1.Item2) ||
                                    (b1.Item1 == c1.Item1 && b1.Item2 == c1.Item2))
                                    continue;

                                var first = possibles[a1.Item1, a1.Item2];
                                var second = possibles[b1.Item1, b1.Item2];
                                var third = possibles[c1.Item1, c1.Item2];

                                var groups = first.Concat(second).Concat(third).GroupBy(g => g);

                                if (groups.Count() == 3)
                                {
                                    for (int i = minX; i < maxX; i++)
                                    {
                                        for (int j = minY; j < maxY; j++)
                                        {
                                            if (values[i, j] > 0)
                                                continue;

                                            if ((i == a1.Item1 && j == a1.Item2) || 
                                                (i == b1.Item1 && j == b1.Item2) ||
                                                (i == c1.Item1 && j == c1.Item2))
                                                continue;

                                            var possible = possibles[i, j];

                                            var number = groups.First().Key;
                                            if (possible.Contains(number))
                                            {
                                                possible.Remove(number);
                                                possiblesRemoved.Add(new Move { Row = i, Column = j, Value = number });
                                                _engine.WriteLog(string.Format("performing naked triples in square removal of value {0} at {1}", number, _engine.CellFormat(i, j)));
                                                performedNakedPairRemoval = true;
                                                goto beforeend;
                                            }

                                            number = groups.Skip(1).First().Key;
                                            if (possible.Contains(number))
                                            {
                                                possible.Remove(number);
                                                possiblesRemoved.Add(new Move { Row = i, Column = j, Value = number });
                                                _engine.WriteLog(string.Format("performing naked triples in square removal of value {0} at {1}", number, _engine.CellFormat(i, j)));
                                                performedNakedPairRemoval = true;
                                                goto beforeend;
                                            }

                                            number = groups.Skip(2).First().Key;
                                            if (possible.Contains(number))
                                            {
                                                possible.Remove(number);
                                                possiblesRemoved.Add(new Move { Row = i, Column = j, Value = number });
                                                _engine.WriteLog(string.Format("performing naked triples in square removal of value {0} at {1}", number, _engine.CellFormat(i, j)));
                                                performedNakedPairRemoval = true;
                                                goto beforeend;
                                            }
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
