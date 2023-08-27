using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Windows.Controls;

namespace BatchImageConverter
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Initialize the state based on default selected format.
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
            // Check if the ComboBox itself is null
            if (ConvertTo == null) return;

            // Check if the selected item is null
            if (ConvertTo.SelectedItem == null) return;

            // Try to cast the selected item to a ComboBoxItem and check if it's null or its Content is null
            var selectedItem = ConvertTo.SelectedItem as ComboBoxItem;
            if (selectedItem == null || selectedItem.Content == null) return;

            // Check if the Slider is null
            if (CompressionSlider == null) return;

            // Actual logic
            if (selectedItem.Content.ToString() == "JPG")
            {
                CompressionSlider.IsEnabled = true;
            }
            else
            {
                CompressionSlider.IsEnabled = false;
            }
        }

        private void ExecuteConversion(object sender, RoutedEventArgs e)
        {
            try
            {
                string sourceFolder = SourceFolder.Text;
                string destinationFolder = DestinationFolder.Text;

                if (!Directory.Exists(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);
                }

                // Initialize the ProgressBar
                ConversionProgressBar.Value = 0;

                // Parse width and height as double
                if (!double.TryParse(WidthBox.Text, out double width) || !double.TryParse(HeightBox.Text, out double height))
                {
                    System.Windows.MessageBox.Show("Invalid width or height");
                    return;
                }

                string format = (ConvertTo.SelectedItem as dynamic).Content.ToString();
                int compression = (int)CompressionSlider.Value;

                if (Directory.Exists(sourceFolder))
                {
                    string[] allFiles = Directory.GetFiles(sourceFolder, "*.*", SearchOption.TopDirectoryOnly);
                    int totalFiles = allFiles.Length;  // Total files to be processed

                    ConversionProgressBar.Maximum = totalFiles;  // Set the Maximum property of ProgressBar

                    foreach (string file in allFiles)
                    {
                        FileInfo fileInfo = new FileInfo(file);

                        if (fileInfo.Extension.ToLower() == ".jpg" || fileInfo.Extension.ToLower() == ".png" ||
                            fileInfo.Extension.ToLower() == ".bmp" || fileInfo.Extension.ToLower() == ".gif" ||
                            fileInfo.Extension.ToLower() == ".tiff" || fileInfo.Extension.ToLower() == ".tif")
                        {
                            // Image conversion code
                            BitmapImage image = new BitmapImage();
                            image.BeginInit();
                            image.UriSource = new Uri(file);
                            image.EndInit();

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
                                new System.Windows.Media.ScaleTransform(width / image.PixelWidth, height / image.PixelHeight)
                            );
                            encoder.Frames.Clear();
                            encoder.Frames.Add(BitmapFrame.Create(resizedImage));

                            string destinationFile = Path.Combine(
                                destinationFolder,
                                $"{fileInfo.Name}_{width}x{height}.{format.ToLower()}"
                            );

                            using (var stream = new FileStream(destinationFile, FileMode.Create))
                            {
                                encoder.Save(stream);
                            }

                            // Update the ProgressBar after each file is processed
                            ConversionProgressBar.Value++;
                            double percent = (ConversionProgressBar.Value / ConversionProgressBar.Maximum) * 100;
                            ProgressText.Text = $"{percent}%";
                        }
                    }
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


    }
}