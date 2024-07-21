using Canvas_Note_Desktop.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Canvas_Note_Desktop.Save
{
    public class CanvasState
    {
        public List<ImageState> Images { get; set; } = new List<ImageState>();
        public List<TextBoxState> TextBoxes { get; set; } = new List<TextBoxState>();

        public static string SaveCanvas(Canvas canvas, string? filePath = null)
        {
            var canvasState = new CanvasState();

            foreach (var child in canvas.Children)
            {
                if (child is Image image)
                {
                    var bitmapImage = image.Source;
                    string? base64Image = ConvertBitmapImageToBase64(bitmapImage);

                    if (base64Image == null)
                        continue;

                    var imageState = new ImageState
                    {
                        Base64Image = base64Image,
                        Width = image.Width,
                        Height = image.Height,
                        Left = Canvas.GetLeft(image),
                        Top = Canvas.GetTop(image),
                        Index = canvas.Children.IndexOf((System.Windows.UIElement) child)
                    };
                    canvasState.Images.Add(imageState);
                }
                else if (child is TextBox textBox)
                {
                    var textBoxState = new TextBoxState
                    {
                        Text = ConvertStringToBase64(textBox.Text),
                        Left = Canvas.GetLeft(textBox),
                        Top = Canvas.GetTop(textBox),
                        FontSize = textBox.FontSize,
                        Index = canvas.Children.IndexOf((System.Windows.UIElement)child)
                    };
                    canvasState.TextBoxes.Add(textBoxState);
                }
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonString = JsonSerializer.Serialize(canvasState, options);


            if(filePath == null)
            {
                var dialog = new SaveFileDialog()
                {
                    AddExtension = true,
                    DefaultExt = "canv",
                    Filter = "Canvas Note|*.canv"
                };

                dialog.ShowDialog();

                filePath = dialog.FileName;
            }

            if (filePath == null || filePath == string.Empty)
                return string.Empty;

            File.WriteAllText(filePath, jsonString);
            return filePath;
        }

        private static string? ConvertBitmapImageToBase64(ImageSource bitmapImage)
        {
            if (bitmapImage is BitmapSource bitmapSource)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder()
                    {
                        QualityLevel = 98
                    };
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    encoder.Save(memoryStream);
                    byte[] imageBytes = memoryStream.ToArray();
                    return Convert.ToBase64String(imageBytes);
                }
            }

            return null;
        }

        public static string LoadCanvas(Canvas canvas, string? filePath = null)
        {
            if (filePath == null)
            {
                var dialog = new OpenFileDialog()
                {
                    AddExtension = true,
                    DefaultExt = "canv",
                    Filter = "Canvas Note|*.canv"
                };

                dialog.ShowDialog();

                filePath = dialog.FileName;
            }

            if (filePath == null || filePath == string.Empty)
                return string.Empty;

            filePath = Path.Combine(filePath);
            var jsonString = File.ReadAllText(filePath);
            var canvasState = JsonSerializer.Deserialize<CanvasState>(jsonString);

            canvas.Children.Clear();

            foreach (var imageState in canvasState.Images)
            {
                var bitmapImage = ConvertBase64ToBitmapImage(imageState.Base64Image);

                var image = new CImage
                {
                    Source = bitmapImage,
                    Width = imageState.Width,
                    Height = imageState.Height
                };
                Canvas.SetLeft(image, imageState.Left);
                Canvas.SetTop(image, imageState.Top);
                canvas.Children.Insert(Math.Max(Math.Min(imageState.Index, canvas.Children.Count - 1), 0), image);
            }

            foreach (var textBoxState in canvasState.TextBoxes)
            {
                var textBox = new CTextBox
                {
                    Text = ConvertBase64ToString(textBoxState.Text),
                    FontSize = (textBoxState.FontSize > 0) ? textBoxState.FontSize : 12
                };

                Canvas.SetLeft(textBox, textBoxState.Left);
                Canvas.SetTop(textBox, textBoxState.Top);
                canvas.Children.Insert(Math.Max(Math.Min(textBoxState.Index, canvas.Children.Count - 1), 0), textBox);
            }

            return filePath;
        }

        private static BitmapImage ConvertBase64ToBitmapImage(string base64Image)
        {
            byte[] imageBytes = Convert.FromBase64String(base64Image);
            using (MemoryStream memoryStream = new MemoryStream(imageBytes))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        public static string ConvertStringToBase64(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string ConvertBase64ToString(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }

    public class ImageState
    {
        public string Base64Image { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public int Index { get; set; }
    }

    public class TextBoxState
    {
        public string Text { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public double FontSize { get; set; }
        public int Index { get; set; } = 0;
    }

}
