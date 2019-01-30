using System;
using System.Linq;
using Xunit;

using fast.search;

namespace fast.search.tests
{
    public class NPuzzleTests
    {
        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        public void TestDefaultInitialization(int n)
        {
            var puzzle3 = new NPuzzle(n);
            Assert.True(puzzle3.IsGoal());
        }

        [Fact]
        public void TestInitializationParsing()
        {
            var stateString = "8 6 7 2 5 4 3 0 1";
            var flat = stateString.Split(' ').Select(int.Parse).ToArray();
            var puzzle3 = new NPuzzle(3, stateString);
            int i = 0;
            for(int row = 0; row < 3; row++)
            for(int col = 0; col < 3; col++)
            {
                Assert.Equal(flat[i], puzzle3[row, col]);
                i++;
            }
        }

        [Theory]
        [InlineData(3, "8 6 7 2 5 4 3 0 1", 21)]
        [InlineData(3, "1 2 3 4 5 6 7 8 0", 0)]
        [InlineData(3, "0 1 2 3 4 5 6 7 8", 12)]
        private static void TestNPuzzleManhattanDistance(int n, string initialState, int expected)
        {
            var state = new NPuzzle(n, initialState);
            var score = NPuzzle.ManhattanDistance(state);
            Assert.Equal(expected, score);
        }

        // TODO: test move expansion
        // TODO: test that making a move makes a copy
        // TODO: test that making a move results in correct state
        // TODO: test hamming distance
        // TODO: test invalid initialization of state
    }
}