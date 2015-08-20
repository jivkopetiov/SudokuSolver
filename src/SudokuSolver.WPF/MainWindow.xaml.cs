using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using SudokuSolver.Engine;
using System.Threading;

namespace SudokuSolver.WPF
{
    public partial class MainWindow : Window
    {
        private const int _defaultNumberOfCells = 30;

        private static readonly Brush _solvedColor = (Brush)new BrushConverter().ConvertFrom("#DEFDEF");
        private static readonly Brush _initialColor = (Brush)new BrushConverter().ConvertFrom("#FEDFED");
        private static readonly Brush _currentColor = (Brush)new BrushConverter().ConvertFrom("#DCADCA");

        private TextBox[,] boxes = new TextBox[9, 9];
        private Border[,] borders = new Border[9, 9];

        private SolverEngine engine;

        private DispatcherTimer _playTimer;
        private bool PlayInProgress;

        public MainWindow()
        {
            InitializeComponent();

            InitializeEngine();
            InitializeGrid();

            var puzzle = PredefinedPuzzles.GetPuzzleByGroupAndNumber("nakedpairs", 1);
            //var puzzle = PredefinedPuzzles.EmptyPuzzle;
            //var puzzle = PredefinedPuzzles.GetRandomPuzzle();
            engine.LoadPuzzle(puzzle);
            PuzzleName.Content = puzzle.Name;
            PopulateUIWithValues();
            DrawAllPossibles();
        }

        private void InitializeEngine()
        {
            engine = new SolverEngine();
            engine.WriteLog = (message) =>
            {
                WriteLog(message);
            };
        }

