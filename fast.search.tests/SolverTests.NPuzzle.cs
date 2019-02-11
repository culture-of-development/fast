using fast.search.problems;
using Xunit;

namespace fast.search.tests
{
    partial class SolverTests
    {
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
            var problem = new NPuzzleProblem(4, 4, "4, 3, 6, 13, 7, 15, 9, 0, 10, 5, 8, 11, 2, 12, 1, 14"); int expected = 50;
            //var problem = new NPuzzleProblem(4, 4, "0, 11, 3, 12, 5, 2, 1, 9, 8, 10, 14, 15, 7, 4, 13, 6"); int expected = 54;
            TestSolver(solver, problem, expected);
        }
    }
}