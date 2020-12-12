using System;
using System.Collections.Generic;
using System.Text;

namespace Lab1
{
    

    public class Block
    {
        public Block(int dimension, Type type, int x, int y)
        {
            Matrix = new int[dimension, dimension];
            Type = type;
            X = x;
            Y = y;
        }

        public int[,] Matrix { get; set; }
        public Type Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public void Compress()
        {
            var b = new int[4, 4];

            for (var i = 0; i < 4; i++)
                for (var j = 0; j < 4; j++)
                    b[i, j] = (Matrix[i * 2, j * 2] + Matrix[i * 2, j * 2 + 1] + Matrix[i * 2 + 1, j * 2] +
                               Matrix[i * 2 + 1, j * 2 + 1]) / 4;
            
            Matrix = b;
        }

        public void Decompress()
        {
            var b = new int[8, 8];

            for (var i = 0; i < 4; i++)
                for (var j = 0; j < 4; j++)
                    b[i * 2, j * 2] = b[i * 2, j * 2 + 1] =
                        b[i * 2 + 1, j * 2] = b[i * 2 + 1, j * 2 + 1] = Matrix[i, j];
            
            Matrix = b;
        }

        public void Subtract(int val)
        {
            for (var i = 0; i < Matrix.GetLength(0); i++)
                for (var j = 0; j < Matrix.GetLength(1); j++)
                    Matrix[i, j] -= val;
        }

        public double Alpha(int u)
        {
            if (u == 0)
                return 1 / Math.Sqrt(2);
            return 1;
        }

        public double DctFormulaSum(int x, int y)
        {
            double sum = 0;

            for (var i = 0; i < 8; i++)
                for (var j = 0; j < 8; j++)
                    sum += Matrix[i, j] * Math.Cos((2 * i + 1) * x * Math.PI / 16) *
                             Math.Cos((2 * j + 1) * y * Math.PI / 16);
            
            return sum;
        }

        public void ToDCTBlock()
        {
            var aux = new int[8, 8];

            for (var i = 0; i < 8; i++)
                for (var j = 0; j < 8; j++)
                    aux[i, j] = (int)Math.Floor(1f / 4f * Alpha(i) * Alpha(j) * DctFormulaSum(i, j));
            
            Matrix = aux;
        }

        public void ToQuantizedCoefficientsBlock(int[,] quantizationMatrix)
        {
            for (var i = 0; i < 8; i++)
                for (var j = 0; j < 8; j++)
                    Matrix[i, j] = Matrix[i, j] / quantizationMatrix[i, j];
        }

        public void DeQuantizeBlock(int[,] quantizationMatrix)
        {
            for (var i = 0; i < 8; i++)
            {
                for (var j = 0; j < 8; j++)
                {
                    Matrix[i, j] = Matrix[i, j] * quantizationMatrix[i, j];
                }
            }
        }

        public double IdctFormulaSum(int x, int y)
        {
            double sum = 0;

            for (var i = 0; i < 8; i++)
                for (var j = 0; j < 8; j++)
                    sum += Alpha(i) * Alpha(j) * Matrix[i, j] * Math.Cos((2 * x + 1) * i * Math.PI / 16) *
                             Math.Cos((2 * y + 1) * j * Math.PI / 16);

            return sum;
        }

        public void InverseDCT()
        {
            var aux = new int[8, 8];

            for (var i = 0; i < 8; i++)
                for (var j = 0; j < 8; j++)
                    aux[i, j] = (int)Math.Floor(1f / 4f * IdctFormulaSum(i, j));
            
            Matrix = aux;
        }
    }

    public enum Type
    {
        Y,
        U,
        V
    }


}
