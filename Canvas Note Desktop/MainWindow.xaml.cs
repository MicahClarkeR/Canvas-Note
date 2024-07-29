using Canvas_Note_Desktop.Controls;
using Canvas_Note_Desktop.Factories;
using Canvas_Note_Desktop.Save;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.Mime.MediaTypeNames;

namespace Canvas_Note_Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Fields

        public static MainWindow Current;
        public TextBox KeyboardDetector => KeyboardTextbox;

        private string? _filePath = null;
        private bool _isDragging;
        private UIElement _originalElement;
        private double _originalLeft;
        private double _originalTop;
        private SimpleCircleAdorner _overlayElement;
        private Point _startMmbPoint = new Point(0, 0);
        private Point _startPoint;
        private double ScaleX = 1, ScaleY = 1;

        #endregion Fields

        #region Public Constructors

        public MainWindow()
        {
            InitializeComponent();
            Current = this;
        }

        #endregion Public Constructors

        #region Protected Methods

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            KeyboardManager.Initialise(MainCanvas);
            KeyboardTextbox.Focus();
        }

        #endregion Protected Methods

        #region Private Methods

        private void ClipboardPaste(Point position)
        {
            if (Clipboard.ContainsText())
            {
                if (Keyboard.FocusedElement is CTextBox)
                    return;

                string text = Clipboard.GetText();

                ControlFactory.CreateTextbox(MainCanvas, text, position, focus: true);
            }
            else if (Clipboard.ContainsImage())
            {
                BitmapSource source = Clipboard.GetImage();
                ControlFactory.CreateImage(MainCanvas, source, position);
            }
        }

        private void DragFinished(bool cancelled = false)
        {
            Mouse.Capture(null);
            if (_isDragging)
            {
                AdornerLayer.GetAdornerLayer(_overlayElement.AdornedElement).Remove(_overlayElement);

                if (cancelled == false)
                {
                    var currentPosition = Mouse.GetPosition(MainCanvas);
                    Canvas.SetTop(_originalElement, _originalTop + (currentPosition.Y - _startPoint.Y));
                    Canvas.SetLeft(_originalElement, _originalLeft + (currentPosition.X - _startPoint.X));
                }
                _overlayElement = null;
            }
            _isDragging = false;
        }

        private void DragMoved()
        {
            var currentPosition = Mouse.GetPosition(MainCanvas);

            if (Math.Abs(Point.Subtract(currentPosition, _startPoint).Length) > 2)
            {
                _overlayElement.LeftOffset = (currentPosition.X - _startPoint.X) * ScaleX;
                _overlayElement.TopOffset = (currentPosition.Y - _startPoint.Y) * ScaleX;
            }
        }

        private void DragStarted(Point position, UIElement clicked)
        {
            _isDragging = true;
            _startPoint = position;
            _originalElement = clicked;
            _originalLeft = Canvas.GetLeft(_originalElement);
            _originalTop = Canvas.GetTop(_originalElement);

            _overlayElement = new SimpleCircleAdorner(_originalElement);
            var layer = AdornerLayer.GetAdornerLayer(_originalElement);
            layer.Add(_overlayElement);
        }

        private void HandleLeftButtonDrag(MouseEventArgs e)
        {
            if (!_isDragging && ShouldStartDrag(e, out UIElement? clicked) && clicked != null)
            {
                DragStarted(e.GetPosition(MainCanvas), clicked);
            }
            if (_isDragging)
            {
                DragMoved();
            }
        }

        private void HandleLeftSingleClick(MouseButtonEventArgs e)
        {
            MainCanvas.CaptureMouse();
        }

        private void HandleMiddleButtonDrag(MouseEventArgs e)
        {
            if (Math.Abs(Point.Subtract(e.GetPosition(MainCanvas), _startMmbPoint).Length) > 4)
            {
                MoveCanvasChildren(e);
            }
        }

        private void HandleMiddleMouseButtonDown(MouseButtonEventArgs e)
        {
            _startMmbPoint = e.GetPosition(MainCanvas);
        }

        private async void LoadButton_MouseLeftButtonUp(object sender, EventArgs e)
        {
            LoadFile();
        }

        private void MainCanvas_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))    
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void MainCanvas_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetDataPresent(DataFormats.Text))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void MainCanvas_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] file = (string[])e.Data.GetData(DataFormats.FileDrop);
                var position = e.GetPosition(MainCanvas);
                ControlFactory.CreateImageFromFile(MainCanvas, file[0], position);
            }
            else if (e.Data.GetDataPresent(DataFormats.Text))
            {
                string text = (string)e.Data.GetData(DataFormats.Text);
                var position = e.GetPosition(MainCanvas);
                ControlFactory.CreateTextbox(MainCanvas, text, position, focus: true);
            }
        }

        private void MainCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.Modifiers != ModifierKeys.Control || !KeyboardDetector.IsFocused)
            {
                return;
            }

            ScaleX = Math.Clamp((e.Delta < 0) ? ScaleX - 0.1 : ScaleX + 0.1, 0.3, 2);
            LabelZoom.Content = $"Zoom: {Math.Round(ScaleX, 2)}";

            var mousePosition = Mouse.GetPosition(this);

            var transform = new TransformGroup();
            transform.Children.Add(new TranslateTransform((ActualWidth * ScaleX / 2), (ActualHeight * ScaleX / 2)));
            transform.Children.Add(new ScaleTransform(ScaleX, ScaleX));
            MainCanvas.LayoutTransform = transform;
        }

        private void MainCanvas_PreviewKeyUp(object sender, KeyEventArgs e)
        {
        }

        private void MainCanvas_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                HandleMiddleMouseButtonDown(e);
            }
            else if (e.ChangedButton == MouseButton.Left && e.ClickCount == 1)
            {
                HandleLeftSingleClick(e);
            }
        }
        private void MainCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DragFinished();
        }

        private void MainCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                HandleLeftButtonDrag(e);
            }
            else if (e.MiddleButton == MouseButtonState.Pressed)
            {
                HandleMiddleButtonDrag(e);
            }
        }
        private void MoveCanvasChildren(MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(MainCanvas);
            Vector difference = mousePos - _startMmbPoint;

            foreach (UIElement child in MainCanvas.Children)
            {
                double left = Canvas.GetLeft(child);
                double top = Canvas.GetTop(child);

                Canvas.SetLeft(child, left + difference.X);
                Canvas.SetTop(child, top + difference.Y);
            }

            _startMmbPoint = mousePos;
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            MainCanvas.Children.Clear();
            _filePath = null;
            Title = "Canvas Note";
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            _filePath = null;
            SaveButton_Click(sender, e);
        }

        private async void SaveButton_Click(object sender, EventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                _filePath = CanvasState.SaveCanvas(MainCanvas, _filePath);
                Title = $"{System.IO.Path.GetFileName(_filePath)} — Canvas Note";
            });
        }

        private bool ShouldStartDrag(MouseEventArgs e, out UIElement? clicked)
        {

            Point currentPosition = e.GetPosition(MainCanvas);

            HitTestResult hitTestResult = VisualTreeHelper.HitTest(MainCanvas, currentPosition);
            
            if (hitTestResult == null || hitTestResult.VisualHit == MainCanvas)
            {
                clicked = null;
                return false;
            }

            clicked = GetTopLevelUIElement(hitTestResult.VisualHit);

            return clicked != null &&
                   clicked != MainCanvas && 
                   ((Math.Abs(currentPosition.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                   (Math.Abs(currentPosition.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance));
        }

        private UIElement GetTopLevelUIElement(DependencyObject visualHit)
        {
            while (visualHit != null && (visualHit is not CTextBox && visualHit is not CImage && visualHit is not Canvas))
            {
                visualHit = VisualTreeHelper.GetParent(visualHit);
            }

            return visualHit as UIElement;
        }

        private void Application_Paste(object sender, ExecutedRoutedEventArgs e)
        {
            ClipboardPaste(Mouse.GetPosition(MainCanvas));
        }

        private void KeyboardTextbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
            {
                SaveButton_Click(sender, e);
            }
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.V)
                {
                    ClipboardPaste(Mouse.GetPosition(MainCanvas));
                }
            }
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(MainCanvas, Mouse.GetPosition(MainCanvas));

            if (hitTestResult == null || hitTestResult.VisualHit == MainCanvas)
            {
                KeyboardTextbox.Focus();
            }
        }

        #region Create Callbacks

        private void CreateTextBox_Click(object sender, RoutedEventArgs e)
        {
            ControlFactory.CreateTextbox(MainCanvas, "Text", new Point(ActualWidth / 2, ActualHeight / 2), focus: true);
        }

        private void CreateProduct_Click(object sender, RoutedEventArgs e)
        {
            ControlFactory.CreateProductWithDialog(MainCanvas, Mouse.GetPosition(MainCanvas));
        }

        #endregion Create Callbacks


        public void LoadFile(string? filepath = null)
        {
            Dispatcher.InvokeAsync(() =>
            {
                _filePath = CanvasState.LoadCanvas(MainCanvas, filepath);

                if (_filePath == null || _filePath == string.Empty)
                    return;

                Title = $"{System.IO.Path.GetFileName(_filePath)} — Canvas Note";
            });
        }

        #endregion Private Methods

    }
}