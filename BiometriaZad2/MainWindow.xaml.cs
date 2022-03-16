using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BiometriaZad2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string Source = "";
        public Bitmap OriginalBitmap;
        public Bitmap ImageBitmap;
        public Zadanie2Algorithm Algorithm2;
        public int BrightnessThreshold = 0;
        private BitmapImage BitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }
        }
        public MainWindow()
        {
            InitializeComponent();

            Algorithm2 = new Zadanie2Algorithm();
            //BrightnessThreshold = 0;
        }
        private void ChoseFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.tif;...";
            if (openFileDialog.ShowDialog() == true)
            {
                Source = openFileDialog.FileName;
                OriginalBitmap = new Bitmap(Source);
                ImageBitmap = new Bitmap(Source);
                Image.Source = BitmapToImageSource(ImageBitmap);
                Algorithm2.Histogram(ImageBitmap, WpfPlot);
            }
        }
        private void OtsuAlgorithm_Click(object sender, RoutedEventArgs e)
        {
            Image.Source = BitmapToImageSource(Algorithm2.BinaryThreshold(ImageBitmap, Algorithm2.Otsu(ImageBitmap, WpfPlot), WpfPlot));
        }
        private void Stretching_Click(object sender, RoutedEventArgs e)
        {
            Image.Source = BitmapToImageSource(Algorithm2.HistogramStretching(ImageBitmap, WpfPlot, StretchingValue.Value));
        }
        private void Equalization_Click(object sender, RoutedEventArgs e)
        {
            Image.Source = BitmapToImageSource(Algorithm2.HistogramEqualization(OriginalBitmap, WpfPlot));
        }
        private void Brightness_Click(object sender, RoutedEventArgs e)
        {
            Image.Source = BitmapToImageSource(Algorithm2.HistogramBightnessPL(ImageBitmap, BrightnessThreshold, WpfPlot));
        }
        private void Orginial_Click(object sender, RoutedEventArgs e)
        {
            Image.Source = BitmapToImageSource((Bitmap)OriginalBitmap);
            Algorithm2.Histogram(OriginalBitmap, WpfPlot);
        }
        private void Threshold_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BrightnessThreshold = (int)Threshold.Value;
            ThresholdLabel.Content = "Threshold: " + BrightnessThreshold;
        }
    }
}
