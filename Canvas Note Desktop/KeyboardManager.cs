using H.Hooks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Canvas_Note_Desktop
{
    internal class KeyboardManager
    {
        public static event RoutedEventHandler OnPasteDown
        {
            add { MainCanvas.AddHandler(OnPasteDownEvent, value); }
            remove { MainCanvas.RemoveHandler(OnPasteDownEvent, value); }
        }
        public static readonly RoutedEvent OnPasteDownEvent = EventManager.RegisterRoutedEvent(
            name: "Tap",
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(Canvas)
        );

        private static Canvas MainCanvas;

        public static void Initialise(Canvas canvas)
        {
            MainCanvas = canvas;
        }

        private static void KeyboardKeyDown(object sender, KeyEventArgs e)
        {
            /*if(Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.V)
                {
                    MainCanvas.RaiseEvent(new RoutedEventArgs(OnPasteDownEvent, sender));
                }
            }*/
        }
    }
}
