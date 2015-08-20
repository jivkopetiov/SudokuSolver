using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SudokuSolver.Engine
{
    public class ReducePossibilitiesMove : Move
    {
        public List<Move> Reduced { get; set; }
    }
}
