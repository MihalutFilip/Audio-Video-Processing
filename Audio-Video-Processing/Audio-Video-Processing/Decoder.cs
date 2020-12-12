using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Lab1
{
    public class Decoder
    {
        public RGBPixel[,] rgbImage { get; private set; }
        public int[,] Y { get; private set; }
        public int[,] U { get; private set; }
        public int[,] V { get; private set; }
        public List<Block> YBlocks { get; set; }
        public List<Block> UBlocks { get; set; }
        public List<Block> VBlocks { get; set; }

        public Decoder(List<Block> YBlocks, List<Block> UBlocks, List<Block> VBlocks)
        {
            this.YBlocks = YBlocks;
            this.UBlocks = UBlocks;
            this.VBlocks = VBlocks;
        }

        public void Decode()
        {
            var height = YBlocks[YBlocks.Count - 1].X + 8;
            var width = YBlocks[YBlocks.Count - 1].Y + 8;
            Y = new int[height, width];
            U = new int[height, width];
            V = new int[height, width];
            BlocksToMatrix(Y, Type.Y, YBlocks);
            BlocksToMatrix(U, Type.U, YBlocks);
            BlocksToMatrix(V, Type.V, VBlocks);
            YUVToRGB();
        }
        public void Decode2(int[,] quantizationMatrix)
        {
            DeQuantizeBlocks(YBlocks, quantizationMatrix);
            DeQuantizeBlocks(UBlocks, quantizationMatrix);
            DeQuantizeBlocks(VBlocks, quantizationMatrix);
            InverseDCTBlocks(Type.Y, YBlocks);
            InverseDCTBlocks(Type.U, UBlocks);
            InverseDCTBlocks(Type.V, VBlocks);
            AddToBlocks(128, YBlocks);
            AddToBlocks(128, UBlocks);
            AddToBlocks(128, VBlocks);
        }

        public int Clamp(int no)
        {
            return no > 0 ? ( no > 255 ? 255 : no ) : 0;
        }

        public void YUVToRGB()
        {
            var width = Y.GetLength(1);
            var height = Y.GetLength(0);
            var rgbImg = new RGBPixel[height, width];

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    rgbImg[i, j] = new RGBPixel() { 
                        Red = Clamp((int)Math.Floor(Y[i, j] + 1.402 * (V[i, j] - 128))),
                        Green = Clamp((int)Math.Floor(Y[i, j] - 0.344 * (U[i, j] - 128) - 0.714 * (V[i, j] - 128))),
                        Blue = Clamp((int)Math.Floor(Y[i, j] + 1.772 * (U[i, j] - 128)))
                    };

            rgbImage = rgbImg;
        }

        private void BlocksToMatrix(int[,] matrix, Type type, List<Block> blocks)
        {
            foreach (var block in blocks)
            {
                if (type != Type.Y)
                    block.Decompress();

                for (int i = block.X; i < block.X + 8; i++)
                    for (int j = block.Y; j < block.Y + 8; j++)
                        matrix[i, j] = block.Matrix[i - block.X, j - block.Y];
            }
        }

        public void DeQuantizeBlocks(List<Block> blocks, int[,] quantizationMatrix)
        {
            foreach (var block in blocks)
                block.DeQuantizeBlock(quantizationMatrix);
        }

        public void InverseDCTBlocks(Type type, List<Block> blocks)
        {
            foreach (var block in blocks)
            {
                block.InverseDCT();

                if (type != Type.Y)
                    block.Compress();
            }
        }

        public void AddToBlocks(int value, List<Block> blocks)
        {
            foreach (var block in blocks)
                block.Subtract(-value);
        }

        public void WriteToFile(string filePath)
        {
            using (var sw = new StreamWriter(filePath))
            {
                sw.WriteLine("P3");
                sw.WriteLine(rgbImage.GetLength(1) + " " + rgbImage.GetLength(0));
                sw.WriteLine("255");
                for (int i = 0; i < rgbImage.GetLength(0); i++)
                {
                    for (int j = 0; j < rgbImage.GetLength(1); j++)
                    {
                        sw.WriteLine(rgbImage[i, j].Red);
                        sw.WriteLine(rgbImage[i, j].Green);
                        sw.WriteLine(rgbImage[i, j].Blue);
                    }
                }
            }
        }
    }
}
