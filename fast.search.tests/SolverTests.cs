using System;
using Xunit;

using fast.search;
using System.Diagnostics;
using System.Timers;
using Xunit.Abstractions;

namespace fast.search.tests
{
    public class SolverTests : TestsBase
    {
        public SolverTests(ITestOutputHelper output) : base(output) { }
        
        [Fact]
        public void TestBreadthFirstSearch()
        {
            var solver = new BreadthFirstSearchSolver();
            TestSolver(solver, 3, 3, "8 6 7 2 5 4 3 0 1", 31);
            // cannot really solve larger problems with this method
        }

        [Fact]
        public void TestAStarSearchHammingDistance()
        {
            var solver = new AStarSearchSolver(NPuzzle.HammingDistance);
            TestSolver(solver, 3, 3, "8 6 7 2 5 4 3 0 1", 31);
            // cannot really solve larger problems with this method
        }

        [Fact]
        public void TestAStarSearchManhattanDistance()
        {
            var solver = new AStarSearchSolver(NPuzzle.ManhattanDistance);
            TestSolver(solver, 3, 3, "8 6 7 2 5 4 3 0 1", 31);
            //TestSolver(solver, 3, 4, "3 7 9 11 4 8 10 0 5 2 1 6", 37);
            //TestSolver(solver, 4, 4, "14, 1, 9, 6, 4, 8, 12, 5, 7, 2, 3, 0, 10, 11, 13, 15", 45);
        }

        // optimal cost of -1 represents no solution!
        private void TestSolver(ISearchAlgorithm solver, int nrows, int ncols, string initialState, int optimalCost)
        {
            output.WriteLine(solver.ToString());

            var puzzle3_hard = new NPuzzle(nrows, ncols, initialState);
            var puzzle = puzzle3_hard;

            var timer = new Stopwatch();
            double lastTotalS = 0d;
            ulong lastEvals = 0UL;

            // https://docs.microsoft.com/en-us/dotnet/api/system.timers.timer?view=netframework-4.7.2
            var reportingTimer = new Timer(10_000);
            Action reportingHeartbeat = () => {
                var currentTotalS = timer.Elapsed.TotalSeconds;
                var currentEvals = solver.StatesEvaluated;
                var statesPerSecond = (currentEvals - lastEvals) / (currentTotalS - lastTotalS);
                output.WriteLine($"evals: {solver.StatesEvaluated:#,0}   ms: {timer.Elapsed.TotalMilliseconds}   S/s: {statesPerSecond:#,#.###}   cost: {solver.MaxCostEvaulated}");
                lastTotalS = currentTotalS;
                lastEvals = currentEvals;
            };
            reportingTimer.Elapsed += (Object source, ElapsedEventArgs e) => reportingHeartbeat();
            reportingTimer.AutoReset = true;
            reportingTimer.Enabled = true;

            timer.Start();
            var solution = solver.Solve(puzzle);
            timer.Stop();
            reportingTimer.Enabled = false;

            reportingHeartbeat();
            output.WriteLine("Time: {0}", timer.Elapsed);

            if (solution == null)
            {
                Assert.True(optimalCost == -1, "Found no solution but one should exist.");
            }
            else
            {
                output.WriteLine("Initial State ----------------");
                output.WriteLine(puzzle.ToString());
                int i = 0;
                foreach(var move in solution)
                {
                    i++;
                    puzzle.Move(move);
                }
                output.WriteLine("Final State ({0} moves) ----------------", i);
                output.WriteLine(puzzle.ToString());
                Assert.True(optimalCost == i, "Found a non optimal cost: expected = " + optimalCost + "; actual = " + i);
            }
        }
    }
}