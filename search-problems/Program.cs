using System;
using System.Diagnostics;
using System.Timers;

namespace search_problems
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //TestGoalInitialization();
            //TestMinHeap();
            TestSolve();
        }

        private static void TestMinHeap()
        {
            var queue = new MinHeap<int, int>(1000);
            var values = new int[] { 9, 2, 3, 8, 0, 4, 7, 5, 6, 1 };
            foreach(var i in values)
            {
                queue.Push(i, i);
            }
            int expected = 0;
            while(!queue.IsEmpty)
            {
                var val = queue.Pop();
                if (val != expected++) throw new InvalidOperationException("min heap fail: " + expected);
            }
            if (expected == values.Length)
                Console.WriteLine("Simple min heap seems fine.");
            else 
                Console.WriteLine("Incorrect number of items extracted from queue.");
        }

        private static void TestGoalInitialization()
        {
            var puzzle3 = new NPuzzle(3);
            Console.WriteLine(puzzle3);
            Console.WriteLine(puzzle3.IsGoal());

            puzzle3.Shuffle(1);
            Console.WriteLine("-----------");
            Console.WriteLine(puzzle3);
            Console.WriteLine(puzzle3.IsGoal());

            Console.WriteLine("-----------");
            puzzle3 = new NPuzzle(3, "8 6 7 2 5 4 3 0 1");
            Console.WriteLine(puzzle3);
            Console.WriteLine(puzzle3.IsGoal());

            puzzle3.Shuffle(1);
            Console.WriteLine("-----------");
            Console.WriteLine(puzzle3);
            Console.WriteLine(puzzle3.IsGoal());
        }

        private static void Log(string value)
        {
            Console.WriteLine("[" + DateTime.Now + "]: " + value);
        }

        private static void TestSolve()
        {
            var puzzle3_hard = new NPuzzle(3, "8 6 7 2 5 4 3 0 1");
            var puzzle = puzzle3_hard;
            
            ISearchAlgorithm solver;
            //solver = new BreadthFirstSearchSolver();
            solver = new AStarSearchSolver();
            Console.WriteLine(solver.GetType().Name);

            var timer = new Stopwatch();

            // https://docs.microsoft.com/en-us/dotnet/api/system.timers.timer?view=netframework-4.7.2
            var reportingTimer = new Timer(10_000);
            reportingTimer.Elapsed += (Object source, ElapsedEventArgs e) => {
                var statesPerSecond = solver.StatesEvaluated / timer.Elapsed.TotalSeconds;
                Log($"evals: {solver.StatesEvaluated:#,0}   ms: {timer.Elapsed.TotalMilliseconds}   S/s: {statesPerSecond:#,#.###}   depth: {solver.MaxDepth}");
            };
            reportingTimer.AutoReset = true;
            reportingTimer.Enabled = true;

            timer.Start();
            var solution = solver.Solve(puzzle);
            timer.Stop();
            reportingTimer.Enabled = false;

            if (solution == null)
            {
                Console.WriteLine("no solution found!");
            }
            else
            {
                Console.WriteLine("Initial State ----------------");
                Console.WriteLine(puzzle);
                int i = 1;
                foreach(var move in solution)
                {
                    Console.WriteLine("Move: {0} ----------------", i++);
                    puzzle.Move(move);
                    Console.WriteLine(puzzle);
                }
                Console.WriteLine("Optimal solution found!");
            }
            Console.WriteLine("Time: {0}", timer.Elapsed);
        }
    }
}
