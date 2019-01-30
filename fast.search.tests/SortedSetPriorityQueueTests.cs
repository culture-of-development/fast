using System;
using Xunit;

using fast.search;

namespace fast.search.tests
{
    public class SortedSetPriorityQueueTests
    {
        [Fact]
        public void TestSorting10()
        {
            var queue = new SortedSetPriorityQueue<int, int>();
            var values = new int[] { 9, 2, 3, 8, 0, 4, 7, 5, 6, 1 };
            foreach(var i in values)
            {
                queue.Push(i, i);
            }
            int expected = 0;
            while(!queue.IsEmpty)
            {
                var val = queue.Pop();
                if (val != expected) break;
                expected += 1;
            }
            Assert.Equal(expected, values.Length);
        }
    }
}
