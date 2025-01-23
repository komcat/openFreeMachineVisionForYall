using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OpenCVwpf.ImageClass
{
    public class RectanglePropertyHandler : IPropertyHandler
    {
        private CustomRectangle _rectangle;
        private Dictionary<string, string> _properties;

        public RectanglePropertyHandler()
        {
            _properties = new Dictionary<string, string>();
        }

        public void UpdateProperties(IDrawableObject obj)
        {
            if (obj is CustomRectangle rect)
            {
                _rectangle = rect;
                CalculateProperties();
            }
        }

        private void CalculateProperties()
        {
            _properties.Clear();

            double left = Canvas.GetLeft(_rectangle.MainRectangle);
            double top = Canvas.GetTop(_rectangle.MainRectangle);
            double width = _rectangle.MainRectangle.Width;
            double height = _rectangle.MainRectangle.Height;
            double area = width * height;
            var transform = _rectangle.MainRectangle.RenderTransform as RotateTransform;
            double rotation = transform?.Angle ?? 0;
            Point center = new Point(left + width / 2, top + height / 2);

            _properties["Width"] = $"{width:F2} pixels";
            _properties["Height"] = $"{height:F2} pixels";
            _properties["Area"] = $"{area:F2} pixels²";
            _properties["Position"] = $"({left:F1}, {top:F1})";
            _properties["Rotation"] = $"{rotation:F2}°";
            _properties["Center"] = $"({center.X:F1}, {center.Y:F1})";
        }

        public List<(string Label, string Value)> GetProperties()
        {
            return _properties
                .Select(kvp => (kvp.Key, kvp.Value))
                .ToList();
        }
    }
}