using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SudokuSolver.Engine
{
    public class HiddenPairsInRowAlgorithm : Algorithm
    {
        public HiddenPairsInRowAlgorithm(SolverEngine engine)
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
                for (int i = 0; i < 9; i++)
                {
                    var dict = new Dictionary<int, int>();
                    var indexes = new Dictionary<int, List<int>>();

                    for (int j = 0; j < 9; j++)
                    {
                        if (values[i, j] > 0) continue;

                        foreach (int number in possibles[i, j])
                        {
                            if (!dict.ContainsKey(number))
                                dict[number] = 1;
                            else
                                dict[number]++;

                            if (!indexes.ContainsKey(number))
                                indexes[number] = new List<int> { j };
                            else
                                indexes[number].Add(j);
                        }
                    }

                    var numbers = dict.Keys.Where(k => dict[k] == 2).ToList();

                    if (numbers.Count < 2)
                        continue;

                    for (int k = 0; k < numbers.Count; k++)
                    {
                        var first = numbers[k];

                        for (int j = k + 1; j < numbers.Count; j++)
                        {
                            var second = numbers[j];
                            var firstIndex = indexes[first];
                            firstIndex.Sort();
                            var secondIndex = indexes[second];
                            secondIndex.Sort();

                            if (BclEx.AreEqualSequences(firstIndex, secondIndex))
                            {
                                int indexX = i;
                                int indexY = firstIndex[0];
                                var p = possibles[indexX, indexY];
                                var possiblesToRemove = new List<int>();
                                foreach (int number in p)
                                {
                                    if (number != first && number != second)
                                    {
                                        possiblesToRemove.Add(number);
                                        possiblesRemoved.Add(new Move { Row = indexX, Column = indexY, Value = number });
                                        _engine.WriteLog(string.Format("performing hidden pairs in row removal of value {0} at {1}", number, _engine.CellFormat(indexX, indexY)));
                                    }
                                }
                                foreach (var n in possiblesToRemove)
                                    p.Remove(n);

                                indexX = i;
                                indexY = firstIndex[1];
                                p = possibles[indexX, indexY];
                                foreach (int number in p)
                                {
                                    if (number != first && number != second)
                                    {
                                        possiblesToRemove.Add(number);
                                        possiblesRemoved.Add(new Move { Row = indexX, Column = indexY, Value = number });
                                        _engine.WriteLog(string.Format("performing hidden pairs in row removal of value {0} at {1}", number, _engine.CellFormat(indexX, indexY)));
                                    }
                                }
                                foreach (var n in possiblesToRemove)
                                    p.Remove(n);

                                goto beforeend;
                            }
                        }
                    }
                }

            beforeend:
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
