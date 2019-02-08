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
            var solver = new BreadthFirstSearchSolver<NPuzzle>();
            var problem = new NPuzzleProblem(3, 3, "8 6 7 2 5 4 3 0 1");
            TestSolver(solver, problem, 27);
            // cannot really solve larger problems with this method
        }

        [Fact]
        public void TestAStarSearchHammingDistance()
        {
            var solver = new AStarSearchSolver<NPuzzle>(NPuzzle.HammingDistance);
            var problem = new NPuzzleProblem(3, 3, "8 6 7 2 5 4 3 0 1");
            TestSolver(solver, problem, 27);
            // cannot really solve larger problems with this method
        }

        [Fact]
        public void TestAStarSearchManhattanDistance()
        {
            var solver = new AStarSearchSolver<NPuzzle>(NPuzzle.ManhattanDistance);
            var problem = new NPuzzleProblem(3, 3, "8 6 7 2 5 4 3 0 1"); int expected = 27;
            //var problem = new NPuzzleProblem(3, 4, "3 7 9 11 4 8 10 0 5 2 1 6"); int expected = 37;
            //var problem = new NPuzzleProblem(4, 4, "14, 1, 9, 6, 4, 8, 12, 5, 7, 2, 3, 0, 10, 11, 13, 15"); int expected = 45;
            //var problem = new NPuzzleProblem(4, 4, "4, 3, 6, 13, 7, 15, 9, 0, 10, 5, 8, 11, 2, 12, 1, 14"); int expected = 50;
            TestSolver(solver, problem, expected);
        }

        [Fact]
        public void TestIDAStarSearchManhattanDistance()
        {
            var solver = new IDAStarSearchSolver<NPuzzle>(NPuzzle.ManhattanDistance);
            //var problem = new NPuzzleProblem(3, 3, "8 6 7 2 5 4 3 0 1"); int expected = 27;
            //var problem = new NPuzzleProblem(3, 4, "3 7 9 11 4 8 10 0 5 2 1 6"); int expected = 37;
            //var problem = new NPuzzleProblem(4, 4, "14, 1, 9, 6, 4, 8, 12, 5, 7, 2, 3, 0, 10, 11, 13, 15"); int expected = 45;
            //var problem = new NPuzzleProblem(4, 4, "4, 3, 6, 13, 7, 15, 9, 0, 10, 5, 8, 11, 2, 12, 1, 14"); int expected = 50;
            var problem = new NPuzzleProblem(4, 4, "0, 11, 3, 12, 5, 2, 1, 9, 8, 10, 14, 15, 7, 4, 13, 6"); int expected = 54;
            TestSolver(solver, problem, expected);
        }

        // optimal cost of -1 represents no solution!
        private void TestSolver(ISearchAlgorithm<NPuzzle> solver, IProblem<NPuzzle> problem, int optimalCost)
        {
            output.WriteLine(solver.ToString());

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
            var solution = solver.Solve(problem);
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
                var puzzle = problem.GetInitialState();
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
                Assert.True(puzzle.IsGoal(), "Non-goal state claimed to be goal.");
                Assert.True(optimalCost == i, "Found a non optimal cost: expected = " + optimalCost + "; actual = " + i);
            }
        }
    }
}