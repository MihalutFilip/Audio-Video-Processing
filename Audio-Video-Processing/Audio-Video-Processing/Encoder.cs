using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Lab1
{
    public class Encoder
    {
        public RGBPixel[,] rgbImage { get; private set; }
        public int[,] Y { get; private set; }
        public int[,] U { get; private set; }
        public int[,] V { get; private set; }
        public List<Block> YBlocks { get; private set; }
        public List<Block> UBlocks { get; private set; }
        public List<Block> VBlocks { get; private set; }

        public Encoder(RGBPixel[,] rgbImg)
        {
            rgbImage = rgbImg;
        }

        public void Encode()
        {
            RGBToYUV();
            YBlocks = YUVToBlocks(Type.Y, Y);
            UBlocks = YUVToBlocks(Type.U, U);
            VBlocks = YUVToBlocks(Type.V, V);
        }

        public void Encode2(int[,] quantizationMatrix)
        {
            SubtractFromBlocks(128, YBlocks);
            SubtractFromBlocks(128, UBlocks);
            SubtractFromBlocks(128, VBlocks);
            BlocksToDCTBlocks(Type.Y, YBlocks);
            BlocksToDCTBlocks(Type.U, UBlocks);
            BlocksToDCTBlocks(Type.V, VBlocks);
            QuantizeBlocks(YBlocks, quantizationMatrix);
            QuantizeBlocks(UBlocks, quantizationMatrix);
            QuantizeBlocks(VBlocks, quantizationMatrix);
        }

        private void RGBToYUV()
        {
            var height = rgbImage.GetLength(0);
            var width = rgbImage.GetLength(1);
            Y = new int[height, width];
            U = new int[height, width];
            V = new int[height, width];
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    Y[i, j] = (int)(0.299 * rgbImage[i, j].Red + 0.587 * rgbImage[i, j].Green + 0.114 * rgbImage[i, j].Blue);
                    U[i, j] = (int)(128 - 0.1687 * rgbImage[i, j].Red - 0.3312 * rgbImage[i, j].Green + 0.5 * rgbImage[i, j].Blue);
                    V[i, j] = (int)(128 + 0.5 * rgbImage[i, j].Red - 0.4186 * rgbImage[i, j].Green - 0.0813 * rgbImage[i, j].Blue);
                }
        }

        private List<Block> YUVToBlocks(Type type, int[,] YUV)
        {
            var blocks = new List<Block>();
            for (int i = 0; i < YUV.GetLength(0); i += 8)
                for (int j = 0; j < YUV.GetLength(1); j += 8)
                {
                    var block = new Block(8, type, i, j);

                    for (int k = i; k < i + 8; k++)
                        for (int l = j; l < j + 8; l++)
                            block.Matrix[k - i, l - j] = YUV[k, l];
                   
                    if(type != Type.Y)
                        block.Compress();

                    blocks.Add(block);
                }
            return blocks;
        }

        public void SubtractFromBlocks(int value, List<Block> blocks)
        {
            foreach (var block in blocks)
                block.Subtract(value);
        }

        public void BlocksToDCTBlocks(Type type, List<Block> blocks)
        {
            foreach (var block in blocks)
            {
                if (type != Type.Y)
                    block.Decompress();

                block.ToDCTBlock();
            }
        }

        public void QuantizeBlocks(List<Block> blocks, int[,] quantizationMatrix)
        {
            foreach (var block in blocks)
                block.ToQuantizedCoefficientsBlock(quantizationMatrix);
        }

        public void WriteToFile(string filePath)
        {
            using (var sw = new StreamWriter(filePath))
            {
                sw.WriteLine("Y Blocks");

                foreach (var block in YBlocks)
                {
                    for (int i = 0; i < block.Matrix.GetLength(0); i++)
                    {
                        for (int j = 0; j < block.Matrix.GetLength(1); j++)
                        {
                            sw.Write(block.Matrix[i, j] + " ");
                        }
                        sw.WriteLine();
                    }
                    sw.WriteLine();
                }

                sw.WriteLine("U Blocks");

                foreach (var block in UBlocks)
                {
                    for (int i = 0; i < block.Matrix.GetLength(0); i++)
                    {
                        for (int j = 0; j < block.Matrix.GetLength(1); j++)
                        {
                            sw.Write(block.Matrix[i, j] + " ");
                        }
                        sw.WriteLine();
                    }
                    sw.WriteLine();
                }

                sw.WriteLine("V Blocks");

                foreach (var block in VBlocks)
                {
                    for (int i = 0; i < block.Matrix.GetLength(0); i++)
                    {
                        for (int j = 0; j < block.Matrix.GetLength(1); j++)
                        {
                            sw.Write(block.Matrix[i, j] + " ");
                        }
                        sw.WriteLine();
                    }
                    sw.WriteLine();
                }
            }
        }
    }
}
