using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace OpenCVwpf.ImageClass
{
    public class LinePropertyHandler : IPropertyHandler
    {
        private CustomLine _line;
        private Dictionary<string, string> _properties;
        private readonly PointDetector _pointDetector;

        public LinePropertyHandler()
        {
            _properties = new Dictionary<string, string>();
            _pointDetector = new PointDetector(20, 5);
        }

        public void UpdateProperties(IDrawableObject obj)
        {
            if (obj is CustomLine line)
            {
                _line = line;
                CalculateProperties();
            }
        }

        private void CalculateProperties()
        {
            _properties.Clear();
            var startPoint = new Point(_line.MainLine.X1, _line.MainLine.Y1);
            var endPoint = new Point(_line.MainLine.X2, _line.MainLine.Y2);

            double length = CalculateLength(startPoint, endPoint);
            double angle = CalculateAngle(startPoint, endPoint);

            _properties["Length"] = $"{length:F2} pixels";
            _properties["Angle"] = $"{angle:F2}°";
            _properties["Start Point"] = $"({_line.MainLine.X1:F1}, {_line.MainLine.Y1:F1})";
            _properties["End Point"] = $"({_line.MainLine.X2:F1}, {_line.MainLine.Y2:F1})";
            _properties["Mid Point"] = $"({(_line.MainLine.X1 + _line.MainLine.X2) / 2:F1}, {(_line.MainLine.Y1 + _line.MainLine.Y2) / 2:F1})";

            var (positions, values) = SamplePixelsAlongLine(_line.MainLine);
            if (positions != null && values != null)
            {
                var points = _pointDetector.DetectPoints(positions, values, startPoint, endPoint);

                if (points.Any())
                {
                    int riseCount = points.Count(p => p.Type == "Rise");
                    int fallCount = points.Count(p => p.Type == "Fall");
                    _properties["Detected Points"] = $"Rise: {riseCount}, Fall: {fallCount}";

                    for (int i = 0; i < points.Count; i++)
                    {
                        var point = points[i];
                        _properties[$"Point {i + 1}"] = $"{point.Type} at ({point.Location.X:F1}, {point.Location.Y:F1})";
                    }
                }
                else
                {
                    _properties["Detected Points"] = "No significant points detected";
                }
            }
        }

        private (double[] positions, double[] values) SamplePixelsAlongLine(System.Windows.Shapes.Line line)
        {
            var canvas = line.Parent as Canvas;
            if (canvas == null) return (null, null);

            var image = canvas.Children.OfType<Image>().FirstOrDefault()?.Source as BitmapSource;
            if (image == null) return (null, null);

            int stride = (image.PixelWidth * image.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[image.PixelHeight * stride];
            image.CopyPixels(pixels, stride, 0);

            double dx = line.X2 - line.X1;
            double dy = line.Y2 - line.Y1;
            double length = Math.Sqrt(dx * dx + dy * dy);
            int numSamples = Math.Max((int)length, 2);

            double[] positions = new double[numSamples];
            double[] values = new double[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                double t = i / (double)(numSamples - 1);
                int x = (int)(line.X1 + t * dx);
                int y = (int)(line.Y1 + t * dy);

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

        private double CalculateLength(Point start, Point end)
        {
            double dx = end.X - start.X;
            double dy = end.Y - start.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private double CalculateAngle(Point start, Point end)
        {
            double dx = end.X - start.X;
            double dy = end.Y - start.Y;
            double angleRad = Math.Atan2(dy, dx);
            return angleRad * (180 / Math.PI);
        }

        public List<(string Label, string Value)> GetProperties()
        {
            return _properties
                .Select(kvp => (kvp.Key, kvp.Value))
                .ToList();
        }
    }
}