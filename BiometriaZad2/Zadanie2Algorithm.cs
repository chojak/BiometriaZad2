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
        private int[] red;
        private int[] green;
        private int[] blue;
        private int[] calculateLUT(int[] histogram, double threshold)
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
            double a = threshold / (maxValue - minValue);
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
        private void MakePlot(int[] histogram, WpfPlot wpfPlot)
        {
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
        }
        public byte Otsu(Bitmap bmp, WpfPlot wpfPlot)
        {
            // https://www.programmerall.com/article/1106135802/

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
                w += histogram[i];  
                u += i * histogram[i];
                double t = avgValue * w - u;
                double variance = t * t / (w * (1 - w));
                
                if (variance > maxVariance)
                {
                    maxVariance = variance;
                    threshold = i;
                }
            }

            return (byte)threshold;
        }
        public int[] Histogram(Bitmap bmp, WpfPlot wpfPlot)
        {
            int bmpHeight = bmp.Height;
            int bmpWidth = bmp.Width;

            double[] histogram = new double[256];

            red = new int[256];
            green = new int[256];
            blue = new int[256];    

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

            MakePlot(histogram.Select(d => (int)d).ToArray(), wpfPlot);
            return histogram.Select(d => (int)d).ToArray();
        }
        public Bitmap HistogramBrightness(Bitmap bmp, int Value, WpfPlot wpfPlot)
        {
            Bitmap TempBitmap = new Bitmap(bmp);
            float FinalValue = (float)Value / 255.0f;
            Bitmap NewBitmap = new Bitmap(TempBitmap.Width, TempBitmap.Height);
            Graphics NewGraphics = Graphics.FromImage(NewBitmap);
            float[][] FloatColorMatrix = {
                     new float[] {1, 0, 0, 0, 0},
                     new float[] {0, 1, 0, 0, 0},
                     new float[] {0, 0, 1, 0, 0},
                     new float[] {0, 0, 0, 1, 0},
                     new float[] {FinalValue, FinalValue, FinalValue, 1, 1}
            };
            ColorMatrix NewColorMatrix = new ColorMatrix(FloatColorMatrix);
            ImageAttributes Attributes = new ImageAttributes();

            Attributes.SetColorMatrix(NewColorMatrix);
            NewGraphics.DrawImage(TempBitmap, new System.Drawing.Rectangle(0, 0, TempBitmap.Width, TempBitmap.Height), 0, 0, TempBitmap.Width, TempBitmap.Height, System.Drawing.GraphicsUnit.Pixel, Attributes);
            Attributes.Dispose();
            NewGraphics.Dispose();

            MakePlot(Histogram(NewBitmap, wpfPlot), wpfPlot);
            return NewBitmap;
        }
        public Bitmap HistogramBightnessPL(Bitmap bmp, int Value, WpfPlot wpfPlot)
        {
            byte[] LUT = new byte[256];
            int b = Value;

            for (int i = 0; i < 256; i++)
            {
                if ((b + i) > 255)
                {
                    LUT[i] = 255;
                }
                else if ((b + i) < 0)
                {
                    LUT[i] = 0;
                }
                else
                {
                    LUT[i] = (byte)(b + i);
                }
            }

            Bitmap bitmap = new Bitmap(bmp);

            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            byte[] pixelValues = new byte[Math.Abs(bmpData.Stride) * bitmap.Height];
            Marshal.Copy(bmpData.Scan0, pixelValues, 0, pixelValues.Length);

            for (int i = 0; i < pixelValues.Length; i++)
            {
                pixelValues[i] = LUT[pixelValues[i]];
            }

            Marshal.Copy(pixelValues, 0, bmpData.Scan0, pixelValues.Length);
            bitmap.UnlockBits(bmpData);

            Histogram(bitmap, wpfPlot);
            return bitmap;
        } 
        public Bitmap HistogramStretching(Bitmap bmp, WpfPlot wpfPlot, double threshold)
        {
            int[] histogram = Histogram(bmp, wpfPlot);

            int[] LUTred = calculateLUT(red, threshold);
            int[] LUTgreen = calculateLUT(green, threshold);
            int[] LUTblue = calculateLUT(blue, threshold);

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

            Histogram(newBitmap, wpfPlot);
            return newBitmap;
        }
        public Bitmap HistogramEqualization(Bitmap bmp, WpfPlot wpfPlot)
        {
            int[] LUTred = calculateLUT(red, bmp.Width * bmp.Height);
            int[] LUTgreen = calculateLUT(green, bmp.Width * bmp.Height);
            int[] LUTblue = calculateLUT(blue, bmp.Width * bmp.Height);

            red = green =  blue = new int[256];

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

            Histogram(newBitmap, wpfPlot);
            return newBitmap;
        }
        public Bitmap BinaryThreshold(Bitmap bmp, byte threshold, WpfPlot wpfPlot)
        {
            Bitmap newBitmap = new Bitmap(bmp);
            var data = newBitmap.LockBits(
                new Rectangle(0, 0, newBitmap.Width, newBitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb
            );

            var newBitmapData = new byte[data.Width     * 3 * data.Height];

            Marshal.Copy(data.Scan0, newBitmapData, 0, newBitmapData.Length);

            for (int i = 0; i < newBitmapData.Length; i += 3)
            {
                byte r = newBitmapData[i + 0];
                byte g = newBitmapData[i + 1];
                byte b = newBitmapData[i + 2];

                byte mean = (byte)((r + g + b) / 3);

                newBitmapData[i + 0] =
                newBitmapData[i + 1] =
                newBitmapData[i + 2] = mean > threshold ? byte.MaxValue : byte.MinValue;
            }

            Marshal.Copy(newBitmapData, 0, data.Scan0, newBitmapData.Length);

            newBitmap.UnlockBits(data);

            Histogram(newBitmap, wpfPlot);
            return newBitmap;
        }
    }
}
