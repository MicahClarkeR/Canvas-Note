using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Canvas_Note_Desktop.Controls
{
    class CTextBox : TextBox
    {
        private Canvas? ParentCanvas => (Canvas)this.Parent;

        public CTextBox()
        {
            MouseWheel += TextBox_MouseWheel;
            TextChanged += TextBox_TextChanged;
            PreviewKeyUp += TextBox_PreviewKeyUp;
            LostFocus += CTextBox_LostFocus;

            Padding = new Thickness(4);
        }

        private void CTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ParentCanvas?.CaptureMouse();
        }

        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Keyboard.Focus((IInputElement)e.Source);
        }

        private void TextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                MainWindow.Current.KeyboardDetector.Focus();
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = ((TextBox)e.Source);

            if (textBox.Text == string.Empty)
            {
                ParentCanvas?.Children.Remove(textBox);
            }
        }

        private void TextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                if (e.Delta < 0)
                    ((TextBox)e.Source).FontSize--;
                else if (e.Delta > 0)
                    ((TextBox)e.Source).FontSize++;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                ControlManager.MoveElement(this, (e.Delta < 0) ? -1 : 1);
            }
        }
    }
}
