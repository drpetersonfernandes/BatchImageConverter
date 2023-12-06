**Batch Image Converter**
=======================

A simple batch image converter.<br><br>


![Screenshot](screenshot.png)

<br>
This program converts batches of image files from JPG, PNG, BMP, GIF, or TIFF to PNG or JPG formats.
<br>
It prompts the user for the input and output folders.
<br>
The program also requests the desired output resolution. If only one value is provided, it maintains the aspect ratio of the original image.
<br>
Additionally, it asks for the output format (JPG or PNG). If JPG is selected as the output format, the program will inquire about the level of JPG compression.
<br>
Users can also choose the resize algorithm, with options including Bicubic, Lanczos, and Spline. 
<br>
When the user presses the "Convert" button, the program converts all images in the input folder and saves them to the output folder, retaining the original filenames. If the output file already exists, it will be overwritten.
<br><br>

Made for windows. Tested on windows 11.
<br><br>

*Written in C#<br>
Microsoft Visual Studio Community 2022 - Version 17.9.0 Preview 1.1<br>
Microsoft .NET 8.0<br>
Using the WPF Framework*