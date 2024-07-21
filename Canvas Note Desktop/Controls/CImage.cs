using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Canvas_Note_Desktop.Controls
{
    class CImage : System.Windows.Controls.Image
    {
        private Canvas ParentCanvas => (Canvas) this.Parent;
        public double Scale {
            get => _scale;
            set
            {
                double oldScale = _scale;
                _scale = Math.Clamp(value, 0.05, 8);

                if(value != oldScale)
                    ScaleChanged(oldScale, _scale);
            }
        }

        private double _scale = 1.0;

        public CImage()
        {
            PreviewMouseDown += CImage_PreviewMouseDown;
            MouseWheel += Image_OnMouseWheel;
            PreviewMouseRightButtonUp += Image_PreviewMouseRightButtonUp;
            PreviewKeyUp += CImage_PreviewKeyUp;
            Focusable = true;
            SetCurrentValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.Fant);
        }

        private void CImage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.XButton1)
            {
                Scale = 1;
            }
        }

        private void Image_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
                return; 

            ParentCanvas.Children.Remove(this);

            e.Handled = true;
        }

        private void Image_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                if (e.Delta < 0)
                {
                    Scale -= 0.05;
                }
                else if (e.Delta > 0)
                {
                    Scale += 0.05;
                }
            }
            else if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                ControlManager.MoveElement(this, (e.Delta < 0) ? -1 : 1);
            }
        }

        private void CImage_PreviewKeyUp(object sender, KeyEventArgs e)
        {
        }


        private void ScaleChanged(double oldScale, double newScale)
        {
            _scale = ScaleImage(this, oldScale, newScale);
        }

        private static double ScaleImage(Image image, double oldScale, double newScale)
        {
            // Calculate the scale ratio
            double scaleRatio = newScale / oldScale;

            // Calculate the new width and height
            double newWidth = image.Width * scaleRatio;
            double newHeight = image.Height * scaleRatio;

            // Calculate the offsets to keep the image centered
            double offsetX = (image.Width - newWidth) / 2;
            double offsetY = (image.Height - newHeight) / 2;

            // Apply the new width and height
            image.Width = newWidth;
            image.Height = newHeight;

            // Update the position to keep the image centered
            double left = Canvas.GetLeft(image) + offsetX;
            double top = Canvas.GetTop(image) + offsetY;

            Canvas.SetLeft(image, left);
            Canvas.SetTop(image, top);

            return newScale;
        }
    }
}
