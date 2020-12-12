using System;
using System.Collections.Generic;
using System.IO;

namespace Lab1
{
    class Program
    {
        private static readonly string filePath = @"C:\Users\mihalutf\Desktop\Facultate - Anul 3\Procesare video si audio\nt-P3.ppm";
        private static RGBPixel[,] ReadFile()
        {
            string[] lines = File.ReadAllLines(filePath), widthAndHeightLine;

            int lineCount = 1, width, height;

            while (lines[lineCount].StartsWith("#"))
            {
                lineCount++;
            }

            widthAndHeightLine = lines[lineCount].Split(" ");
            lineCount += 2; // height and width and max value
            width = int.Parse(widthAndHeightLine[0]);
            height = int.Parse(widthAndHeightLine[1]);
            var rgbImage = new RGBPixel[height, width];

            for (int i = 0; i < height; i++)
                for (var j = 0; j < width; j++)
                {
                    rgbImage[i, j] = new RGBPixel()
                    {
                        Red = int.Parse(lines[lineCount]),
                        Green = int.Parse(lines[lineCount + 1]),
                        Blue = int.Parse(lines[lineCount + 2])
                    };

                    lineCount += 3;
                }

            return rgbImage;
        }

        static void Main(string[] args)
        {
            var quantizationMatrix = new[,]
            {
                {6, 4, 4, 6, 10, 16, 20, 24},
                {5, 5, 6, 8, 10, 23, 24, 22},
                {6, 5, 6, 10, 16, 23, 28, 22},
                {6, 7, 9, 12, 20, 35, 32, 25},
                {7, 9, 15, 22, 27, 44, 41, 31},
                {10, 14, 22, 26, 32, 42, 45, 37},
                {20, 26, 31, 35, 41, 48, 48, 40},
                {29, 37, 38, 39, 45, 40, 41, 40}
            };

            var rgbImage = ReadFile();

            var encoder = new Encoder(rgbImage);
            encoder.Encode();
            encoder.Encode2(quantizationMatrix);
            //encoder.WriteToFile(@"C:\Users\mihalutf\Desktop\Facultate - Anul 3\Procesare video si audio\blocks.txt");

            var decoder = new Decoder(encoder.YBlocks, encoder.UBlocks, encoder.VBlocks);
            decoder.Decode2(quantizationMatrix);
            decoder.Decode();

            decoder.WriteToFile(@"C:\Users\mihalutf\Desktop\Facultate - Anul 3\Procesare video si audio\results.ppm");
        }
    }
}
