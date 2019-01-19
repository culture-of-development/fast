using System;
using System.Runtime.InteropServices;

namespace blas
{
    public class MatrixMultiply
    {
        public float[,] Naive(float[,] a, float[,] b)
        {
            var x = a.GetLength(0);
            var z = a.GetLength(1);
            var y = b.GetLength(1);

            float[,] result = new float[x, y];
            for(int i = 0; i < x; i++)
            for(int j = 0; j < y; j++)
            for(int k = 0; k < z; k++)
            {
                result[i,j] += a[i, k] * b[k, j];
            }
            return result;
        }

        public float[,] NaiveSum(float[,] a, float[,] b)
        {
            var x = a.GetLength(0);
            var z = a.GetLength(1);
            var y = b.GetLength(1);

            float[,] result = new float[x, y];
            for(int i = 0; i < x; i++)
            for(int j = 0; j < y; j++)
            {
                float sum = 0f;
                for(int k = 0; k < z; k++)
                {
                    sum += a[i, k] * b[k, j];
                }
                result[i,j] = sum;
            }
            return result;
        }

        public unsafe float[,] NaiveSumUnsafe(float[,] a, float[,] b)
        {
            var x = a.GetLength(0);
            var z = a.GetLength(1);
            var y = b.GetLength(1);

            float[,] result = new float[x, y];
            
            fixed(float* af = a)
            fixed(float* bf = b)
            for(int i = 0; i < x; i++)
            for(int j = 0; j < y; j++)
            {
                float sum = 0f;
                float* aa = &af[i*z];
                float* bb = &bf[j];
                for(int k = 0; k < z; k++)
                {
                    sum += *aa * *bb;
                    aa++;
                    bb += y;
                }
                result[i,j] = sum;
            }
            return result;
        }

        public float[,] TransposeSum(float[,] a, float[,] b)
        {
            var bT = MatrixTranspose(b);

            var x = a.GetLength(0);
            var z = a.GetLength(1);
            var y = b.GetLength(1);

            float[,] result = new float[x, y];
            for(int i = 0; i < x; i++)
            for(int j = 0; j < y; j++)
            {
                float sum = 0f;
                for(int k = 0; k < z; k++)
                {
                    sum += a[i, k] * bT[j, k];
                }
                result[i,j] = sum;
            }
            return result;
        }

        public float[,] MatrixTranspose(float[,] m)
        {
            var x = m.GetLength(0);
            var y = m.GetLength(1);
            var result = new float[y, x];
            for(int i = 0; i < x; i++)
            for(int j = 0; j < y; j++)
            {
                result[j, i] = m[i, j];
            }
            return result;
        }

        public unsafe float[,] TransposeSumUnsafe(float[,] a, float[,] b)
        {
            var bT = MatrixTranspose(b);

            var x = a.GetLength(0);
            var z = a.GetLength(1);
            var y = b.GetLength(1);

            float[,] result = new float[x, y];

            fixed(float* af = a)
            fixed(float* bf = bT)
            for(int i = 0; i < x; i++)
            for(int j = 0; j < y; j++)
            {
                float sum = 0f;
                float* aa = &af[i*z];
                float* bb = &bf[j*z];
                for(int k = 0; k < z; k++)
                {
                    sum += *aa * *bb;
                    aa++;
                    bb++;
                }
                result[i,j] = sum;
            }
            return result;
        }
    }
}