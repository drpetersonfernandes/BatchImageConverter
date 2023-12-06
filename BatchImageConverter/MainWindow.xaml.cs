using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Image = SixLabors.ImageSharp.Image;

namespace BatchImageConverter
{
    public partial class MainWindow : Window
    {
        private string selectedAlgorithm = "Bicubic interpolation"; //Default algorithm

        public MainWindow()
        {
            InitializeComponent();
            string defaultFormat = (ConvertTo.SelectedItem as ComboBoxItem).Content as string;
            CompressionSlider.IsEnabled = (defaultFormat == "JPG");
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Batch Image Converter\nPeterson's Software\nVersion 1.2\n12/2023", "About");
        }

        private void ChooseSourceFolder(object sender, RoutedEventArgs e)
        {
#pragma warning disable CA1416 // Validate platform compatibility
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
#pragma warning restore CA1416 // Validate platform compatibility
            if (result == System.Windows.Forms.DialogResult.OK)
            {
#pragma warning disable CA1416 // Validate platform compatibility
                SourceFolder.Text = dialog.SelectedPath;
#pragma warning restore CA1416 // Validate platform compatibility
            }
        }

        private void ChooseDestinationFolder(object sender, RoutedEventArgs e)
        {
#pragma warning disable CA1416 // Validate platform compatibility
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
#pragma warning restore CA1416 // Validate platform compatibility
            if (result == System.Windows.Forms.DialogResult.OK)
            {
#pragma warning disable CA1416 // Validate platform compatibility
                DestinationFolder.Text = dialog.SelectedPath;
#pragma warning restore CA1416 // Validate platform compatibility
            }
        }

        private void ConvertTo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Null checks and the null-conditional operator to prevent NullReferenceException
            var content = (ConvertTo?.SelectedItem as ComboBoxItem)?.Content?.ToString();

            // Check if 'CompressionSlider' is null before attempting to set 'IsEnabled'
            if (CompressionSlider != null)
            {
                CompressionSlider.IsEnabled = (content == "JPG");
            }
        }

        private async void ExecuteConversion(object sender, RoutedEventArgs e)
        {
            try
            {
                string sourceFolder = SourceFolder.Text;
                string destinationFolder = DestinationFolder.Text;

                if (!Directory.Exists(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);
                }

                if (!double.TryParse(WidthBox.Text, out double width))
                {
                    width = 0;
                }

                if (!double.TryParse(HeightBox.Text, out double height))
                {
                    height = 0;
                }

                if (width <= 0 && height <= 0)
                {
                    System.Windows.MessageBox.Show("At least one of the dimensions (width or height) should be valid.");
                    return;
                }

                string format = (ConvertTo.SelectedItem as dynamic).Content.ToString();
                int compression = (int)CompressionSlider.Value;

                if (Directory.Exists(sourceFolder))
                {
                    string[] allFiles = Directory.GetFiles(sourceFolder, "*.*", SearchOption.TopDirectoryOnly);
                    int totalFiles = allFiles.Length;

                    ConversionProgressBar.Maximum = totalFiles;

                    Progress<double> progress = new(percent =>
                    {
                        ConversionProgressBar.Value = percent;
                        int percentage = (int)Math.Round((percent / totalFiles) * 100);
                        ProgressText.Text = $"{percentage}%";
                    });
                    await Task.Run(() => ProcessFiles(allFiles, destinationFolder, format, compression, width, height, progress, selectedAlgorithm));
                }

            }

            catch (DirectoryNotFoundException ex)
            {
                System.Windows.MessageBox.Show("Directory not found: " + ex.Message);
            }
            catch (IOException ex)
            {
                System.Windows.MessageBox.Show("I/O error: " + ex.Message);
            }
            catch (Exception ex)
            {
                // General exception handler
                System.Windows.MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void AlgorithmSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                selectedAlgorithm = selectedItem.Content.ToString();
            }
        }


