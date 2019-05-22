using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace fast.modeling.tests
{
    public abstract class TestsBase
    {
        protected readonly ITestOutputHelper output;

        public TestsBase(ITestOutputHelper output)
        {
            string filename = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + ".test-output";
            this.output = new FileWritingTestOutputHelper(filename, output);
        }

        private class FileWritingTestOutputHelper : ITestOutputHelper
        {
            private readonly ITestOutputHelper inner;
            private readonly string filename;

            public FileWritingTestOutputHelper(string filename, ITestOutputHelper main)
            {
                inner = main;
                this.filename = filename;
            }

            public void WriteLine(string message)
            {
                inner.WriteLine(message);
                File.AppendAllText(filename, $"[{DateTime.UtcNow}] {message}\n");
            }

            public void WriteLine(string format, params object[] args)
            {
                inner.WriteLine(format, args);
                var message = string.Format(format, args);
                File.AppendAllText(filename, $"[{DateTime.UtcNow}] {message}\n");
            }
        }
    }
}