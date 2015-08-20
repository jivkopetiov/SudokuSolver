using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SudokuSolver.Engine
{
    public class SolverEngine
    {
        private static readonly List<int> oneToNine = Enumerable.Range(1, 9).ToList();

        private static readonly Random _random = new Random();

        public Move lastRandomMove;

        public int[,] values = new int[9, 9];
        public List<int>[,] possibles = new List<int>[9, 9];
        public List<int>[,] guesses = new List<int>[9, 9];
        public Stack<Move> moves = new Stack<Move>();

        public int Undos;

        public Action<string> WriteLog = delegate { };

        public int MoveCount
        {
            get { return moves.Count; }
        }

        public SolverEngine()
        {
            AnnulAllPossibles();
        }

        public Move PerformSolveNumberMove(SolveNumberMove move)
        {
            string message = string.Format("Performing a move on {0} with val {1}", CellFormat(move.Row, move.Column), move.Value);
            if (move.Algorithm != null)
            {
                string algoName = move.Algorithm.GetType().Name;
                algoName = algoName.Replace("Algorithm", "");
                message += " using algo " + algoName;
            }

            WriteLog(message);
            values[move.Row, move.Column] = move.Value;
            move.Possibles = new List<int>(possibles[move.Row, move.Column]);
            moves.Push(move);
            return move;
        }

        public Move PerformReducePossibilitiesMove(ReducePossibilitiesMove move)
        {
            WriteLog("Reducing possibilities");
            moves.Push(move);
            return move;
        }

        private Move ExecuteAlgorithms()
        {
            var algorithms = new Algorithm[] 
            { 
                new RefreshPossiblesAlgorithm(this),
                new SingleAlgorithm(this),
                new SingleInRowAlgorithm(this),
                new SingleInColumnAlgorithm(this),
                new SingleInSquareAlgorithm(this),
                new NakedPairsInRowAlgorithm(this), 
                new NakedPairsInColumnAlgorithm(this),
                new NakedPairsInSquareAlgorithm(this),
                new NakedTriplesInRowAlgorithm(this),
                new NakedTriplesInColumnAlgorithm(this),
                new NakedTriplesInSquareAlgorithm(this),
                new HiddenPairsInRowAlgorithm(this),
                new HiddenPairsInSquareAlgorithm(this),
                new PointingPairsAlgorithm(this, Direction.Vertical),
                new PointingPairsAlgorithm(this, Direction.Horizontal),
                new RandomMoveAlgorithm(this)
            };

            foreach (var algo in algorithms)
            {
                var move = algo.Execute();
                if (move != null)
                {
                    if (move is SolveNumberMove)
                        return PerformSolveNumberMove((SolveNumberMove)move);
                    else if (move is ReducePossibilitiesMove)
                        return PerformReducePossibilitiesMove((ReducePossibilitiesMove)move);
                    else
                        throw new InvalidOperationException("Not supported move object");
                }
            }

            return null;
        }

        public bool IsFullySolved()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (values[i, j] == 0)
                        return false;
                }
            }

            return true;
        }

        private Move UndoLastRandomMove()
        {
            if (moves.Count == 0)
            {
                WriteLog("Cannot undo move, there is no more moves made");
                return null;
            }

            if (lastRandomMove == null)
            {
                WriteLog("cannot undo last random move, there are no registered random moves");
                return null;
            }

            while (moves.Count > 0)
            {
                var move = UndoOneMove();
                if (move == lastRandomMove)
                    return move;
            }

            WriteLog("Invalid condition. Failed to make an undo last random move, none of the previous moves was the same as the last registered random move");
            return null;
        }

        public Move NextMove()
        {
            if (IsFullySolved())
            {
                WriteLog("Completed the puzzle successfully");
                return null;
            }

            Move move = null;
            bool isValid = CheckForValidity();
            if (!isValid)
            {
                WriteLog("cannot perform a move, puzzle is in an invalid state");
                return UndoLastRandomMove();
            }

            move = ExecuteAlgorithms();

            if (move != null)
            {
                if (IsFullySolved())
                    WriteLog("Completed the puzzle successfully");

                return move;
            }
            else
            {
                WriteLog("Failed to find a move in all algorithms");
                return null;
            }
        }

        public long TrySolveToEnd()
        {
            var stopwatch = Stopwatch.StartNew();

            int maxRandomIterations = 500;
            int counter = 0;

            while (true)
            {
                if (counter > maxRandomIterations)
                {
                    WriteLog("Exceeded max random moves of 500. Interrupting");
                    break;
                }

                Move move = NextMove();

                if (move == null)
                    break;
            }

            stopwatch.Stop();

            return stopwatch.ElapsedMilliseconds;
        }

        public void AnnulAllPossibles()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    possibles[i, j] = new List<int>(oneToNine);
                }
            }
        }

        public void LoadPuzzle(Puzzle puzzle)
        {
            AnnulAllValues();
            AnnulAllPossibles();

            Array.Copy(puzzle.Values, values, puzzle.Values.Length);
        }

        private void AnnulAllValues()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    values[i, j] = 0;
                }
            }
        }

        private bool CheckForValidity()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (values[i, j] == 0 && possibles[i, j].Count == 0)
                    {
                        WriteLog(string.Format("Cell {0} has no possible values. Impossible to solve", CellFormat(i, j)));
                        return false;
                    }
                }
            }

            for (int c = 0; c < 9; c++)
            {
                var column = GetColumnArray(c);
                var groups = column.Where(r => r > 0).GroupBy(r => r).OrderBy(r => r.Key);
                if (groups.Any() && groups.Any(g => g.Count() > 1))
                {
                    var group = groups.First(g => g.Count() > 1);
                    WriteLog(string.Format("Duplicate value {0} in column {1}", group.Key, ColumnFormat(c)));
                    return false;
                }
            }

            for (int c = 0; c < 9; c++)
            {
                var row = GetRowArray(c);
                var groups = row.GroupBy(r => r).OrderBy(r => r.Key);
                var g = groups.FirstOrDefault();
                if (g.Key == 0)
                    g = groups.Skip(1).FirstOrDefault();

                if (g != null && g.Any() && g.Count() > 1)
                {
                    WriteLog(string.Format("Duplicate value {0} in row {1}", g.Key, RowFormat(c)));
                    return false;
                }
            }

            return true;
        }

        public string RowFormat(int i)
        {
            return ((char)(i + 65)).ToString();
        }

        public string ColumnFormat(int j)
        {
            return (j + 1).ToString();
        }

        public string CellFormat(int i, int j)
        {
            return string.Format("[{0},{1}]", RowFormat(i), ColumnFormat(j));
        }

        private int[] GetColumnArray(int column)
        {
            var array = new int[9];
            for (int counter = 0; counter < 9; counter++)
            {
                array[counter] = values[counter, column];
            }

            return array;
        }

        private int[] GetRowArray(int row)
        {
            var array = new int[9];
            for (int counter = 0; counter < 9; counter++)
            {
                array[counter] = values[row, counter];
            }

            return array;
        }

        public Move UndoOneMove()
        {
            if (moves.Count == 0)
                return null;

            var move = moves.Pop();
            Undos++;

            if (move is SolveNumberMove)
            {
                WriteLog(string.Format("Undo move of value {0} at {1}", move.Value, CellFormat(move.Row, move.Column)));
                values[move.Row, move.Column] = 0;
                var poss = ((SolveNumberMove)move).Possibles;
                var possiblesObject = possibles[move.Row, move.Column];
                possiblesObject.Clear();
                possiblesObject.AddRange(poss);
            }
            else if (move is ReducePossibilitiesMove)
            {
                WriteLog("Undo solve possibilities move");
                foreach (var reduced in ((ReducePossibilitiesMove)move).Reduced)
                {
                    possibles[reduced.Row, reduced.Column].Add(reduced.Value);
                }
            }
            else
                throw new InvalidOperationException("Not supported move object");

            return move;
        }

        public string CopyValues()
        {
            string result = "";

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    int val = values[i, j];
                    if (val > 0)
                        result += val;
                    else
                        result += "0";
                }

                result += " ";
            }

            result = result.Trim();

            return result;
        }

        public int GuessedMoves()
        {
            return moves.Count(m => m.Algorithm != null && m.Algorithm.GetType() == typeof(RandomMoveAlgorithm));
        }

        public void ClearXMoves(int countMoves)
        {
            while (countMoves > 0)
            {
                int row = _random.Next(100000000) % 9;
                int column = _random.Next(100000000) % 9;

                if (values[row, column] > 0)
                {
                    values[row, column] = 0;
                    countMoves--;
                }
            }
        }
    }
}
