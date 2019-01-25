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
            TestSolve();
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
        }

        private static void Log(string value)
        {
            Console.WriteLine("[" + DateTime.Now + "]: " + value);
        }

        private static void TestSolve()
        {
            var puzzle3_hard = new NPuzzle(3, "8 6 7 2 5 4 3 0 1");
            var puzzle = puzzle3_hard;
            
            var solver = new BreadthFirstSearchSolver();

            var timer = new Stopwatch();

            // https://docs.microsoft.com/en-us/dotnet/api/system.timers.timer?view=netframework-4.7.2
            var reportingTimer = new Timer(10_000);
            reportingTimer.Elapsed += (Object source, ElapsedEventArgs e) => 
                Log($"States evaluated: {solver.StatesEvaluated:#,0}   Millis: {timer.Elapsed.TotalMilliseconds}");
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
