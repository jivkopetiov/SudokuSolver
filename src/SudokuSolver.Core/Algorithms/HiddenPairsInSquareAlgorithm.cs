using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolver.Engine
{
    public class HiddenPairsInSquareAlgorithm : Algorithm
    {
        public HiddenPairsInSquareAlgorithm(SolverEngine engine)
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
                for (int squareIndex = 0; squareIndex < 9; squareIndex++)
                {
                    int minX = (squareIndex / 3) * 3;
                    int maxX = minX + 3;

                    int minY = (squareIndex % 3) * 3;
                    int maxY = minY + 3;

                    var dict = new Dictionary<int, int>();
                    var indexes = new Dictionary<int, List<int>>();

                    for (int i = minX; i < maxX; i++)
                    {
                        for (int j = minY; j < maxY; j++)
                        {
                            if (values[i, j] > 0)
                                continue;

                            foreach (int number in possibles[i, j])
                            {
                                if (!dict.ContainsKey(number))
                                    dict[number] = 1;
                                else
                                    dict[number]++;

                                int index = (i * 10 + j);
                                if (!indexes.ContainsKey(number))
                                    indexes[number] = new List<int> { index };
                                else
                                    indexes[number].Add(index);
                            }
                        }
                    }

                    var numbers = dict.Keys.Where(k => dict[k] == 2).ToList();

                    if (numbers.Count < 2)
                        continue;

                    for (int i = 0; i < numbers.Count; i++)
                    {
                        var first = numbers[i];

                        for (int j = i + 1; j < numbers.Count; j++)
                        {
                            var second = numbers[j];
                            var firstIndex = indexes[first];
                            firstIndex.Sort();
                            var secondIndex = indexes[second];
                            secondIndex.Sort();

                            if (BclEx.AreEqualSequences(firstIndex, secondIndex))
                            {
                                int firstNumberX = firstIndex[0] / 10;
                                int firstNumberY = firstIndex[0] % 10;
                                int secondNumberX = firstIndex[1] / 10;
                                int secondNumberY = firstIndex[1] % 10;

                                var p = possibles[firstNumberX, firstNumberY];
                                var possiblesToRemove = new List<int>();
                                foreach (int number in p)
                                {
                                    if (number != first && number != second)
                                    {
                                        possiblesToRemove.Add(number);
                                        possiblesRemoved.Add(new Move { Row = firstNumberX, Column = firstNumberY, Value = number });
                                        _engine.WriteLog(string.Format("performing naked pairs square removal of value {0} at {1}", number, _engine.CellFormat(firstNumberX, firstNumberY)));
                                    }
                                }
                                foreach (var n in possiblesToRemove)
                                    p.Remove(n);

                                p = possibles[secondNumberX, secondNumberY];
                                possiblesToRemove = new List<int>();
                                foreach (int number in p)
                                {
                                    if (number != first && number != second)
                                    {
                                        possiblesToRemove.Add(number);
                                        possiblesRemoved.Add(new Move { Row = secondNumberX, Column = secondNumberY, Value = number });
                                        _engine.WriteLog(string.Format("performing naked pairs square removal of value {0} at {1}", number, _engine.CellFormat(secondNumberX, secondNumberY)));
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
