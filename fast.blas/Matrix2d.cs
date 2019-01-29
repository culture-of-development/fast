using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace fast.blas
{
    public class FloatMatrix2d
    {
        public readonly int rows;
        public readonly int cols;

        private float[] m;

        public FloatMatrix2d(int rows, int cols)
        {
            this.rows = rows;
            this.cols = cols;
            this.m = new float[rows * cols];
        }

        public FloatMatrix2d(float[,] values)
        {
            this.rows = values.GetLength(0);
            this.cols = values.GetLength(1);
            this.m = new float[this.rows * this.cols];
            Buffer.BlockCopy(values, 0, this.m, 0, this.m.Length * sizeof(float));
        }

        public float this[int row, int col]
        {
            get 
            {
                //if (row >= this.rows || row < 0) throw new ArgumentOutOfRangeException("row");
                //if (col >= this.cols || col < 0) throw new ArgumentOutOfRangeException("col");
                return this.m[this.GetIndex(row, col)]; 
            }
            set 
            {
                //if (row >= this.rows || row < 0) throw new ArgumentOutOfRangeException("row");
                //if (col >= this.cols || col < 0) throw new ArgumentOutOfRangeException("col");
                this.m[this.GetIndex(row, col)] = value; 
            }
        }

        public Vector<float> Vector(int row, int col)
        {
            return new Vector<float>(this.m, this.GetIndex(row, col));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetIndex(int row, int col)
        {
            return row * this.cols + col;
        }
    }
}