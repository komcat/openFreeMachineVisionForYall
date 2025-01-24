using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OpenCVwpf.ImageClass
{
    public class PointMarker
    {
        private readonly Canvas _canvas;
        private readonly List<UIElement> _markers = new();
        private const double MARKER_SIZE = 5;
        private const double STROKE_THICKNESS = 2;

        public PointMarker(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void UpdatePoints(List<(int Position, string Type, Point Location)> points)
        {
            ClearMarkers();

            foreach (var point in points)
            {
                DrawMarker(point.Location, point.Type);
            }
        }

        private void DrawMarker(Point location, string type)
        {
            var line1 = new Line
            {
                X1 = location.X - MARKER_SIZE / 2,
                Y1 = location.Y - MARKER_SIZE / 2,
                X2 = location.X + MARKER_SIZE / 2,
                Y2 = location.Y + MARKER_SIZE / 2,
                Stroke = type == "Rise" ? Brushes.OrangeRed : Brushes.PeachPuff,
                StrokeThickness = STROKE_THICKNESS
            };

            var line2 = new Line
            {
                X1 = location.X - MARKER_SIZE / 2,
                Y1 = location.Y + MARKER_SIZE / 2,
                X2 = location.X + MARKER_SIZE / 2,
                Y2 = location.Y - MARKER_SIZE / 2,
                Stroke = type == "Rise" ? Brushes.OrangeRed : Brushes.PeachPuff,
                StrokeThickness = STROKE_THICKNESS
            };

            _canvas.Children.Add(line1);
            _canvas.Children.Add(line2);
            Panel.SetZIndex(line1, 2);
            Panel.SetZIndex(line2, 2);
            _markers.Add(line1);
            _markers.Add(line2);
        }

        public void ClearMarkers()
        {
            foreach (var marker in _markers)
            {
                _canvas.Children.Remove(marker);
            }
            _markers.Clear();
        }
    }
}