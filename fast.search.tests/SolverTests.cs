using System;
using Xunit;

using fast.search;
using fast.search.problems;
using System.Diagnostics;
using System.Timers;
using Xunit.Abstractions;

namespace fast.search.tests
{
    public partial class SolverTests : TestsBase
    {
        public SolverTests(ITestOutputHelper output) : base(output) { }
        
        // optimal cost of -1 represents no solution!
        private void TestSolver<TProblemState>(ISearchAlgorithm<TProblemState> solver, IProblem<TProblemState> problem, double optimalCost)
            where TProblemState : IProblemState<TProblemState>
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
                Assert.True(optimalCost == -1d, "Found no solution but one should exist.");
            }
            else
            {
                var state = problem.GetInitialState();
                output.WriteLine("Initial State ----------------");
                output.WriteLine(state.ToString());
                int i = 0;
                double stepCost, totalCost = 0d;
                foreach(var move in solution)
                {
                    i++;
                    (state, stepCost) = problem.ApplyAction(state, move);
                    totalCost += stepCost;
                    //output.WriteLine(state.ToString());
                }
                output.WriteLine("Final State ({0} moves, {1} cost) ----------------", i, totalCost);
                output.WriteLine(state.ToString());
                Assert.True(problem.IsGoal(state), "Non-goal state claimed to be goal.");
                Assert.True(Math.Abs(optimalCost - totalCost) < 1e-8, "Found a non optimal cost: expected = " + optimalCost + "; actual = " + totalCost);
            }
        }
    }
}