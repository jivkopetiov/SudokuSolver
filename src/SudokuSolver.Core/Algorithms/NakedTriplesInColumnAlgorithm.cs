using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SudokuSolver.Engine
{
    public class NakedTriplesInColumnAlgorithm : Algorithm
    {
        public NakedTriplesInColumnAlgorithm(SolverEngine engine)
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
                    var possiblesWithTriples = new List<int>();
                    for (int i = 0; i < 9; i++)
                    {
                        int count = possibles[i, j].Count;
                        if (count == 3 || count == 2)
                            possiblesWithTriples.Add(i);
                    }

                    if (possiblesWithTriples.Count < 3)
                        continue;

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

                                if (a1 == b1 || a1 == c1 || b1 == c1)
                                    continue;

                                var first = possibles[a1, j];
                                var second = possibles[b1, j];
                                var third = possibles[c1, j];

                                var groups = first.Concat(second).Concat(third).GroupBy(g => g);
                                if (groups.Count() == 3)
                                {
                                    for (int i = 0; i < 9; i++)
                                    {
                                        if (i == a1 || i == b1 || i == c1 || values[i, j] > 0)
                                            continue;

                                        var possible = possibles[i, j];

                                        int number = groups.First().Key;
                                        if (possible.Contains(number))
                                        {
                                            possible.Remove(number);
                                            possiblesRemoved.Add(new Move { Row = i, Column = j, Value = number });
                                            _engine.WriteLog(string.Format("performing naked triples in column removal of value {0} at {1}", number, _engine.CellFormat(i, j)));
                                            performedNakedPairRemoval = true;
                                            goto beforeend;
                                        }

                                        number = groups.Skip(1).First().Key;
                                        if (possible.Contains(number))
                                        {
                                            possible.Remove(number);
                                            possiblesRemoved.Add(new Move { Row = i, Column = j, Value = number });
                                            _engine.WriteLog(string.Format("performing naked triples in column removal of value {0} at {1}", number, _engine.CellFormat(i, j)));
                                            performedNakedPairRemoval = true;
                                            goto beforeend;
                                        }

                                        number = groups.Skip(2).First().Key;
                                        if (possible.Contains(number))
                                        {
                                            possible.Remove(number);
                                            possiblesRemoved.Add(new Move { Row = i, Column = j, Value = number });
                                            _engine.WriteLog(string.Format("performing naked triples in column removal of value {0} at {1}", number, _engine.CellFormat(i, j)));
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
