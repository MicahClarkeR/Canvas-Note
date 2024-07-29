using Canvas_Note_Desktop.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Input;
using Point = System.Windows.Point;
using Canvas_Note_Desktop.Dialogs;
using Canvas_Note_Desktop.Save;

namespace Canvas_Note_Desktop.Factories
{
    internal class ControlFactory
    {
        public static CTextBox CreateTextbox(Canvas canvas, string? content = null, Point? position = null, double? width = null, double? fontSize = null, bool focus = false, int? insertLocation = null)
        {
            position ??= Mouse.GetPosition(canvas);
            content ??= "Text";

            CTextBox textBox = new CTextBox()
            {
                Text = content.Trim(),
                AcceptsReturn = true
            };

            if (insertLocation != null)
                canvas.Children.Insert(insertLocation.Value, textBox);
            else
                canvas.Children.Add(textBox);

            Canvas.SetLeft(textBox, position.Value.X);
            Canvas.SetTop(textBox, position.Value.Y);

            if (width != null)
                if (width == 0)
                    textBox.Width = double.NaN;
                else
                    textBox.Width = width.Value;

            if (fontSize != null)
                textBox.FontSize = fontSize.Value;

            if (focus)
            {
                textBox.Focus();
                textBox.CaretIndex = textBox.Text.Length;
            }

            return textBox;
        }

        public static CImage CreateImage(Canvas canvas, BitmapSource source, Point? position = null)
        {
            position ??= Mouse.GetPosition(canvas);
            CImage image = new CImage()
            {
                Source = source,
                Width = source.Width,
                Height = source.Height
            };

            canvas.Children.Add(image);
            Canvas.SetLeft(image, position.Value.X - (source.Width / 2));
            Canvas.SetTop(image, position.Value.Y - (source.Height / 2));

            return image;
        }

        public static CImage CreateImageFromFile(Canvas canvas, string file, Point? position = null)
        {
            BitmapSource imageSource = new BitmapImage(new Uri(file));
            return CreateImage(canvas, imageSource, position);
        }

        public static CProduct CreateProduct(Canvas canvas, BitmapSource source, string text)
        {
            CProduct product = new CProduct();
            product.SetBitmapSource(source);
            product.SetText(text);

            return product;
        }

        public static CProduct? CreateProductWithDialog(Canvas canvas, Point? position = null)
        {
            ProductDialog dialog = new ProductDialog();
            dialog.ShowDialog();

            if(dialog.Successful)
            {
                BitmapSource imageSource = new BitmapImage(new Uri(dialog.Image));
                CProduct product = new CProduct(canvas);
                product.SetBitmapSource(imageSource);
                product.SetText(dialog.Text);

                position ??= Mouse.GetPosition(canvas);

                canvas.Children.Add(product);
                Canvas.SetLeft(product, position.Value.X);
                Canvas.SetTop(product, position.Value.Y);

                return product;
            }

            return null;
        }
    }
}
