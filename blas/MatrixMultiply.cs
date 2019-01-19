using System;

namespace blas
{
    public class MatrixMultiply
    {
        public float[,] Naive(float[,] a, float[,] b)
        {
            var m = a.GetLength(0);
            var n = a.GetLength(1);
            var o = b.GetLength(1);

            float[,] result = new float[m, o];
            for(int i = 0; i < m; i++)
            for(int j = 0; j < o; j++)
            for(int k = 0; k < n; k++)
            {
                result[i,j] += a[i, k] * b[k, j];
            }
            return result;
        }

        public float[,] NaiveSum(float[,] a, float[,] b)
        {
            var m = a.GetLength(0);
            var n = a.GetLength(1);
            var o = b.GetLength(1);

            float[,] result = new float[m, o];
            for(int i = 0; i < m; i++)
            for(int j = 0; j < o; j++)
            {
                float sum = 0f;
                for(int k = 0; k < n; k++)
                {
                    sum += a[i, k] * b[k, j];
                }
                result[i,j] = sum;
            }
            return result;
        }
    }
}