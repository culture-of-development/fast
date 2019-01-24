using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace search_problems
{
    public class NPuzzle
    {
        public const int StepCost = 1;

        public int N { get; private set; }

        private byte[,] board;
        private int blankRow;
        private int blankCol;

        private NPuzzle()
        {
            // to allow us to copy without initialization
        }

        public NPuzzle(int n)
        {
            this.N = n;
            this.board = Initialize(n);
            this.blankCol = 0;
            this.blankRow = 0;
        }

        public NPuzzle(int n, string initial)
            : this(n)
        {
            var values = initial.Split(' ').Select(m => (byte)int.Parse(m)).ToArray();
            for(int i = 0; i < n; i++)
            for(int j = 0; j < n; j++)
                this.board[i, j] = values[i * n + j];
        }

        private static byte[,] Initialize(int size)
        {
            byte[,] result = new byte[size, size];
            for(int i = 0; i < size; i++)
            for(int j = 0; j < size; j++)
                result[i, j] = (byte)(i * size + j);
            return result;
        }

        //https://stackoverflow.com/a/19271062/178082
        static int randomSeed = Environment.TickCount;
        static readonly ThreadLocal<Random> random = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref randomSeed)));
        private static int Rand(int max)
        {
            return random.Value.Next(max);
        }

        public void Shuffle(int moves)
        {
            for(int i = 0; i < moves; i++)
            {
                var successors = ExpandMoves();
                var selectedMove = successors[Rand(successors.Count)];
                Move(selectedMove);
            }
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
            if (this.blankRow < (this.N - 1))
            {
                successors.Add(Location.Create(this.blankRow + 1, this.blankCol));
            }
            // left
            if (this.blankCol > 0)
            {
                successors.Add(Location.Create(this.blankRow, this.blankCol - 1));
            }
            // right
            if (this.blankCol < (this.N - 1))
            {
                successors.Add(Location.Create(this.blankRow, this.blankCol + 1));
            }
            return successors;
        }

        // TODO: return the cost of the action with the new state
        public void Move(Location newBlankLocation)
        {
            byte temp = this.board[this.blankRow, this.blankCol];
            this.board[this.blankRow, this.blankCol] = this.board[newBlankLocation.Row, newBlankLocation.Col];
            this.board[newBlankLocation.Row, newBlankLocation.Col] = temp;
            this.blankRow = newBlankLocation.Row;
            this.blankCol = newBlankLocation.Col;
        }

        // TODO: return the cost of the action with the new state
        public NPuzzle MoveCopy(Location newBlankLocation)
        {
            var copy = new NPuzzle { 
                N = this.N, 
                board = this.board.Clone() as byte[,], 
                blankCol = this.blankCol, 
                blankRow = this.blankRow 
            };
            copy.Move(newBlankLocation);
            return copy;
        }

        // TODO: move this elsewhere but right now the state is private
        // The Hamming distance in this case is the number of misplaced tiles
        public int HammingDistance()
        {
            int hammingDistance = 0;
            byte expected = 0;
            for(int i = 0; i < this.N; i++)
            for(int j = 0; j < this.N; j++)
                hammingDistance += this.board[i, j] == expected++ ? 0 : 1;
            return hammingDistance;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for(int i = 0; i < this.N; i++)
            {
                if (i > 0) sb.AppendLine();
                for(int j = 0; j < this.N; j++)
                {
                    sb.AppendFormat("{0:##0} ", this.board[i, j]);
                }
            }
            return sb.ToString();
        }

        public bool IsGoal()
        {
            int expected = 0;
            for(int i = 0; i < this.N; i++)
            for(int j = 0; j < this.N; j++)
                if (this.board[i, j] != expected++) return false;
            return true;
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