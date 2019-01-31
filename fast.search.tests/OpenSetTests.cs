using System;
using Xunit;

using fast.search;
using Xunit.Abstractions;
using System.Collections.Generic;

namespace fast.search.tests
{
    public class OpenSetTests : TestsBase
    {
        public OpenSetTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void AddSameIsNoOp()
        {
            var openSet = new OpenSet<int, int>();
            openSet.PushOrImprove(1, 99);
            openSet.PushOrImprove(1, 99);
            Assert.Equal(1, openSet.Size);
        }

        [Fact]
        public void AddingLowerCostReplacesState()
        {
            var openSet = new OpenSet<int, int>();
            openSet.PushOrImprove(8, 99);
            openSet.PushOrImprove(5, 99);
            Assert.Equal(1, openSet.Size);
            Assert.Equal(5, openSet.MinCost);
        }

        [Fact]
        public void AddingHigherCostIsNoOp()
        {
            var openSet = new OpenSet<int, int>();
            openSet.PushOrImprove(5, 99);
            openSet.PushOrImprove(8, 99);
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
                openSet.PushOrImprove(i, i);
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

        [Fact]
        public void PopRemovesMin()
        {
            var openSet = new OpenSet<int, int>();
            openSet.PushOrImprove(5, 99);
            openSet.PushOrImprove(8, 33);
            int minValue = openSet.PopMin();
            Assert.Equal(99, minValue);
            Assert.Equal(1, openSet.Size);
            Assert.Equal(8, openSet.MinCost);
        }

        [Fact]
        public void ImproveWorksAfter3SameValue()
        {
            var openSet = new OpenSet<int, int>();
            openSet.PushOrImprove(5, 10);
            openSet.PushOrImprove(5, 30);
            openSet.PushOrImprove(5, 20);
            
            openSet.PushOrImprove(6, 30);
            openSet.PushOrImprove(6, 10);
            openSet.PushOrImprove(6, 20);
            Assert.Equal(3, openSet.Size);
        }

        [Fact]
        public void ImproveWorksAfter7SameValue()
        {
            var openSet = new OpenSet<int, int>();
            openSet.PushOrImprove(5, 10);
            openSet.PushOrImprove(5, 60);
            openSet.PushOrImprove(5, 70);
            openSet.PushOrImprove(5, 20);
            openSet.PushOrImprove(5, 40);
            openSet.PushOrImprove(5, 50);
            openSet.PushOrImprove(5, 30);
            
            openSet.PushOrImprove(6, 10);
            openSet.PushOrImprove(6, 60);
            openSet.PushOrImprove(6, 70);
            openSet.PushOrImprove(6, 20);
            openSet.PushOrImprove(6, 40);
            openSet.PushOrImprove(6, 50);
            openSet.PushOrImprove(6, 30);
            Assert.Equal(7, openSet.Size);
        }
    }
}