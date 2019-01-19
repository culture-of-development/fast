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
            int bumper = 0;
            var a = DataGenerator.Matrix(size, size+bumper);
            var b = DataGenerator.Matrix(size+bumper, size);
            var mm = new MatrixMultiply();
            //EnsureCorrectness(a, b, mm.Naive, mm.TransposeSumUnsafe);
            TimeIt("MM - Naive", () => mm.Naive(a, b));
            TimeIt("MM - NaiveSum", () => mm.NaiveSum(a, b));
            TimeIt("MM - NaiveSumUnsafe", () => mm.NaiveSumUnsafe(a, b));
            TimeIt("MM - TransposeSum", () => mm.TransposeSum(a, b));
            TimeIt("MM - TransposeSumUnsafe", () => mm.TransposeSumUnsafe(a, b));
        }

        static void EnsureCorrectness<T>(T[,] a, T[,] b, Func<T[,], T[,], T[,]> baseline, Func<T[,], T[,], T[,]> hypothesis)
        {
            var bl = baseline(a, b);
            var h = hypothesis(a, b);
            CompareMatricies(bl, h);
        }

        static void CompareMatricies<T>(T[,] a, T[,] b)
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
