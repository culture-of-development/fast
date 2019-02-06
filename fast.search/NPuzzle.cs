using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace fast.search
{
    public class NPuzzle : IEquatable<NPuzzle>
    {
        public const int StepCost = 1;

        public int NRows { get; private set; }
        public int NCols { get; private set; }
        private int totalCells;

        const int valueBits = 4;
        private ulong board;
        private ulong goal;
        private int blankRow;
        private int blankCol;

        public int this[int row, int col]
        {
            get {
                ulong tileValue = (this.board >> (row * valueBits * this.NCols) + (col * valueBits)) & 0xFUL;
                return (int)tileValue;
            }
        }

        private NPuzzle()
        {
            // to allow us to copy without initialization
            // TODO: this is sort of an anti-pattern because there is no initialization, have to check everywhere everytime we update
        }

        public NPuzzle(int nrows, int ncols)
        {
            if (nrows*ncols > 16) throw new ArgumentException($"The maximum number of tiles is 16, but {nrows*ncols} were requested.");
            this.NRows = nrows;
            this.NCols = ncols;
            this.totalCells = nrows * ncols;
            this.goal = GenerateGoalState(nrows, ncols);
            this.board = this.goal;
            this.blankCol = nrows-1;
            this.blankRow = ncols-1;
        }

        public NPuzzle(int nrows, int ncols, string initial)
            : this(nrows, ncols)
        {
            // TODO: validate that all expected numbers are present
            var values = initial.Replace(",", "").Split(' ')
                .Select(m => ulong.Parse(m))
                .ToArray();

            this.board = 0UL;
            for(int i = values.Length-1; i >= 0; i--)
            {
                ulong value = values[i];
                this.board = this.board << valueBits;
                this.board |= value;
                if (value == 0UL)
                {
                    this.blankRow = (int)i / ncols;
                    this.blankCol = (int)i % ncols;
                }
            }
        }

        private static ulong GenerateGoalState(int nrows, int ncols)
        {
            ulong maxTileValue = (ulong)(nrows * ncols - 1);
            ulong result = 0UL;
            for(ulong i = maxTileValue; i > 0; i--)
            {
                result = result << valueBits;
                result |= i & 0xFUL;
            }
            return result;
        }

        public List<Location> ExpandMoves()
        {
            var successors = new List<Location>(4);
            // up
            if (this.blankRow > 0)
            {
                successors.Add(Location.Create(this.blankRow - 1, this.blankCol));
            }
            // down
            if (this.blankRow < (this.NRows - 1))
            {
                successors.Add(Location.Create(this.blankRow + 1, this.blankCol));
            }
            // left
            if (this.blankCol > 0)
            {
                successors.Add(Location.Create(this.blankRow, this.blankCol - 1));
            }
            // right
            if (this.blankCol < (this.NCols - 1))
            {
                successors.Add(Location.Create(this.blankRow, this.blankCol + 1));
            }
            return successors;
        }

        // TODO: return the cost of the action with the new state
        public void Move(Location newBlankLocation)
        {
            int newBlankOffset = newBlankLocation.Row * valueBits * this.NCols + newBlankLocation.Col * valueBits;
            int oldBlankOffset = this.blankRow * valueBits * this.NCols + this.blankCol * valueBits;

            ulong val = (this.board >> newBlankOffset) & 0xFUL;
            this.board &= ~(0xFUL << newBlankOffset);
            this.board |= val << oldBlankOffset;
            this.blankRow = newBlankLocation.Row;
            this.blankCol = newBlankLocation.Col;
        }

        // TODO: return the cost of the action with the new state
        public NPuzzle MoveCopy(Location newBlankLocation)
        {
            var copy = new NPuzzle { 
                NRows = this.NRows, 
                NCols = this.NCols,
                totalCells = this.totalCells,
                goal = this.goal,
                board = this.board, 
                blankCol = this.blankCol, 
                blankRow = this.blankRow,
            };
            copy.Move(newBlankLocation);
            return copy;
        }

        // TODO: move this elsewhere but right now the state is private
        // The Hamming distance in this case is the number of misplaced tiles
        public static int HammingDistance(NPuzzle state)
        {
            int cost = 0;
            ulong expected = 1UL;
            ulong uCells = (ulong)state.totalCells;
            var value = state.board;
            for(int i = 0; i < state.NRows; i++)
            for(int j = 0; j < state.NCols; j++)
            {
                ulong tileValue = value & 0xFUL;
                if (tileValue != 0UL)
                {
                    cost += tileValue == expected ? 0 : 1;
                }
                value = value >> valueBits;
                expected = (++expected) % uCells;
            }
            return cost;
        }
        public static int ManhattanDistance(NPuzzle state)
        {
            int cost = 0;
            ulong uCells = (ulong)state.totalCells;
            var value = state.board;
            for(int i = 0; i < state.NRows; i++)
            for(int j = 0; j < state.NCols; j++)
            {
                // the addition of state.totalCells here is to ensure the value is always positive
                // and ensure the remainder function works as intended
                int tileValue = (int)(value & 0xFUL);
                if (tileValue != 0)
                {
                    int expectedCellLocation = (tileValue - 1 + state.totalCells) % state.totalCells;
                    int expectedRow = expectedCellLocation / state.NCols;
                    int expectedCol = expectedCellLocation % state.NCols;
                    cost += Math.Abs(i - expectedRow) + Math.Abs(j - expectedCol);
                }
                value = value >> valueBits;
            }
            return cost;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var value = this.board;
            for(int i = 0; i < this.NRows; i++)
            {
                if (i > 0) sb.AppendLine();
                for(int j = 0; j < this.NCols; j++)
                {
                    sb.AppendFormat("{0,2} ", value & 0xFUL);
                    value = value >> valueBits;
                }
            }
            return sb.ToString();
        }

        // these have to be implemented so that the sets can correctly dedupe
        public override int GetHashCode()
        {
            // the only meaningful part of this object is the board representation
            // the rest is only there to help us perform other ops faster
            return board.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return Equals((NPuzzle)obj);
        }

        public bool IsGoal()
        {
            return this.goal == this.board;
        }

        public bool Equals(NPuzzle other)
        {
            return this.board == other.board;
        }

        public class Location
        {
            public int Row { get; private set; }
            public int Col { get; private set; }

            private Location(int row, int col)
            {
                this.Row = row;
                this.Col = col;
            }

            public static Location Create(int row, int col)
            {
                return new Location(row, col);
            }

            public override string ToString()
            {
                return $"({Row}, {Col})";
            }
        }
    }
}