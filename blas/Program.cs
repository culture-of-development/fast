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
            int size = 512;
            var a = DataGenerator.Matrix(size, size);
            var b = DataGenerator.Matrix(size, size);
            var mm = new MatrixMultiply();
            //var knownCorrect = mm.Naive(a, b);
            //var nu = mm.NaiveSumUnsafe(a, b);
            //CompareMatricies(knownCorrect, nu);
            TimeIt("MM - Naive", () => mm.Naive(a, b));
            TimeIt("MM - NaiveSum", () => mm.NaiveSum(a, b));
            TimeIt("MM - NaiveSumUnsafe", () => mm.NaiveSumUnsafe(a, b));
        }

        static void CompareMatricies(float[,] a, float[,] b)
        {
            for(int i = 0; i < a.GetLength(0); i++)
            for(int j = 0; j < a.GetLength(1); j++)
            {
                Console.WriteLine(i + ", " + j + " --- " + a[i, j] + " --- " + b[i, j]);
            }
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
