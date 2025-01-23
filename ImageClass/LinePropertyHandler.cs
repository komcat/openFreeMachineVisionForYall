using System.Windows;
using System.Windows.Controls;

namespace OpenCVwpf.ImageClass
{
    public class LinePropertyHandler : IPropertyHandler
    {
        private CustomLine _line;
        private Dictionary<string, string> _properties;

        public LinePropertyHandler()
        {
            _properties = new Dictionary<string, string>();
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