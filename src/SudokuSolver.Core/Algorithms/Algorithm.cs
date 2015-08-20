using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SudokuSolver.Engine
{
    public class Algorithm
    {
        protected SolverEngine _engine;

        public Algorithm(SolverEngine engine)
        {
            _engine = engine;
        }

        public virtual Move Execute() { return null; }
    }
}
