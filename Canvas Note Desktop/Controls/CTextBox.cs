using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Canvas_Note_Desktop.Controls
{
    class CTextBox : TextBox
    {
        private static readonly Regex UrlRegex = new Regex(@"(http|https)://[^\s/$.?#].[^\s]*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private Canvas? ParentCanvas => (Canvas)this.Parent;

        public CTextBox()
        {
            Padding = new Thickness(4);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            if (Keyboard.FocusedElement != this)
            {
                Keyboard.Focus(this);

                Point mousePosition = e.GetPosition(this);
                int charIndex = GetCharacterIndexFromPoint(mousePosition);
                CaretIndex = charIndex;
            }
        }

        private int GetCharacterIndexFromPoint(Point point)
        {
            // Get the character index at the specified point
            int charIndex = this.GetCharacterIndexFromPoint(point, true);

            // If the point is not inside any character, charIndex will be -1
            if (charIndex != -1)
            {
                // Adjust the character index based on the nearest character
                Rect charRect = this.GetRectFromCharacterIndex(charIndex);
                if (point.X > charRect.Right)
                {
                    charIndex++;
                }
            }

            return charIndex;
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                MainWindow.Current.KeyboardDetector.Focus();

                if (Text == string.Empty)
                {
                    ParentCanvas?.Children.Remove(this);
                }
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
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

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            base.OnContextMenuOpening(e);

            ContextMenu = null;
            MatchCollection matches = UrlRegex.Matches(Text);

            if (matches.Count == 0)
                return;

            ContextMenu = new ContextMenu();

            foreach(Match match in matches)
            {
                MenuItem item = new MenuItem()
                {
                    Header = match.Value
                };

                item.Click += (s, e) =>
                {
                    MainWindow.Current.KeyboardDetector.Focus();
                    Task.Run(() => Process.Start(new ProcessStartInfo(match.Value) { UseShellExecute = true }));
                };

                ContextMenu.Items.Add(item);
            }
        }
    }
}
