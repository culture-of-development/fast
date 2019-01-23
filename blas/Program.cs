using System;
using System.Diagnostics;
using System.Numerics;

namespace fast.blas
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
            int size = true ? 512 : 16;
            Console.WriteLine("Matrix size: " + size);
            Console.WriteLine("Vector size: " + Vector<float>.Count);
            int bumper = 0;
            var a = DataGenerator.Matrix(size, size+bumper);
            var am = new FloatMatrix2d(a);
            var b = DataGenerator.Matrix(size+bumper, size);
            var bm = new FloatMatrix2d(b);
            var mm = new MatrixMultiply();
            if (size <= 20)
            {
                EnsureCorrectness(a, b, mm.Naive, mm.NaiveSumUnsafe);
                //EnsureCorrectness(a, b, mm.TransposeSum, am, bm, mm.TransposeSumVectorMat2d);
            }
            TimeIt("MM - " + nameof(mm.Naive), () => mm.Naive(a, b));
            TimeIt("MM - " + nameof(mm.NaiveMat2d), () => mm.NaiveMat2d(am, bm));
            TimeIt("MM - " + nameof(mm.NaiveSum), () => mm.NaiveSum(a, b));
            TimeIt("MM - " + nameof(mm.NaiveSumUnsafe), () => mm.NaiveSumUnsafe(a, b));
            TimeIt("MM - " + nameof(mm.TransposeSum), () => mm.TransposeSum(a, b));
            TimeIt("MM - " + nameof(mm.TransposeSumMat2d), () => mm.TransposeSumMat2d(am, bm));
            TimeIt("MM - " + nameof(mm.TransposeSumVectorMat2d), () => mm.TransposeSumVectorMat2d(am, bm));
            TimeIt("MM - " + nameof(mm.TransposeSumUnsafe), () => mm.TransposeSumUnsafe(a, b));
            TimeIt("MM - " + nameof(mm.StridingSum), () => mm.StridingSum(a, b));
            TimeIt("MM - " + nameof(mm.StridingSumUnsafe), () => mm.StridingSumUnsafe(a, b));
            TimeIt("MM - " + nameof(mm.StridingUnrolledUnsafe), () => mm.StridingUnrolledUnsafe(a, b));
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

        static void EnsureCorrectness(
            float[,] a, float[,] b, Func<float[,], float[,], float[,]> baseline, 
            FloatMatrix2d am, FloatMatrix2d bm, Func<FloatMatrix2d, FloatMatrix2d, FloatMatrix2d> hypothesis)
        {
            var bl = baseline(a, b);
            var h = hypothesis(am, bm);
            for(int i = 0; i < a.GetLength(0); i++)
            for(int j = 0; j < a.GetLength(1); j++)
            {
                Console.WriteLine(i + ", " + j + " --- " + bl[i, j] + " --- " + h[i, j]);
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
