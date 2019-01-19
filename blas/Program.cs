using System;
using System.Diagnostics;

namespace blas
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            DoMatrix();
        }

        static void DoMatrix()
        {
            int size = 200;
            var a = DataGenerator.Matrix(size, size);
            var b = DataGenerator.Matrix(size, size);
            var mm = new MatrixMultiply();
            TimeIt("MM - Naive", () => mm.Naive(a, b));
            TimeIt("MM - NaiveSum", () => mm.NaiveSum(a, b));
        }

        static void TimeIt(string description, Action work)
        {
            Console.Write(description + ": ");
            var timer = Stopwatch.StartNew();
            work();
            timer.Stop();
            Console.WriteLine(timer.Elapsed.TotalMilliseconds);
        }
    }
}
