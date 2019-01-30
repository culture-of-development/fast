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
            TestSolver(solver, "8 6 7 2 5 4 3 0 1", 31);
        }

        [Fact]
        public void TestAStarSearchHammingDistance()
        {
            var solver = new AStarSearchSolver(NPuzzle.HammingDistance);
            TestSolver(solver, "8 6 7 2 5 4 3 0 1", 31);
        }

        [Fact]
        public void TestAStarSearchManhattanDistance()
        {
            var solver = new AStarSearchSolver(NPuzzle.ManhattanDistance);
            TestSolver(solver, "8 6 7 2 5 4 3 0 1", 31);
        }

        // optimal cost of -1 represents no solution!
        private void TestSolver(ISearchAlgorithm solver, string initialState, int optimalCost)
        {
            output.WriteLine(solver.ToString());

            var puzzle3_hard = new NPuzzle(3, initialState);
            var puzzle = puzzle3_hard;

            var timer = new Stopwatch();

            // https://docs.microsoft.com/en-us/dotnet/api/system.timers.timer?view=netframework-4.7.2
            var reportingTimer = new Timer(10_000);
            Action reportingHeartbeat = () => {
                var statesPerSecond = solver.StatesEvaluated / timer.Elapsed.TotalSeconds;
                output.WriteLine($"evals: {solver.StatesEvaluated:#,0}   ms: {timer.Elapsed.TotalMilliseconds}   S/s: {statesPerSecond:#,#.###}   cost: {solver.MaxCostEvaulated}");
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