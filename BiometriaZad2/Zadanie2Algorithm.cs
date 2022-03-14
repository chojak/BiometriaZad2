using ScottPlot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BiometriaZad2
{
    public class Zadanie2Algorithm
    {
        public int[] red = null;
        private int[] green = null;
        private int[] blue = null;

        public int[] Histogram(Bitmap bmp, WpfPlot wpfPlot)
        {
            int bmpHeight = bmp.Height;
            int bmpWidth = bmp.Width;

            double[] histogram = new double[256];
            double[] histogramX = new double[256];

            red = new int[256];
            green = new int[256];
            blue = new int[256];    

            for (int i = 0; i < histogramX.Length; i++)
                histogramX[i] = i;

            for (int x = 0; x < bmpWidth; x++)
            {
                for (int y = 0; y < bmpHeight; y++)
                {
                    Color pixel = bmp.GetPixel(x, y);

                    red[pixel.R]++;
                    green[pixel.G]++;
                    blue[pixel.B]++;

                    int mean = (pixel.R + pixel.G + pixel.B) / 3;
                    histogram[mean]++;
                }
            }
            return histogram.Select(d => (int)d).ToArray();
        }
        private int[] calculateLUT(int[] histogram)
        {
            int minValue = 0;
            int maxValue = 255;

            for (int i = 0; i < histogram.Length; i++)
                if (histogram[i] != 0)
                {
                    minValue = i;
                    break;
                }

            for (int i = 255; i >= 0; i--)
                if (histogram[i] != 0)
                {
                    maxValue = i;
                    break;
                }

            int[] lut = new int[256];
            double a = 255.0 / (maxValue - minValue);
            for (int i = 0; i < lut.Length; i++)
            {
                lut[i] = (int)(a * (i - minValue));
            }

            return lut;
        }
        private int[] calculateLUT(int[] histogram, int size)
        {
            double minValue = 0;
            for (int i = 0; i < 256; i++)
            {
                if (histogram[i] != 0)
                {
                    minValue = histogram[i];
                    break;
                }
            }

            int[] result = new int[256];
            double sum = 0;
            for (int i = 0; i < 256; i++)
            {
                sum += histogram[i];
                result[i] = (int)(((sum - minValue) / (size - minValue)) * 255.0);
            }

            return result;
        }
        public Bitmap HistogramStretching(Bitmap bmp, WpfPlot wpfPlot)
        {
            int[] histogram = Histogram(bmp, wpfPlot);

            int[] LUTred = calculateLUT(red);
            int[] LUTgreen = calculateLUT(green);
            int[] LUTblue = calculateLUT(blue);

            red = new int[256];
            green = new int[256];
            blue = new int[256];
            
            Bitmap newBitmap = new Bitmap(bmp.Width, bmp.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color pixel = bmp.GetPixel(x, y);
                    Color newPixel = Color.FromArgb(LUTred[pixel.R], LUTgreen[pixel.G], LUTblue[pixel.B]);
                    newBitmap.SetPixel(x, y, newPixel);
                    red[newPixel.R]++;
                    green[newPixel.G]++;
                    blue[newPixel.B]++;
                }
            }

            return newBitmap;
        }
        public Bitmap HistogramEqualization(Bitmap bmp, WpfPlot wpfPlot)
        {
            int[] LUTred = calculateLUT(red, bmp.Width * bmp.Height);
            int[] LUTgreen = calculateLUT(green, bmp.Width * bmp.Height);
            int[] LUTblue = calculateLUT(blue, bmp.Width * bmp.Height);

            red = new int[256];
            green = new int[256];
            blue = new int[256];
            Bitmap newBitmap = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format24bppRgb);
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color pixel = bmp.GetPixel(x, y);
                    Color newPixel = Color.FromArgb(LUTred[pixel.R], LUTgreen[pixel.G], LUTblue[pixel.B]);
                    newBitmap.SetPixel(x, y, newPixel);
                    red[newPixel.R]++;
                    green[newPixel.G]++;
                    blue[newPixel.B]++;
                }
            }
            return newBitmap;
        }
        public byte Otsu(Bitmap bmp, WpfPlot wpfPlot)
        {
            int[] tmp = Histogram(bmp, wpfPlot);
            double[] histogram = tmp.Select(x => (double)x).ToArray();

            int size = bmp.Height * bmp.Width;
            for (int i = 0; i < 256; i++)
            {
                histogram[i] = histogram[i] / size;
            }

            double avgValue = 0;
            for (int i = 0; i < 256; i++)
            {
                avgValue += i * histogram[i];  
            }

            int threshold = 0;
            double maxVariance = 0;
            double w = 0, u = 0;
            for (int i = 0; i < 256; i++)
            {
                w += histogram[i];  //Assuming that the current gray level i is the threshold value, the pixels of 0~i gray level (assuming that the pixels with the pixel value in this range are called foreground pixels) are the proportion of the entire image      
                u += i * histogram[i];  //  Average gray value of pixels (0~i) before gray level i: average gray value of foreground pixels      
                double t = avgValue * w - u;
                double variance = t * t / (w * (1 - w));
                
                if (variance > maxVariance)
                {
                    maxVariance = variance;
                    threshold = i;
                }
            }

            double[] histogramX = new double[256];
            double[] histogramY = new double[256];

            for (int i = 0; i < 256; i++)
            {
                histogramX[i] = i;
                histogramY[i] = histogram[i];
            }

            wpfPlot.Plot.Clear();
            wpfPlot.Plot.AddScatter(histogramX, histogramY);
            wpfPlot.Render();

            return (byte)threshold;
        }
        public Bitmap BinaryThreshold(Bitmap bmp, byte threshold)
        {
            var data = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                System.Drawing.Imaging.ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb
            );

            var bmpData = new byte[data.Width * 3 * data.Height];

            Marshal.Copy(data.Scan0, bmpData, 0, bmpData.Length);

            for (int i = 0; i < bmpData.Length; i += 3)
            {
                byte r = bmpData[i + 0];
                byte g = bmpData[i + 1];
                byte b = bmpData[i + 2];

                byte mean = (byte)((r + g + b) / 3);

                bmpData[i + 0] =
                bmpData[i + 1] =
                bmpData[i + 2] = mean > threshold ? byte.MaxValue : byte.MinValue;
            }

            Marshal.Copy(bmpData, 0, data.Scan0, bmpData.Length);

            bmp.UnlockBits(data);
            return bmp;
        }
    }
}
