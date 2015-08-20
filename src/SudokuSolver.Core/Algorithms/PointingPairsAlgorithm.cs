using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuSolver.Engine
{
    public enum Direction
    {
        Horizontal,
        Vertical
    }

    public class PointingPairsAlgorithm : Algorithm
    {
        private Direction _direction;

        public PointingPairsAlgorithm(SolverEngine engine, Direction direction)
            : base(engine)
        {
            _direction = direction;
        }

        public override Move Execute()
        {
            var possiblesRemoved = new List<Move>();

            var possibles = _engine.possibles;
            var values = _engine.values;

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

                var numbers = dict.Keys.Where(k => dict[k] == 2 || dict[k] == 3).ToList();

                foreach (var number in numbers)
                {
                    var allInd = indexes[number];
                    int groupCount = 0;
                    if (_direction == Direction.Vertical)
                        groupCount = allInd.GroupBy(i => i % 10).Count();
                    else
                        groupCount = allInd.GroupBy(i => i / 10).Count();

                    if (groupCount == 1)
                    {
                        if (_direction == Direction.Vertical)
                        {
                            int column = allInd[0] % 10;
                            var existingIndexes = allInd.Select(a => a / 10).ToList();

                            for (int row = 0; row < 9; row++)
                            {
                                var p = possibles[row, column];
                                if (p.Contains(number) &&
                                    !existingIndexes.Contains(row) &&
                                    values[row, column] == 0)
                                {
                                    PerformRemove(possiblesRemoved, number, column, row, p);
                                }
                            }
                        }
                        else
                        {
                            int row = allInd[0] / 10;
                            var existingIndexes = allInd.Select(a => a % 10).ToList();

                            for (int column = 0; column < 9; column++)
                            {
                                var p = possibles[row, column];
                                if (p.Contains(number) &&
                                    !existingIndexes.Contains(column) &&
                                    values[row, column] == 0)
                                {
                                    PerformRemove(possiblesRemoved, number, column, row, p);
                                }
                            }
                        }
                    }
                }
            }

            if (possiblesRemoved.Any())
                return new ReducePossibilitiesMove { Reduced = possiblesRemoved };
            else
                return null;
        }

        private void PerformRemove(List<Move> possiblesRemoved, int number, int column, int row, List<int> p)
        {
            p.Remove(number);
            possiblesRemoved.Add(new Move { Row = row, Column = column, Value = number });
            _engine.WriteLog(string.Format("performing pointing pairs vertical removal of value {0} at {1}", number, _engine.CellFormat(row, column)));
        }
    }
}
