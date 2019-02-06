using System;
using System.Linq;
using Xunit;

using fast.search;
using Xunit.Abstractions;

namespace fast.search.tests
{
    public class NPuzzleTests : TestsBase
    {
        public NPuzzleTests(ITestOutputHelper output) : base(output) { }

        [Theory]
        [InlineData(3,3)]
        [InlineData(3,4)]
        [InlineData(4,3)]
        [InlineData(4,4)]
        public void TestDefaultInitialization(int nrows, int ncols)
        {
            var puzzle3 = new NPuzzle(nrows, ncols);
            Assert.True(puzzle3.IsGoal());
        }

        [Theory]
        [InlineData(3, 3, "8 6 7 2 5 4 3 0 1")]
        [InlineData(3, 4, "8 6 7 2 5 4 3 0 1 9 11 10")]
        [InlineData(4, 3, "8 6 7 2 5 4 3 0 1 9 11 10")]
        public void TestInitializationParsing(int nrows, int ncols, string stateString)
        {
            var flat = stateString.Split(' ').Select(int.Parse).ToArray();
            var puzzle3 = new NPuzzle(nrows, ncols, stateString);
            int i = 0;
            for(int row = 0; row < nrows; row++)
            for(int col = 0; col < ncols; col++)
            {
                Assert.Equal(flat[i], puzzle3[row, col]);
                i++;
            }
        }

        [Theory]
        [InlineData(3, 3, "8 6 7 2 5 4 3 0 1", 21)]
        [InlineData(3, 3, "1 2 3 4 5 6 7 8 0", 0)]
        [InlineData(3, 3, "0 1 2 3 4 5 6 7 8", 12)]
        [InlineData(3, 4, "3 7 9 11 4 8 10 0 5 2 1 6", 29)]
        [InlineData(4, 3, "3 7 9 11 4 8 10 0 5 2 1 6", 26)]
        [InlineData(4, 4, "14 1 9 6 4 8 12 5 7 2 3 0 10 11 13 15", 37)]
        private void TestNPuzzleManhattanDistance(int nrows, int ncols, string initialState, int expected)
        {
            var state = new NPuzzle(nrows, ncols, initialState);
            var score = NPuzzle.ManhattanDistance(state);
            Assert.Equal(expected, score);
        }

        [Theory]
        // [InlineData(3, 3, "8 6 7 2 5 4 3 0 1", true)]
        // [InlineData(3, 3, "1 2 3 4 5 6 7 8 0", true)]
        // [InlineData(3, 3, "0 1 2 3 4 5 6 7 8", true)]
        //[InlineData(4, 4, "14, 1, 9, 6, 4, 8, 12, 5, 7, 2, 3, 0, 10, 11, 13, 15", true)]
        [InlineData(4, 4, "9, 14, 5, 7, 8, 15, 1, 2, 10, 4, 13, 6, 12, 0, 3, 11", true)]
        // [InlineData(4, 4, "13, 8, 14, 3, 9, 1, 0, 7, 15, 5, 4, 10, 12, 2, 6, 11", true)]
        // [InlineData(4, 4, "4, 7, 13, 10, 1, 2, 9, 6, 12, 8, 14, 5, 3, 0, 11, 15", true)]
        // [InlineData(4, 4, "6, 0, 5, 10, 11, 12, 9, 2, 1, 7, 4, 3, 14, 8, 13, 15", true)]
        // [InlineData(4, 4, "5, 4, 7, 1, 11, 12, 14, 15, 10, 13, 8, 6, 2, 0, 9, 3", true)]
        private void TestNPuzzleIsSolvable(int nrows, int ncols, string initialState, bool expected)
        {
            var state = new NPuzzle(nrows, ncols, initialState);
            var sol = state.IsSolvable();
            output.WriteLine(initialState);
            output.WriteLine("inversions: " + sol.Item1);
            Assert.Equal(expected, sol.Item2);
        }

        // TODO: test move expansion
        // TODO: test that making a move makes a copy
        // TODO: test that making a move results in correct state
        // TODO: test hamming distance
        // TODO: test invalid initialization of state
    }
}