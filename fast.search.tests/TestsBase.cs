using System;
using Xunit;
using Xunit.Abstractions;

namespace fast.search.tests
{
    public abstract class TestsBase
    {
        protected readonly ITestOutputHelper output;

        public TestsBase(ITestOutputHelper output)
        {
            this.output = output;
        }
    }
}