using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace fast.blas
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

        public FloatMatrix2d NaiveMat2d(FloatMatrix2d a, FloatMatrix2d b)
        {
            var x = a.rows;
            var z = a.cols;
            var y = b.cols;

            var result = new FloatMatrix2d(x, y);
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
                float* aa = af+i*z;
                float* bb = bf+j;
                float* max_a = aa+z;
                float sum = 0f;
                for(; aa < max_a; aa++, bb+=y)
                {
                    sum += *aa * *bb;
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

        public FloatMatrix2d TransposeSumMat2d(FloatMatrix2d a, FloatMatrix2d b)
        {
            var bT = MatrixTransposeMat2d(b);

            var x = a.rows;
            var z = a.cols;
            var y = b.cols;

            FloatMatrix2d result = new FloatMatrix2d(x, y);
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

        public FloatMatrix2d MatrixTransposeMat2d(FloatMatrix2d m)
        {
            var x = m.rows;
            var y = m.cols;
            var result = new FloatMatrix2d(x, y);
            for(int i = 0; i < x; i++)
            for(int j = 0; j < y; j++)
            {
                result[j, i] = m[i, j];
            }
            return result;
        }

        public FloatMatrix2d TransposeSumVectorMat2d(FloatMatrix2d a, FloatMatrix2d b)
        {
            var bT = MatrixTransposeMat2d(b);

            var x = a.rows;
            var z = a.cols;
            var y = b.cols;

            FloatMatrix2d result = new FloatMatrix2d(x, y);
            for(int i = 0; i < x; i++)
            for(int j = 0; j < y; j++)
            {
                float sum = 0f;
                for(int k = 0; k < z; k += Vector<float>.Count)
                {
                    var am = a.Vector(i, k);
                    var bm = bT.Vector(j, k);
                    //sum += a[i, k] * bT[j, k];
                    sum += Vector.Dot<float>(am, bm);
                }
                result[i,j] = sum;
            }
            return result;
        }

        public FloatMatrix2d TransposeSumStridingVectorMat2d(FloatMatrix2d a, FloatMatrix2d b)
        {
            const int stride = 64 / sizeof(float);

            var bT = MatrixTransposeMat2d(b);

            var x = a.rows;
            var z = a.cols;
            var y = b.cols;

            FloatMatrix2d result = new FloatMatrix2d(x, y);
            for(int i = 0; i < x; i += stride)
            for(int j = 0; j < y; j += stride)
            for(int k = 0; k < z; k += stride)
            {
                for (int i2 = i; i2 < i+stride; i2++)
                for (int j2 = j; j2 < j+stride; j2++)
                {
                    float sum = 0f;
                    //for (int k2 = k; k2 < k+stride; k2++)
                    for(int k2 = k; k2 < k+stride; k2 += Vector<float>.Count)
                    {
                        try{
                        var am = a.Vector(i2, k2);
                        var bm = bT.Vector(j2, k2);
                        sum += Vector.Dot<float>(am, bm);
                        }catch(Exception)
                        {
                            Console.WriteLine("error: " + i2 + "," + k2 + "," + j2);
                            throw;
                        }
                        //sum += a[i2, k2] * bT[j2, k2];
                    }
                    result[i2,j2] = sum;
                }
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
                float* aa = af+i*z;
                float* bb = bf+j*z;
                float* max_a = aa+z;
                float sum = 0f;
                for(; aa < max_a; aa++, bb++)
                {
                    sum += *aa * *bb;
                }
                result[i,j] = sum;
            }
            return result;
        }

        public float[,] StridingSum(float[,] a, float[,] b)
        {
            const int stride = 64 / sizeof(float);

            var x = a.GetLength(0);
            var z = a.GetLength(1);
            var y = b.GetLength(1);

            float[,] result = new float[x, y];

            for(int i = 0; i < x; i += stride)
            for(int j = 0; j < y; j += stride)
            for(int k = 0; k < z; k += stride)
            {
                for (int i2 = i; i2 < i+stride; i2++)
                for (int j2 = j; j2 < j+stride; j2++)
                {
                    float sum = 0f;
                    for (int k2 = k; k2 < k+stride; k2++)
                    {
                        sum += a[i2, k2] * b[k2, j2];
                    }
                    result[i2,j2] += sum;
                }
            }
            return result;
        }

        public unsafe float[,] StridingSumUnsafe(float[,] a, float[,] b)
        {
            const int stride = 64 / sizeof(float);
            
            var x = a.GetLength(0);
            var z = a.GetLength(1);
            var y = b.GetLength(1);

            float[,] result = new float[x, y];

            fixed(float* af = a)
            fixed(float* bf = b)
            for(int i = 0; i < x; i += stride)
            for(int j = 0; j < y; j += stride)
            for(int k = 0; k < z; k += stride)
            {
                float* a_head = af+i*z+k;
                for (int i2 = i; i2 < i+stride; i2++, a_head+=z)
                {
                    float* b_head = bf+k*y+j;
                    for (int j2 = j; j2 < j+stride; j2++, b_head++)
                    {
                        float* aa = a_head;
                        float* bb = b_head;

                        float sum = 0f;
                        float* max_a = aa+stride;
                        for (; aa < max_a; aa++, bb+=y)
                        {
                            sum += *aa * *bb;
                        }
                        result[i2,j2] += sum;
                    }
                }
            }
            return result;
        }

        public unsafe float[,] StridingUnrolledUnsafe(float[,] a, float[,] b)
        {
            const int stride = 64 / sizeof(float);
            
            var x = a.GetLength(0);
            var z = a.GetLength(1);
            var y = b.GetLength(1);

            float[,] result = new float[x, y];

            fixed(float* af = a)
            fixed(float* bf = b)
            for(int i = 0; i < x; i += stride)
            for(int j = 0; j < y; j += stride)
            for(int k = 0; k < z; k += stride)
            {
                float* a_head = af+i*z+k;
                for (int i2 = i; i2 < i+stride; i2++, a_head+=z)
                {
                    float* b_head = bf+k*y+j;
                    for (int j2 = j; j2 < j+stride; j2++, b_head++)
                    {
                        float* aa = a_head;
                        float* bb = b_head;

                        float sum = 0f;
                        float sum2 = 0f;
                        float* max_a = aa+stride;
                        for (; aa < max_a; aa++,bb+=y<<1)
                        {
                            sum += *aa * *bb;
                            sum2 += *(++aa) * *(bb+y);
                        }
                        result[i2,j2] += sum + sum2;
                    }
                }
            }
            return result;
        }
    }
}