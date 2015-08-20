using SudokuSolver.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SudokuSolver.WinRT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private const int _defaultNumberOfCells = 30;

        public static SolidColorBrush GetColorFromHexa(string hexaColor)
        {
            return new SolidColorBrush(
                Color.FromArgb(
                    Convert.ToByte(hexaColor.Substring(1, 2), 16),
                    Convert.ToByte(hexaColor.Substring(3, 2), 16),
                    Convert.ToByte(hexaColor.Substring(5, 2), 16),
                    Convert.ToByte(hexaColor.Substring(7, 2), 16)
                )
            );
        }

        private static readonly Brush _solvedColor = new SolidColorBrush(Colors.Blue); // GetColorFromHexa("#DEFDEF");
        private static readonly Brush _initialColor = new SolidColorBrush(Colors.Red); // GetColorFromHexa("#FEDFED");
        private static readonly Brush _currentColor = new SolidColorBrush(Colors.Purple); // GetColorFromHexa("#DCADCA");

        private TextBox[,] boxes = new TextBox[9, 9];
        private Border[,] borders = new Border[9, 9];

        private SolverEngine engine;

        private DispatcherTimer _playTimer;
        private bool PlayInProgress;

        public MainPage()
        {
            InitializeComponent();

            InitializeEngine();
            InitializeGrid();

            //var puzzle = PredefinedPuzzles.GetPuzzleByGroupAndNumber("extremesudoku.info", 3);
            var puzzle = PredefinedPuzzles.EmptyPuzzle;
            //var puzzle = PredefinedPuzzles.GetRandomPuzzle();
            engine.LoadPuzzle(puzzle);
            PuzzleName.Text = puzzle.Name;
            PopulateUIWithValues();
            DrawAllPossibles();
        }

        private void InitializeEngine()
        {
            engine = new SolverEngine();
            engine.WriteLog = (message) =>
            {
                Log.Text += message + Environment.NewLine;
            };
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
            canvas.Background = new SolidColorBrush(Colors.RosyBrown);

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
                    border.Background = new SolidColorBrush(Colors.LightGray);
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

            uiOperations();
            //Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, delegate { uiOperations);
        }

        private void SetMoveCount()
        {
            MoveCount.Text = engine.MoveCount.ToString();
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (PlayInProgress && _playTimer != null)
            {
                _playTimer.Stop();
                PlayInProgress = false;
                Play.Visibility = Windows.UI.Xaml.Visibility.Visible;
                Pause.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                Play.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                Pause.Visibility = Windows.UI.Xaml.Visibility.Visible;

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
            PuzzleName.Text = puzzle.Name;
            SetMoveCount();
            PopulateUIWithValues();
            DrawAllPossibles();
        }

        private void GoToEnd_Click(object sender, RoutedEventArgs e)
        {
            ThreadPool.RunAsync(delegate
            {
                Move move = null;
                do
                {
                    move = engine.NextMove();

                    //Dispatcher.Invoke(new Action(() =>
                    //{
                    DrawMoveOnScreen(move);
                    //}));
                }
                while (move != null);
            });
        }

        private void GoToStart_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(delegate
            {
                while (engine.MoveCount > 0)
                {
                    PreviousMove();
                }
            }));
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            string values = engine.CopyValues();
            //Clipboard.SetContent(new DataPackage() { Properties =  values);
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
            PuzzleName.Text = puzzle.Name;
            PopulateUIWithValues();
            DrawAllPossibles();
        }
    }
}