        private void WriteLog(string message)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                Log.Text += Environment.NewLine + message;
                Log.ScrollToEnd();
            }));
        }

        private void PopulateUIWithValues()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    int value = engine.values[i, j];
                    if (value > 0)
                    {
                        var border = borders[i, j];
                        var box = GetOrCreateSolvedBox(i, j);
                        border.Child = box;
                        box.Text = value.ToString();
                        box.Background = _initialColor;
                    }
                }
            }
        }

        private void DrawAllPossibles()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    int value = engine.values[i, j];
                    if (value == 0)
                    {
                        PopulatePossibles(i, j);
                    }
                }
            }
        }

        private void PopulatePossibles(int i, int j)
        {
            var border = borders[i, j];
            border.Child = null;
            border.Padding = new Thickness(1, 1, 1, 1);
            var canvas = new Grid();
            canvas.Background = Brushes.White;

            var poss = engine.possibles[i, j];
            foreach (int possible in poss)
            {
                var block = new TextBlock();
                block.FontSize = 15d;
                int left = 6 + (((possible - 1) % 3) * 15);
                int top = 2 + (((possible - 1) / 3) * 14);

                block.Padding = new Thickness(left, top, 0, 0);
                block.Text = possible.ToString();
                canvas.Children.Add(block);
            }

            border.Child = canvas;
        }

        private void InitializeGrid()
        {
            Grid.Children.Clear();
            //borders = new Border[9, 9];
            //boxes = new TextBox[9, 9];
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    int iCaptured = i;
                    int jCaptured = j;

                    var existing = borders[i, j];
                    if (existing != null)
                    {
                        existing.Child = null;
                        Grid.Children.Remove(existing);
                    }

                    var border = new Border();
                    border.Background = Brushes.LightGray;
                    Grid.SetRow(border, i);
                    Grid.SetColumn(border, j);
                    Grid.Children.Add(border);
                    borders[i, j] = border;
                }
            }
        }

        private TextBox GetOrCreateSolvedBox(int i, int j)
        {
            var box = boxes[i, j];
            if (box == null)
            {
                box = new TextBox();
                box.Padding = new Thickness(12, 5, 0, 0);
                box.IsReadOnly = true;
                box.FontSize = 28d;

                boxes[i, j] = box;
            }

            return box;
        }

        private TextBox _currentBox;

        private void NextMove_Click(object sender, RoutedEventArgs e)
        {
            var move = engine.NextMove();
            DrawMoveOnScreen(move);
        }

        private void DrawMoveOnScreen(Move move)
        {
            if (move != null)
            {
                if (move is SolveNumberMove)
                {
                    if (_currentBox != null)
                        _currentBox.Background = _solvedColor;

                    var box = GetOrCreateSolvedBox(move.Row, move.Column);
                    box.Text = move.Value.ToString();
                    box.Background = _currentColor;

                    var border = borders[move.Row, move.Column];
                    border.Child = GetOrCreateSolvedBox(move.Row, move.Column);

                    _currentBox = box;
                }
                else
                {
                }

                SetMoveCount();
            }

            DrawAllPossibles();
        }

        private void PrevMove_Click(object sender, RoutedEventArgs e)
        {
            PreviousMove();
        }

        private void PreviousMove()
        {
            var move = engine.UndoOneMove();
            if (move == null)
                return;

            Action uiOperations = () =>
            {
                if (move is SolveNumberMove)
                {
                    var box = boxes[move.Row, move.Column];
                    box.Text = "";
                }

                SetMoveCount();
                DrawAllPossibles();
            };

            if (IsInBackgroundThread())
                Dispatcher.Invoke(uiOperations);
            else
                uiOperations();
        }

        private void SetMoveCount()
        {
            MoveCount.Content = engine.MoveCount;
        }

        private void SetButtonImage(string imagePath)
        {
            string xaml = string.Format(
                @"<ControlTemplate TargetType=""{{x:Type Button}}""><Grid><Image Name=""Play"" Source=""{0}"" /></Grid></ControlTemplate>", imagePath);

            using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(xaml)))
            {
                ParserContext parserContext = new ParserContext();
                parserContext.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
                parserContext.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
                ControlTemplate template = (ControlTemplate)XamlReader.Load(stream, parserContext);
                Play.Template = template;
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (PlayInProgress && _playTimer != null)
            {
                _playTimer.Stop();
                PlayInProgress = false;
                SetButtonImage("Images/play.png");
            }
            else
            {
                SetButtonImage("Images/pause.png");

                _playTimer = new DispatcherTimer();
                _playTimer.Interval = TimeSpan.FromMilliseconds(500);
                _playTimer.Tick += delegate
                {
                    var move = engine.NextMove();
                    DrawMoveOnScreen(move);
                };

                _playTimer.Start();
                PlayInProgress = true;
            }
        }

        private void LoadNewPuzzle_Click(object sender, RoutedEventArgs e)
        {
            var puzzle = PredefinedPuzzles.GetRandomPuzzle();

            InitializeGrid();
            InitializeEngine();
            engine.LoadPuzzle(puzzle);
            PuzzleName.Content = puzzle.Name;
            SetMoveCount();
            PopulateUIWithValues();
            DrawAllPossibles();
        }

        private void GoToEnd_Click(object sender, RoutedEventArgs e)
        {
            var thread = new Thread(() =>
            {
                Move move = null;
                do
                {
                    move = engine.NextMove();
                    Dispatcher.Invoke(new Action(() =>
                    {
                        DrawMoveOnScreen(move);
                    }));
                    Thread.Sleep(10);
                }
                while (move != null);
            });

            thread.Start();
        }

        private void GoToStart_Click(object sender, RoutedEventArgs e)
        {
            var thread = new Thread(() =>
            {
                while (engine.MoveCount > 0)
                {
                    PreviousMove();
                    Thread.Sleep(100);
                }
            });

            thread.Start();
        }

        private bool IsInBackgroundThread()
        {
            return Dispatcher.CurrentDispatcher != this.Dispatcher;
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            string values = engine.CopyValues();
            Clipboard.SetText(values);
        }

        private void GenerateRandomPuzzle_Click(object sender, RoutedEventArgs e)
        {
            var empty = PredefinedPuzzles.EmptyPuzzle;
            engine.LoadPuzzle(empty);
            engine.TrySolveToEnd();
            engine.ClearXMoves(50);
            
            var puzzle = new Puzzle(engine.CopyValues()) { Name = "Random " + Guid.NewGuid().ToString() };

            InitializeEngine();
            InitializeGrid();

            engine.LoadPuzzle(puzzle);
            PuzzleName.Content = puzzle.Name;
            PopulateUIWithValues();
            DrawAllPossibles();
        }
    }
}
