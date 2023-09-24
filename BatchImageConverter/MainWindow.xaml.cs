using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Threading;

namespace BatchImageConverter
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            string defaultFormat = (ConvertTo.SelectedItem as ComboBoxItem).Content as string;
            CompressionSlider.IsEnabled = (defaultFormat == "JPG");
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Closes the application
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Batch Image Converter\nPeterson's Software\n08/2023");
        }

        private void ChooseSourceFolder(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                SourceFolder.Text = dialog.SelectedPath;
            }
        }

        private void ChooseDestinationFolder(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                DestinationFolder.Text = dialog.SelectedPath;
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

                //if (!double.TryParse(WidthBox.Text, out double width) || !double.TryParse(HeightBox.Text, out double height))
                //{
                //    System.Windows.MessageBox.Show("Invalid width or height");
                //    return;
                //}

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

                    Progress<double> progress = new Progress<double>(percent =>
                    {
                        ConversionProgressBar.Value = percent;
                        int percentage = (int)Math.Round((percent / totalFiles) * 100);
                        ProgressText.Text = $"{percentage}%";
                    });


                    await Task.Run(() => ProcessFiles(allFiles, destinationFolder, format, compression, width, height, progress));
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

        private void ProcessFiles(string[] allFiles, string destinationFolder, string format, int compression, double targetWidth, double targetHeight, IProgress<double> progress)
        {
            int totalFiles = allFiles.Length;
            double filesProcessed = 0;  // Initialize the counter for files processed

            foreach (string file in allFiles)
            {
                FileInfo fileInfo = new FileInfo(file);

                if (fileInfo.Extension.ToLower() == ".jpg" || fileInfo.Extension.ToLower() == ".png" ||
                    fileInfo.Extension.ToLower() == ".bmp" || fileInfo.Extension.ToLower() == ".gif" ||
                    fileInfo.Extension.ToLower() == ".tiff" || fileInfo.Extension.ToLower() == ".tif")
                {
                    // Load the image
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri(file);
                    image.EndInit();

                    // Determine dimensions for the current image:
                    double currentWidth = targetWidth;
                    double currentHeight = targetHeight;

                    if (currentWidth <= 0)
                    {
                        currentWidth = image.PixelWidth * (targetHeight / image.PixelHeight);
                    }
                    else if (currentHeight <= 0)
                    {
                        currentHeight = image.PixelHeight * (targetWidth / image.PixelWidth);
                    }

                    BitmapEncoder encoder;

                    if (format == "JPG")
                    {
                        encoder = new JpegBitmapEncoder();
                        ((JpegBitmapEncoder)encoder).QualityLevel = compression;
                    }
                    else
                    {
                        encoder = new PngBitmapEncoder();
                    }

                    encoder.Frames.Add(BitmapFrame.Create(image));

                    TransformedBitmap resizedImage = new TransformedBitmap(
                        image,
                        new System.Windows.Media.ScaleTransform(currentWidth / image.PixelWidth, currentHeight / image.PixelHeight)
                    );
                    encoder.Frames.Clear();
                    encoder.Frames.Add(BitmapFrame.Create(resizedImage));

                    string destinationFile = Path.Combine(
                        destinationFolder,
                        $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}.{format.ToLower()}"
                    );

                    using (var stream = new FileStream(destinationFile, FileMode.Create))
                    {
                        encoder.Save(stream);
                    }

                    // Increment the processed files counter
                    filesProcessed++;

                    // Report the progress
                    progress.Report(filesProcessed);
                }
            }
        }


    }
}
