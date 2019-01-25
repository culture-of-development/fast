using System;
using System.Diagnostics;

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

        private static void TestSolve()
        {
            var puzzle3_hard = new NPuzzle(3, "8 6 7 2 5 4 3 0 1");
            
            var timer = Stopwatch.StartNew();
            var solver = new BreadthFirstSearchSolver();
            var solution = solver.Solve(puzzle3_hard);
            timer.Stop();
            if (solution == null)
            {
                Console.WriteLine("no solution found!");
            }
            else
            {
                Console.WriteLine("Initial State ----------------");
                Console.WriteLine(puzzle3);
                int i = 1;
                foreach(var move in solution)
                {
                    Console.WriteLine("Move: {0} ----------------", i++);
                    puzzle3.Move(move);
                    Console.WriteLine(puzzle3);
                }
                Console.WriteLine("Optimal solution found!");
            }
            Console.WriteLine("Time: {0}", timer.Elapsed);
        }
    }
}
