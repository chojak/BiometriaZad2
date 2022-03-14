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
        public Bitmap ImageBitmap;
        public Zadanie2Algorithm Algorithm2;
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

            Image.MaxHeight = 500;
            Image.MaxWidth = 800;
            Algorithm2 = new Zadanie2Algorithm();
        }

        private void ChoseFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.tif;...";
            if (openFileDialog.ShowDialog() == true)
            {
                Source = openFileDialog.FileName;
                ImageBitmap = new Bitmap(Source);
                Image.Source = BitmapToImageSource(ImageBitmap);
            }
        }

        private void OtsuAlgorithm_Click(object sender, RoutedEventArgs e)
        {
            //Algorithm.HistogramTest(ImageBitmap, WpfPlot1);
            //Algorithm.HistogramStretching(ImageBitmap, WpfPlot1);

            //ImageBitmap = new Bitmap(Algorithm2.HistogramStretching(ImageBitmap, WpfPlot1));
            //Image.Source = BitmapToImageSource(ImageBitmap);
            //ImageBitmap = new Bitmap(Algorithm2.HistogramEqualization(ImageBitmap, WpfPlot1));
            //Image.Source = BitmapToImageSource(ImageBitmap);

            ImageBitmap = Algorithm2.BinaryThreshold(ImageBitmap, Algorithm2.Otsu(ImageBitmap, WpfPlot1));
            Image.Source = BitmapToImageSource(ImageBitmap);
        }
    }
}