        private static void ProcessFiles(string[] allFiles, string destinationFolder, string format, int compression, double targetWidth, double targetHeight, IProgress<double> progress, string selectedAlgorithm)
        {
            double filesProcessed = 0;

            foreach (string file in allFiles)
            {
                FileInfo fileInfo = new(file);
                if (IsSupportedImageFile(fileInfo.Extension))
                {
                    BitmapImage image = LoadImage(file);

                    // Determine target dimensions
                    (double currentWidth, double currentHeight) = CalculateDimensions(image.PixelWidth, image.PixelHeight, targetWidth, targetHeight);

                    BitmapSource processedImage = ProcessImageBasedOnAlgorithm(image, currentWidth, currentHeight, selectedAlgorithm);

                    SaveProcessedImage(processedImage, destinationFolder, fileInfo, format, compression);

                    filesProcessed++;
                    progress.Report(filesProcessed);
                }
            }
        }

        private static bool IsSupportedImageFile(string extension)
        {
            string[] supportedExtensions = [".jpg", ".png", ".bmp", ".gif", ".tiff", ".tif"];
            return supportedExtensions.Any(ext => ext.Equals(extension, StringComparison.CurrentCultureIgnoreCase));
        }

        private static BitmapImage LoadImage(string filePath)
        {
            BitmapImage image = new();
            image.BeginInit();
            image.UriSource = new Uri(filePath);
            image.EndInit();
            return image;
        }

        private static BitmapImage ConvertToBitmapSource(Image image)
        {
            using var memoryStream = new MemoryStream();
            image.SaveAsBmp(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();
            bitmapImage.Freeze(); // Important for use in WPF

            return bitmapImage;
        }


        private static Image ConvertBitmapImageToImageSharpImage(BitmapImage bitmapImage)
        {
            using MemoryStream memoryStream = new();
#pragma warning disable CA1859 // Use concrete types when possible for improved performance
            BitmapEncoder encoder = new PngBitmapEncoder();
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            encoder.Save(memoryStream);
            memoryStream.Position = 0;
            return Image.Load(memoryStream);
        }


        private static (double width, double height) CalculateDimensions(double originalWidth, double originalHeight, double targetWidth, double targetHeight)
        {
            double currentWidth = targetWidth > 0 ? targetWidth : originalWidth * (targetHeight / originalHeight);
            double currentHeight = targetHeight > 0 ? targetHeight : originalHeight * (targetWidth / originalWidth);
            return (currentWidth, currentHeight);
        }

        private static BitmapImage ProcessImageBasedOnAlgorithm(BitmapImage bitmapImage, double width, double height, string algorithm)
        {
            using Image imageSharp = ConvertBitmapImageToImageSharpImage(bitmapImage);

            switch (algorithm)
            {
                case "Bicubic interpolation":
                    using (var resizedImage = ResizeWithBicubic(imageSharp, (int)width, (int)height))
                    {
                        return ConvertToBitmapSource(resizedImage);
                    }
                case "Lanczos resampling":
                    using (var resizedImage = ResizeWithLanczos(imageSharp, (int)width, (int)height))
                    {
                        return ConvertToBitmapSource(resizedImage);
                    }
                case "Spline":
                    using (var resizedImage = ResizeWithSpline(imageSharp, (int)width, (int)height))
                    {
                        return ConvertToBitmapSource(resizedImage);
                    }
                default:
                    throw new ArgumentException("Unknown algorithm selected.");
            }
        }

        private static void SaveProcessedImage(BitmapSource image, string destinationFolder, FileInfo fileInfo, string format, int compression)
        {
            BitmapEncoder encoder = format == "JPG" ? new JpegBitmapEncoder { QualityLevel = compression } : new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            string destinationFile = Path.Combine(destinationFolder, $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}.{format.ToLower()}");

            using var stream = new FileStream(destinationFile, FileMode.Create);
            encoder.Save(stream);
        }

        private static Image ResizeWithBicubic(Image image, int width, int height)
        {
            return image.Clone(ctx => ctx.Resize(width, height, KnownResamplers.Bicubic));
        }

        private static Image ResizeWithLanczos(Image image, int width, int height)
        {
            // Lanczos resampling is best suited for downscaling, but it can also be used for upscaling.
            // The third parameter in the Resize method is the size of the Lanczos kernel. 
            // A value of 3 or 5 is common, with 3 being faster and 5 potentially giving higher quality.
            return image.Clone(ctx => ctx.Resize(width, height, KnownResamplers.Lanczos5));
        }

        private static Image ResizeWithSpline(Image image, int width, int height)
        {
            // Use Spline interpolation for resizing.
            return image.Clone(ctx => ctx.Resize(width, height, KnownResamplers.Spline));
        }


    }
}
