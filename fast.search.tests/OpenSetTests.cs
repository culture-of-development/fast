using System;
using Xunit;

using fast.search;

namespace fast.search.tests
{
    public class OpenSetTests
    {
        [Fact]
        public void AddSameIsNoOp()
        {
            var openSet = new OpenSet<int, int>();
            openSet.Improve(1, 99);
            openSet.Improve(1, 99);
            Assert.Equal(1, openSet.Size);
        }

        [Fact]
        public void AddingLowerCostReplacesState()
        {
            var openSet = new OpenSet<int, int>();
            openSet.Improve(8, 99);
            openSet.Improve(5, 99);
            Assert.Equal(1, openSet.Size);
            Assert.Equal(5, openSet.MinCost);
        }

        [Fact]
        public void AddingHigherCostIsNoOp()
        {
            var openSet = new OpenSet<int, int>();
            openSet.Improve(5, 99);
            openSet.Improve(8, 99);
            Assert.Equal(1, openSet.Size);
            Assert.Equal(5, openSet.MinCost);
        }

        [Fact]
        public void PoppingEmptySetFails()
        {
            var openSet = new OpenSet<int, int>();
            try
            {
                openSet.PopMin();
                Assert.True(false, "PopMin from an empty set did not throw InvalidOperationException.");
            }
            catch(InvalidOperationException)
            {
                Assert.True(true);
            }
        }

        [Fact]
        public void TestSorting10()
        {
            var openSet = new OpenSet<int, int>();
            var values = new int[] { 9, 2, 3, 8, 0, 4, 7, 5, 6, 1 };
            for(int j = 0; j < 2; j++)
            foreach(var i in values)
            {
                openSet.Improve(i, i);
            }
            int nextValue = 0;
            while(!openSet.IsEmpty)
            {
                var val = openSet.PopMin();
                if (val != nextValue) break;
                nextValue += 1;
            }
            Assert.Equal(nextValue, values.Length);
        }
    }
}