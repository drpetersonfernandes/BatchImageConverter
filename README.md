**Batch Image Converter**
=======================

A simple batch image converter.<br><br>


![Screenshot](screenshot.png)

<br>
This program batch convert image files from jpg, png, bmp, gif or tiff to png or jpg.<br>
It ask the user for the input and the output folder.<br>
Also ask for the output resolution. If you provide only one value, the program will keep the aspect ratio of the original image.<br>
Also ask for the output format (jpg or png). In case the user select jpg as the output format the program will ask for the level of compression of the jpg.<br>
You can also select the resize algorithm. The options are: Bicubic, Lanczos and Spline.<br>
When the user press the "Convert" button, it will convert all the images inside the input folder and save into the output folder. The output file will have the same filename. If the output file already exist, it will be overwritten.<br>

Made for windows. Tested on windows 11.

*Written in C#<br>
Microsoft Visual Studio Community 2022 - Version 17.9.0 Preview 1.1<br>
Microsoft .NET 8.0<br>
Using the WPF Framework*