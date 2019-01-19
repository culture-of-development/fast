using System;

namespace blas
{
    public static class DataGenerator
    {
        private static Random random = new Random();

        public static float[,] Matrix(int rows, int cols)
        {
            var result = new float[rows, cols];
            for(int r = 0; r < rows; r++)
            for(int c = 0; c < cols; c++)
                result[r, c] = (float)random.NextDouble();
            return result;
        }
    }
}