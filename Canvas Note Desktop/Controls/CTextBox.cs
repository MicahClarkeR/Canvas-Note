using Canvas_Note_Desktop.Builders;
using Canvas_Note_Desktop.Factories;
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
using static Canvas_Note_Desktop.Builders.ContextMenuBuilder;

namespace Canvas_Note_Desktop.Controls
{
    class CTextBox : TextBox
    {
        private static readonly Regex UrlRegex = new Regex(@"(http|https)://[^\s/$.?#].[^\s]*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private Canvas? ParentCanvas => (Canvas) Parent;

        public CTextBox()
        {
            Padding = new Thickness(4);
            TextWrapping = TextWrapping.Wrap;
            TextAlignment = TextAlignment.Center;
            TextChanged += CTextBox_TextChanged;
            ContextMenu = BuildMenu();
            AcceptsReturn = true;
            AcceptsTab = true;
        }

        private void CTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Text == string.Empty)
                ParentCanvas.Children.Remove(this);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            if (Keyboard.FocusedElement != this)
            {
                Keyboard.Focus(this);
            }

            // Set caret index to mouse click location.
            if (e.ChangedButton == MouseButton.Left)
            {
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
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (Keyboard.IsKeyDown(Key.F))
                {
                    if (e.Delta < 0)
                        ((TextBox)e.Source).FontSize -= 2;
                    else if (e.Delta > 0)
                        ((TextBox)e.Source).FontSize += 2;
                }
                if(Keyboard.IsKeyDown(Key.W))
                {
                    if (e.Delta < 0)
                        Width = ActualWidth - 25;
                    else if (e.Delta > 0)
                        Width = ActualWidth + 25;
                }
            }
            else if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                ControlManager.MoveElement(this, (e.Delta < 0) ? -1 : 1);
            }
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            base.OnContextMenuOpening(e);
            MatchCollection matches = UrlRegex.Matches(Text);

            if (matches.Count == 0)
                return;

            ContextMenu = BuildMenu();
        }

        private ContextMenu BuildMenu()
        {
            ContextMenuBuilder builder = new ContextMenuBuilder()
                .CreateSubMenu("Text Alignment")
                    .Add("Left", (s, e) => TextAlignment = TextAlignment.Left)
                    .Add("Centre", (s, e) => TextAlignment = TextAlignment.Center)
                    .Add("Right", (s, e) => TextAlignment = TextAlignment.Right)
                    .BuildAndReturn();

            MatchCollection matches = UrlRegex.Matches(Text);

            if (matches.Count > 0)
            {
                SubMenuBuilder urlMenuBuilder = builder.CreateSubMenu("Open Link...");

                foreach (Match match in matches)
                {
                    urlMenuBuilder.Add(match.Value, (s, e) =>
                    {
                        MainWindow.Current.KeyboardDetector.Focus();
                        Task.Run(() => Process.Start(new ProcessStartInfo(match.Value) { UseShellExecute = true }));
                    });
                }

                builder.Add(urlMenuBuilder);
            }

            builder
             .CreateSubMenu("Set..")
                .Add(ContextMenuFactory.CreateMenuItemSubMenu("Font Size...")
                    .Add("8", (s, e) => FontSize = 8)
                    .Add("10", (s, e) => FontSize = 10)
                    .Add("12", (s, e) => FontSize = 12)
                    .Add("16", (s, e) => FontSize = 16)
                    .Add("20", (s, e) => FontSize = 20)
                    .Add("32", (s, e) => FontSize = 32)
                    .Build())


            .BuildAndReturn()
            .CreateSubMenu("Reset...")
                .Add("Width", (s, e) => Width = double.NaN)
                .Add("Font Size", (s, e) => FontSize = 12)
            .BuildAndReturn();

            return builder.Build();
        }
    }
}
