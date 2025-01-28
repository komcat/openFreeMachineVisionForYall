using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Serilog;

namespace OpenCVwpf.ImageClass
{
    public class RectangleDrawer
    {
        private readonly Canvas _canvas;
        private readonly ObjectManager _objectManager;
        private readonly Window _owner;
        private Point _startPoint;
        private Rectangle _previewRect;
        private bool _isDrawingMode;
        private string _currentPurpose;

        public RectangleDrawer(Canvas canvas, ObjectManager objectManager, Window owner)
        {
            _canvas = canvas;
            _objectManager = objectManager;
            _owner = owner;
            _isDrawingMode = false;
        }

        public void StartDrawingMode(string purpose = "AreaMeasurement")
        {
            _currentPurpose = purpose;
            _isDrawingMode = true;
            _canvas.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
            _canvas.MouseMove += Canvas_MouseMove;
            _canvas.MouseLeftButtonUp += Canvas_MouseLeftButtonUp;
            _canvas.Focus();
        }

        public void StopDrawingMode()
        {
            _isDrawingMode = false;
            _canvas.MouseLeftButtonDown -= Canvas_MouseLeftButtonDown;
            _canvas.MouseMove -= Canvas_MouseMove;
            _canvas.MouseLeftButtonUp -= Canvas_MouseLeftButtonUp;
            Log.Information("Rectangle drawing mode stopped");
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isDrawingMode && (e.Source is Canvas || e.Source is Image))
            {
                _startPoint = e.GetPosition(_canvas);

                _previewRect = new Rectangle
                {
                    Stroke = Brushes.Cyan,
                    StrokeThickness = 1,
                    Fill = new SolidColorBrush(Color.FromArgb(30, 0, 255, 255))
                };

                Canvas.SetLeft(_previewRect, _startPoint.X);
                Canvas.SetTop(_previewRect, _startPoint.Y);
                _canvas.Children.Add(_previewRect);
                e.Handled = true;
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawingMode && _previewRect != null)
            {
                Point currentPoint = e.GetPosition(_canvas);

                double width = Math.Abs(currentPoint.X - _startPoint.X);
                double height = Math.Abs(currentPoint.Y - _startPoint.Y);

                // Update preview rectangle size
                _previewRect.Width = width;
                _previewRect.Height = height;

                // Update position if dragging left or up
                Canvas.SetLeft(_previewRect, Math.Min(currentPoint.X, _startPoint.X));
                Canvas.SetTop(_previewRect, Math.Min(currentPoint.Y, _startPoint.Y));

                e.Handled = true;
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDrawingMode && _previewRect != null)
            {
                Point endPoint = e.GetPosition(_canvas);
                _canvas.Children.Remove(_previewRect);

                double width = Math.Abs(endPoint.X - _startPoint.X);
                double height = Math.Abs(endPoint.Y - _startPoint.Y);
                Point topLeft = new Point(
                    Math.Min(endPoint.X, _startPoint.X),
                    Math.Min(endPoint.Y, _startPoint.Y)
                );

                if (width > 5 && height > 5)
                {
                    var factory = RectangleFactories.GetFactory(_currentPurpose);
                    var rectangle = factory.CreateRectangle(_canvas, topLeft, width, height, _objectManager);

                    rectangle.AddToCanvas();
                    _objectManager.AddObject(rectangle, _owner);
                    _objectManager.SelectObject(rectangle);
                }

                _previewRect = null;
                e.Handled = true;
            }
        }
    }
}