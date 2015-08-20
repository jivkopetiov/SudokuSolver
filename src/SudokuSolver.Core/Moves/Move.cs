
namespace SudokuSolver.Engine
{
    public class Move
    {
        public int Value { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public Algorithm Algorithm { get; set; }
    }
}
