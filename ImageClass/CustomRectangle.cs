using Serilog;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OpenCVwpf.ImageClass
{
    public class CustomRectangle : IDrawableObject
    {
        private readonly Canvas _canvas;
        private readonly ObjectManager _objectManager;
        public Rectangle MainRectangle { get; private set; }
        private TextBlock _nameLabel;
        private string _name;
        private bool _isSelected;
        private readonly List<Rectangle> _controlPoints;
        private Rectangle _rotationHandle;
        private Rectangle _centroid;
        // Add these fields at the top of CustomRectangle class
        private double _rotationAngle = 0;
        private RotateTransform _rotateTransform;
        private const double CONTROL_POINT_SIZE = 10;
        private Point dragStart;
        public virtual string Purpose => "AreaMeasurement";
        public event EventHandler RectanglePositionChanged;
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

        public string ObjectType => "Rectangle";
        public bool IsSelected => _isSelected;

        public CustomRectangle(Canvas canvas, Point topLeft, double width, double height, ObjectManager objectManager)
        {
            _canvas = canvas;
            _objectManager = objectManager;
            _controlPoints = new List<Rectangle>();

            // Create main rectangle
            MainRectangle = new Rectangle
            {
                Width = width,
                Height = height,
                Stroke = Brushes.Cyan,
                StrokeThickness = 1,
                //Fill = new SolidColorBrush(Color.FromArgb(30, 0, 255, 255))
            };

            // Set initial position
            Canvas.SetLeft(MainRectangle, topLeft.X);
            Canvas.SetTop(MainRectangle, topLeft.Y);

            // Create name label
            _nameLabel = new TextBlock
            {
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                Padding = new Thickness(2),
                FontSize = 12
            };

            // Initialize control points
            InitializeControlPoints();

            // Add mouse events
            MainRectangle.MouseLeftButtonDown += Shape_MouseLeftButtonDown;
        }

        // Update InitializeControlPoints()
        private void InitializeControlPoints()
        {
            // Create rotation transform
            _rotateTransform = new RotateTransform(0);
            MainRectangle.RenderTransform = _rotateTransform;

            // Rest remains same
            for (int i = 0; i < 4; i++)
            {
                var point = CreateControlPoint();
                _controlPoints.Add(point);
            }

            _centroid = CreateControlPoint();
            _centroid.Fill = Brushes.Yellow;

            _rotationHandle = CreateControlPoint();
            _rotationHandle.Fill = Brushes.Red;

            UpdateControlPointsPosition();
        }

        private Rectangle CreateControlPoint()
        {
            var point = new Rectangle
            {
                Width = CONTROL_POINT_SIZE,
                Height = CONTROL_POINT_SIZE,
                Fill = Brushes.Cyan,
                Stroke = Brushes.White,
                StrokeThickness = 1,
                Visibility = Visibility.Hidden
            };

            point.MouseLeftButtonDown += ControlPoint_MouseLeftButtonDown;
            point.MouseLeftButtonUp += ControlPoint_MouseLeftButtonUp;
            point.MouseMove += ControlPoint_MouseMove;

            return point;
        }

        // Update UpdateControlPointsPosition() to handle rotation
        private void UpdateControlPointsPosition()
        {
            double left = Canvas.GetLeft(MainRectangle);
            double top = Canvas.GetTop(MainRectangle);
            double right = left + MainRectangle.Width;
            double bottom = top + MainRectangle.Height;
            Point center = new Point(left + MainRectangle.Width / 2, top + MainRectangle.Height / 2);

            // Update corner points with rotation
            UpdateRotatedControlPoint(_controlPoints[0], left, top, center);
            UpdateRotatedControlPoint(_controlPoints[1], right, top, center);
            UpdateRotatedControlPoint(_controlPoints[2], right, bottom, center);
            UpdateRotatedControlPoint(_controlPoints[3], left, bottom, center);

            // Update centroid (no rotation needed)
            UpdateControlPoint(_centroid, center.X, center.Y);

            // Update rotation handle with rotation
            double handleDistance = 30;
            double handleAngle = _rotationAngle * (Math.PI / 180);
            double handleX = center.X + handleDistance * Math.Sin(handleAngle);
            double handleY = center.Y - handleDistance * Math.Cos(handleAngle);
            UpdateControlPoint(_rotationHandle, handleX, handleY);

            // Update label position
            Canvas.SetLeft(_nameLabel, center.X - _nameLabel.ActualWidth / 2);
            Canvas.SetTop(_nameLabel, bottom + 5);
        }
        private void UpdateRotatedControlPoint(Rectangle point, double x, double y, Point center)
        {
            double angle = _rotationAngle * (Math.PI / 180);
            double rotatedX = center.X + (x - center.X) * Math.Cos(angle) - (y - center.Y) * Math.Sin(angle);
            double rotatedY = center.Y + (x - center.X) * Math.Sin(angle) + (y - center.Y) * Math.Cos(angle);
            UpdateControlPoint(point, rotatedX, rotatedY);
        }
        private void UpdateControlPoint(Rectangle point, double x, double y)
        {
            Canvas.SetLeft(point, x - CONTROL_POINT_SIZE / 2);
            Canvas.SetTop(point, y - CONTROL_POINT_SIZE / 2);
        }

        private void Shape_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _objectManager.SelectObject(this);
            e.Handled = true;
        }

        private void ControlPoint_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var point = sender as Rectangle;
            if (point != null)
            {
                dragStart = e.GetPosition(_canvas);
                point.CaptureMouse();

                if (point == _centroid)
                {
                    double rectLeft = Canvas.GetLeft(MainRectangle);
                    double rectTop = Canvas.GetTop(MainRectangle);
                    Point rectCenter = new Point(
                        rectLeft + MainRectangle.Width / 2,
                        rectTop + MainRectangle.Height / 2
                    );
                    Log.Debug($"Centroid drag start - Mouse: ({dragStart.X:F1}, {dragStart.Y:F1}), Rectangle Center: ({rectCenter.X:F1}, {rectCenter.Y:F1})");
                }
            }
            e.Handled = true;
        }

        private void ControlPoint_MouseMove(object sender, MouseEventArgs e)
        {
            var point = sender as Rectangle;
            if (point != null && point.IsMouseCaptured)
            {
                Point currentPos = e.GetPosition(_canvas);

                if (point == _rotationHandle)
                {
                    HandleRotation(currentPos);
                    return;
                }

                if (point == _centroid)
                {
                    HandleCentroidDrag(currentPos);
                    return;
                }

                // Handle corner resizing with rotation
                Point center = GetRectangleCenter();
                Point unrotatedCurrent = RotatePoint(currentPos, center, -_rotationAngle);
                double left = Canvas.GetLeft(MainRectangle);
                double top = Canvas.GetTop(MainRectangle);
                double right = left + MainRectangle.Width;
                double bottom = top + MainRectangle.Height;

                if (point == _controlPoints[0]) // Top-left
                {
                    MainRectangle.Width = right - unrotatedCurrent.X;
                    MainRectangle.Height = bottom - unrotatedCurrent.Y;
                    Canvas.SetLeft(MainRectangle, unrotatedCurrent.X);
                    Canvas.SetTop(MainRectangle, unrotatedCurrent.Y);
                }
                else if (point == _controlPoints[1]) // Top-right
                {
                    MainRectangle.Width = unrotatedCurrent.X - left;
                    MainRectangle.Height = bottom - unrotatedCurrent.Y;
                    Canvas.SetTop(MainRectangle, unrotatedCurrent.Y);
                }
                else if (point == _controlPoints[2]) // Bottom-right
                {
                    MainRectangle.Width = unrotatedCurrent.X - left;
                    MainRectangle.Height = unrotatedCurrent.Y - top;
                }
                else if (point == _controlPoints[3]) // Bottom-left
                {
                    MainRectangle.Width = right - unrotatedCurrent.X;
                    MainRectangle.Height = unrotatedCurrent.Y - top;
                    Canvas.SetLeft(MainRectangle, unrotatedCurrent.X);
                }

                // Ensure minimum size
                MainRectangle.Width = Math.Max(MainRectangle.Width, 10);
                MainRectangle.Height = Math.Max(MainRectangle.Height, 10);

                UpdateControlPointsPosition();
                UpdateRotation();
                OnRectanglePositionChanged(); // Add this
            }
        }


        private Point RotatePoint(Point point, Point center, double angleDegrees)
        {
            double angleRadians = angleDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleRadians);
            double sinTheta = Math.Sin(angleRadians);

            double dx = point.X - center.X;
            double dy = point.Y - center.Y;

            return new Point(
                center.X + (dx * cosTheta - dy * sinTheta),
                center.Y + (dx * sinTheta + dy * cosTheta)
            );
        }

        private Point GetRectangleCenter()
        {
            double left = Canvas.GetLeft(MainRectangle);
            double top = Canvas.GetTop(MainRectangle);
            return new Point(
                left + MainRectangle.Width / 2,
                top + MainRectangle.Height / 2
            );
        }

        private void HandleRotation(Point currentPos)
        {
            Point center = GetRectangleCenter();
            double prevAngle = Math.Atan2(dragStart.Y - center.Y, dragStart.X - center.X);
            double newAngle = Math.Atan2(currentPos.Y - center.Y, currentPos.X - center.X);
            double deltaAngle = (newAngle - prevAngle) * (180 / Math.PI);

            _rotationAngle += deltaAngle;
            UpdateRotation();
            dragStart = currentPos;
            OnRectanglePositionChanged(); // Add this
        }

        private void HandleCentroidDrag(Point currentPos)
        {
            Point center = GetRectangleCenter();
            double deltaX = currentPos.X - center.X;
            double deltaY = currentPos.Y - center.Y;
            Canvas.SetLeft(MainRectangle, Canvas.GetLeft(MainRectangle) + deltaX);
            Canvas.SetTop(MainRectangle, Canvas.GetTop(MainRectangle) + deltaY);
            dragStart = currentPos;
            UpdateControlPointsPosition();
            OnRectanglePositionChanged(); // Add this
        }

        // Add new method for updating rotation
        private void UpdateRotation()
        {
            // Get rectangle center
            double rectLeft = Canvas.GetLeft(MainRectangle);
            double rectTop = Canvas.GetTop(MainRectangle);
            Point center = new Point(
                rectLeft + MainRectangle.Width / 2,
                rectTop + MainRectangle.Height / 2
            );

            // Update main rectangle rotation
            _rotateTransform.Angle = _rotationAngle;
            _rotateTransform.CenterX = MainRectangle.Width / 2;
            _rotateTransform.CenterY = MainRectangle.Height / 2;

            // Update control points positions with rotation
            UpdateControlPointsPosition();
        }
        private void ControlPoint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var point = sender as Rectangle;
            point?.ReleaseMouseCapture();
        }

        public void Select()
        {
            _isSelected = true;
            MainRectangle.Stroke = Brushes.Orange;
            _nameLabel.Foreground = Brushes.Orange;
            ShowControlPoints();
        }

        public void Deselect()
        {
            _isSelected = false;
            MainRectangle.Stroke = Brushes.Cyan;
            _nameLabel.Foreground = Brushes.White;
            HideControlPoints();
        }

        private void ShowControlPoints()
        {
            foreach (var point in _controlPoints)
            {
                point.Visibility = Visibility.Visible;
            }
            _centroid.Visibility = Visibility.Visible;
            _rotationHandle.Visibility = Visibility.Visible;
        }

        private void HideControlPoints()
        {
            foreach (var point in _controlPoints)
            {
                point.Visibility = Visibility.Hidden;
            }
            _centroid.Visibility = Visibility.Hidden;
            _rotationHandle.Visibility = Visibility.Hidden;
        }

        public void AddToCanvas()
        {
            _canvas.Children.Add(MainRectangle);
            foreach (var point in _controlPoints)
            {
                _canvas.Children.Add(point);
            }
            _canvas.Children.Add(_centroid);
            _canvas.Children.Add(_rotationHandle);
            _canvas.Children.Add(_nameLabel);
        }

        public virtual void RemoveFromCanvas()
        {
            _canvas.Children.Remove(MainRectangle);
            foreach (var point in _controlPoints)
            {
                _canvas.Children.Remove(point);
            }
            _canvas.Children.Remove(_centroid);
            _canvas.Children.Remove(_rotationHandle);
            _canvas.Children.Remove(_nameLabel);
        }

        public void UpdatePosition()
        {
            UpdateControlPointsPosition();
        }
        protected virtual void OnRectanglePositionChanged()
        {
            RectanglePositionChanged?.Invoke(this, EventArgs.Empty);
        }



        public virtual Dictionary<string, object> GetOutputData()
        {
            var data = new Dictionary<string, object>();

            double left = Canvas.GetLeft(MainRectangle);
            double top = Canvas.GetTop(MainRectangle);
            Point center = GetRectangleCenter();

            data["Width"] = MainRectangle.Width;
            data["Height"] = MainRectangle.Height;
            data["Area"] = MainRectangle.Width * MainRectangle.Height;
            data["Position"] = new Point(left, top);
            data["Center"] = center;
            data["Rotation"] = _rotationAngle;

            // Calculate corners with rotation
            var corners = new Point[4];
            corners[0] = RotatePoint(new Point(left, top), center, _rotationAngle);
            corners[1] = RotatePoint(new Point(left + MainRectangle.Width, top), center, _rotationAngle);
            corners[2] = RotatePoint(new Point(left + MainRectangle.Width, top + MainRectangle.Height), center, _rotationAngle);
            corners[3] = RotatePoint(new Point(left, top + MainRectangle.Height), center, _rotationAngle);
            data["Corners"] = corners;

            // Calculate perimeter
            double perimeter = 2 * (MainRectangle.Width + MainRectangle.Height);
            data["Perimeter"] = perimeter;

            return data;
        }
    }
}