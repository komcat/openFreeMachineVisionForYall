using Serilog;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OpenCVwpf.ImageClass
{
    public class CustomLine : IDrawableObject
    {
        private readonly Canvas _canvas;
        private readonly ObjectManager _objectManager;
        public Line MainLine { get; private set; }
        public Rectangle StartPoint { get; private set; }
        public Rectangle MidPoint { get; private set; }
        public Rectangle EndPoint { get; private set; }
        private Rectangle _hitArea;
        private TextBlock _nameLabel;

        // Add to class fields
        private readonly PointMarker _pointMarker;
        private readonly PointDetector _pointDetector;

        private bool _hitAreaEnabled = true;

        public bool IsSelected { get; private set; }
        private string _name;
        public event EventHandler LinePositionChanged;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                if (_nameLabel != null)
                {
                    _nameLabel.Text = value;
                }
            }
        }
        public string ObjectType => "Line";

        private const double CONTROL_POINT_SIZE = 10;
        private const double HIT_TOLERANCE = 5.0;

        public CustomLine(Canvas canvas, Point start, Point end, ObjectManager objectManager)
        {
            Log.Debug("Creating new CustomLine at Start: ({StartX}, {StartY}), End: ({EndX}, {EndY})",
                start.X, start.Y, end.X, end.Y);

            _canvas = canvas;
            _objectManager = objectManager;

            MainLine = new Line
            {
                Stroke = Brushes.Cyan,
                StrokeThickness = 0.5,
                X1 = start.X,
                Y1 = start.Y,
                X2 = end.X,
                Y2 = end.Y
            };

            _nameLabel = new TextBlock
            {
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                Padding = new Thickness(2),
                FontSize = 12
            };

            _hitArea = new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromArgb(0, 255, 0, 0)),  // Fully transparent
                Stroke = null,  // Remove border
                StrokeThickness = 0.5,
                Cursor = Cursors.Hand
            };

            _hitArea.MouseLeftButtonDown += Line_MouseLeftButtonDown;
            MainLine.MouseLeftButtonDown += Line_MouseLeftButtonDown;

            _hitArea.MouseEnter += (s, e) => {
                MainLine.StrokeThickness = 0.5;
                //Log.Debug("Mouse entered hit area - Line: {LineName}", Name);
            };

            _hitArea.MouseLeave += (s, e) => {
                if (!IsSelected) MainLine.StrokeThickness = 0.5;
                //Log.Debug("Mouse left hit area - Line: {LineName}", Name);
            };

            StartPoint = CreateControlPoint(start);
            EndPoint = CreateControlPoint(end);
            MidPoint = CreateControlPoint(GetMidPoint());

            UpdateHitArea();
            UpdateLabelPosition();
            OnLinePositionChanged();

            _canvas.Children.Add(_hitArea);
            Panel.SetZIndex(_hitArea, 0);
            Panel.SetZIndex(_nameLabel, 1);

            HideControlPoints();


            _pointMarker = new PointMarker(canvas);
            _pointDetector = new PointDetector(20, 5);

            // Add after UpdateHitArea() and UpdateLabelPosition()
            UpdatePointMarkers();
        }


        public void SetHitAreaEnabled(bool enabled)
        {
            _hitAreaEnabled = enabled;
            if (_hitArea != null)
            {
                _hitArea.IsHitTestVisible = enabled;
                _hitArea.Visibility = enabled ? Visibility.Visible : Visibility.Hidden;
            }
        }

        // Add new method
        private void UpdatePointMarkers()
        {
            var canvas = MainLine.Parent as Canvas;
            if (canvas == null) return;

            var image = canvas.Children.OfType<Image>().FirstOrDefault()?.Source as BitmapSource;
            if (image == null) return;

            var (positions, values) = SamplePixelsAlongLine();
            if (positions != null && values != null)
            {
                var points = _pointDetector.DetectPoints(positions, values,
                    new Point(MainLine.X1, MainLine.Y1),
                    new Point(MainLine.X2, MainLine.Y2));
                _pointMarker.UpdatePoints(points);
            }
        }
        private void UpdateHitArea()
        {
            double minX = Math.Min(MainLine.X1, MainLine.X2);
            double maxX = Math.Max(MainLine.X1, MainLine.X2);
            double minY = Math.Min(MainLine.Y1, MainLine.Y2);
            double maxY = Math.Max(MainLine.Y1, MainLine.Y2);

            double padding = HIT_TOLERANCE;
            double width = maxX - minX + (padding * 2);
            double height = maxY - minY + (padding * 2);

            double left = Math.Max(0, minX - padding);
            double top = Math.Max(0, minY - padding);

            _hitArea.Width = width;
            _hitArea.Height = height;

            Canvas.SetLeft(_hitArea, left);
            Canvas.SetTop(_hitArea, top);

            //Log.Debug("Hit area updated - Width: {Width}, Height: {Height}, Position: ({X}, {Y})",
            //    _hitArea.Width, _hitArea.Height, left, top);
        }

        private void UpdateLabelPosition()
        {
            Point midPoint = GetMidPoint();
            double angle = Math.Atan2(MainLine.Y2 - MainLine.Y1, MainLine.X2 - MainLine.X1);
            double offsetY = angle > -Math.PI / 2 && angle < Math.PI / 2 ? -20 : 10;

            Canvas.SetLeft(_nameLabel, midPoint.X - (_nameLabel.ActualWidth / 2));
            Canvas.SetTop(_nameLabel, midPoint.Y + offsetY);
        }

        private Point GetMidPoint()
        {
            return new Point(
                (MainLine.X1 + MainLine.X2) / 2,
                (MainLine.Y1 + MainLine.Y2) / 2
            );
        }

        public void UpdateControlPointsPosition()
        {
            Canvas.SetLeft(StartPoint, MainLine.X1 - CONTROL_POINT_SIZE / 2);
            Canvas.SetTop(StartPoint, MainLine.Y1 - CONTROL_POINT_SIZE / 2);

            Canvas.SetLeft(EndPoint, MainLine.X2 - CONTROL_POINT_SIZE / 2);
            Canvas.SetTop(EndPoint, MainLine.Y2 - CONTROL_POINT_SIZE / 2);

            Point mid = GetMidPoint();
            Canvas.SetLeft(MidPoint, mid.X - CONTROL_POINT_SIZE / 2);
            Canvas.SetTop(MidPoint, mid.Y - CONTROL_POINT_SIZE / 2);

            UpdateHitArea();
            UpdateLabelPosition();
        }

        private Rectangle CreateControlPoint(Point position)
        {
            var point = new Rectangle
            {
                Width = CONTROL_POINT_SIZE,
                Height = CONTROL_POINT_SIZE,
                Fill = Brushes.Cyan,
                Stroke = Brushes.White,
                StrokeThickness = 1
            };

            Canvas.SetLeft(point, position.X - CONTROL_POINT_SIZE / 2);
            Canvas.SetTop(point, position.Y - CONTROL_POINT_SIZE / 2);

            point.MouseLeftButtonDown += ControlPoint_MouseLeftButtonDown;
            point.MouseLeftButtonUp += ControlPoint_MouseLeftButtonUp;
            point.MouseMove += ControlPoint_MouseMove;

            return point;
        }

        private bool isDragging = false;
        private Point dragStart;
        private Rectangle draggedPoint;

        private void ControlPoint_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var point = sender as Rectangle;
            if (point != null)
            {
                isDragging = true;
                draggedPoint = point;
                dragStart = e.GetPosition(_canvas);
                point.CaptureMouse();
                e.Handled = true;
            }
        }

        private void ControlPoint_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point currentPos = e.GetPosition(_canvas);

                if (draggedPoint == StartPoint)
                {
                    MainLine.X1 = currentPos.X;
                    MainLine.Y1 = currentPos.Y;
                }
                else if (draggedPoint == EndPoint)
                {
                    MainLine.X2 = currentPos.X;
                    MainLine.Y2 = currentPos.Y;
                }
                else if (draggedPoint == MidPoint)
                {
                    double deltaX = currentPos.X - dragStart.X;
                    double deltaY = currentPos.Y - dragStart.Y;

                    MainLine.X1 += deltaX;
                    MainLine.Y1 += deltaY;
                    MainLine.X2 += deltaX;
                    MainLine.Y2 += deltaY;

                    dragStart = currentPos;
                }

                UpdateControlPointsPosition();
                UpdatePointMarkers();
                LinePositionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ControlPoint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                var point = sender as Rectangle;
                point?.ReleaseMouseCapture();
                isDragging = false;
            }
        }

        private void Line_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPoint = e.GetPosition(_canvas);

            if (IsPointNearLine(clickPoint))
            {
                Log.Debug("Click near line - Line: {LineName}", Name);
                _objectManager.SelectObject(this);
                e.Handled = true;
            }
        }

        private bool IsPointNearLine(Point point)
        {
            double lineLength = Math.Sqrt(
                Math.Pow(MainLine.X2 - MainLine.X1, 2) +
                Math.Pow(MainLine.Y2 - MainLine.Y1, 2));

            if (lineLength == 0) return false;

            double distance = Math.Abs(
                (MainLine.X2 - MainLine.X1) * (MainLine.Y1 - point.Y) -
                (MainLine.X1 - point.X) * (MainLine.Y2 - MainLine.Y1)
            ) / lineLength;

            return distance <= HIT_TOLERANCE;
        }

        public void Select()
        {
            IsSelected = true;
            MainLine.Stroke = Brushes.Orange;
            _nameLabel.Foreground = Brushes.Orange;
            ShowControlPoints();
            UpdatePointMarkers();  // Add this line
        }

        public void Deselect()
        {
            IsSelected = false;
            MainLine.Stroke = Brushes.Cyan;
            _nameLabel.Foreground = Brushes.White;
            HideControlPoints();
        }

        private void ShowControlPoints()
        {
            StartPoint.Visibility = Visibility.Visible;
            MidPoint.Visibility = Visibility.Visible;
            EndPoint.Visibility = Visibility.Visible;
        }

        private void HideControlPoints()
        {
            StartPoint.Visibility = Visibility.Hidden;
            MidPoint.Visibility = Visibility.Hidden;
            EndPoint.Visibility = Visibility.Hidden;
        }

        public void AddToCanvas()
        {
            _canvas.Children.Add(MainLine);
            _canvas.Children.Add(StartPoint);
            _canvas.Children.Add(MidPoint);
            _canvas.Children.Add(EndPoint);
            _canvas.Children.Add(_nameLabel);
        }

        public void RemoveFromCanvas()
        {
            _pointMarker.ClearMarkers();
            _canvas.Children.Remove(MainLine);
            _canvas.Children.Remove(StartPoint);
            _canvas.Children.Remove(MidPoint);
            _canvas.Children.Remove(EndPoint);
            _canvas.Children.Remove(_nameLabel);
            _canvas.Children.Remove(_hitArea);
        }

        public void UpdatePosition()
        {
            UpdateControlPointsPosition();
        }

        protected virtual void OnLinePositionChanged()
        {
            LinePositionChanged?.Invoke(this, EventArgs.Empty);
        }

        // Add helper method
        private (double[] positions, double[] values) SamplePixelsAlongLine()
        {
            var canvas = MainLine.Parent as Canvas;
            if (canvas == null) return (null, null);

            var image = canvas.Children.OfType<Image>().FirstOrDefault()?.Source as BitmapSource;
            if (image == null) return (null, null);

            int stride = (image.PixelWidth * image.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[image.PixelHeight * stride];
            image.CopyPixels(pixels, stride, 0);

            double dx = MainLine.X2 - MainLine.X1;
            double dy = MainLine.Y2 - MainLine.Y1;
            double length = Math.Sqrt(dx * dx + dy * dy);
            int numSamples = Math.Max((int)length, 2);

            double[] positions = new double[numSamples];
            double[] values = new double[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                double t = i / (double)(numSamples - 1);
                int x = (int)(MainLine.X1 + t * dx);
                int y = (int)(MainLine.Y1 + t * dy);

                x = Math.Max(0, Math.Min(x, image.PixelWidth - 1));
                y = Math.Max(0, Math.Min(y, image.PixelHeight - 1));

                int index = y * stride + x * (image.Format.BitsPerPixel / 8);
                if (index < pixels.Length)
                {
                    values[i] = pixels[index];
                }
                positions[i] = i;
            }

            return (positions, values);
        }
    }
}