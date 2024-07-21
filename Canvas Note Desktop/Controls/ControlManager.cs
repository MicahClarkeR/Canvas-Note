using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Canvas_Note_Desktop.Controls
{
    class ControlManager
    {
        public static void MoveElement(FrameworkElement element, int change)
        {
            Canvas canvas = element.Parent as Canvas;
            int indexOf = canvas.Children.IndexOf(element);
            int index = Math.Max(Math.Min(indexOf + change, canvas.Children.Count - 1), 0);

            canvas.Children.Remove(element);
            canvas.Children.Insert(index, element);
        }

        public static void ResizeCanvas(Canvas canvas)
        {
            double MaxX = 1, MaxY = 1;

            foreach(UIElement element in canvas.Children)
            {
                double right = (double)element.GetValue(Canvas.RightProperty);
                double bottom = (double)element.GetValue(Canvas.BottomProperty);

                if (double.IsNaN(0 / right) || double.IsNaN(0 / bottom))
                    continue;
             
                MaxX = Math.Max(MaxX, right);
                MaxY = Math.Max(MaxY, bottom);
            }

            canvas.Width = MaxX + 200;
            canvas.Height = MaxY + 200;
        }
    }
}
