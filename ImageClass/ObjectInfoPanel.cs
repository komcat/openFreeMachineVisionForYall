using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace OpenCVwpf.ImageClass
{
    public class ObjectInfoPanel
    {
        private readonly StackPanel _panel;
        private readonly Label _titleLabel;
        private readonly Grid _infoGrid;
        private readonly Dictionary<string, IPropertyHandler> _propertyHandlers;
        private IDrawableObject _currentObject;

        public ObjectInfoPanel(StackPanel panel)
        {
            _panel = panel;
            _panel.Background = Brushes.WhiteSmoke;

            _titleLabel = new Label
            {
                Content = "Object Information",
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _panel.Children.Add(_titleLabel);

            _infoGrid = new Grid();
            _infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            _infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _panel.Children.Add(_infoGrid);

            // Initialize property handlers
            _propertyHandlers = new Dictionary<string, IPropertyHandler>
            {
                { "Rectangle", new RectanglePropertyHandler() },
                { "Line", new LinePropertyHandler() }
            };
        }

        public void UpdateInfo(IDrawableObject obj)
        {
            UnsubscribeFromCurrentObject();

            _currentObject = obj;
            if (_currentObject is CustomLine line)
            {
                line.LinePositionChanged += Object_PositionChanged;
            }
            if (_currentObject is CustomRectangle rect)
            {
                rect.RectanglePositionChanged += Object_PositionChanged;
            }

            UpdateDisplay();
        }

        private void Object_PositionChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void UnsubscribeFromCurrentObject()
        {
            if (_currentObject is CustomLine line)
            {
                line.LinePositionChanged -= Object_PositionChanged;
            }
            if (_currentObject is CustomRectangle rect)
            {
                rect.RectanglePositionChanged -= Object_PositionChanged;
            }
            _currentObject = null;
        }


        private void UpdateDisplay()
        {
            _infoGrid.Children.Clear();
            _infoGrid.RowDefinitions.Clear();

            if (_currentObject == null)
            {
                AddInfoRow("No object selected", "");
                return;
            }

            // Display common properties
            AddInfoRow("Type:", _currentObject.ObjectType);
            AddInfoRow("Name:", _currentObject.Name);
            AddInfoRow("Purpose:", _currentObject.Purpose);

            // Get purpose-specific data
            var data = _currentObject.GetOutputData();

            switch (_currentObject.Purpose)
            {
                case "PointDetection":
                    DisplayPointDetectionInfo(data);
                    break;
                case "AreaMeasurement":
                    DisplayAreaMeasurementInfo(data);
                    break;
                default:
                    DisplayGenericInfo(data);
                    break;
            }
        }

        private void DisplayPointDetectionInfo(Dictionary<string, object> data)
        {
            if (data.TryGetValue("Length", out var length))
                AddInfoRow("Length:", $"{length:F2} pixels");

            if (data.TryGetValue("Angle", out var angle))
                AddInfoRow("Angle:", $"{angle:F2}°");

            if (data.TryGetValue("DetectedPoints", out var points))
            {
                var detectedPoints = points as List<(int Position, string Type, Point Location)>;
                if (detectedPoints != null)
                {
                    int riseCount = detectedPoints.Count(p => p.Type == "Rise");
                    int fallCount = detectedPoints.Count(p => p.Type == "Fall");
                    AddInfoRow("Detected Points:", $"Rise: {riseCount}, Fall: {fallCount}");

                    for (int i = 0; i < detectedPoints.Count; i++)
                    {
                        var point = detectedPoints[i];
                        AddInfoRow($"Point {i + 1}:",
                            $"{point.Type} at ({point.Location.X:F1}, {point.Location.Y:F1})");
                    }
                }
            }
        }

        private void DisplayAreaMeasurementInfo(Dictionary<string, object> data)
        {
            if (data.TryGetValue("Width", out var width))
                AddInfoRow("Width:", $"{width:F2} pixels");

            if (data.TryGetValue("Height", out var height))
                AddInfoRow("Height:", $"{height:F2} pixels");

            if (data.TryGetValue("Area", out var area))
                AddInfoRow("Area:", $"{area:F2} pixels²");

            if (data.TryGetValue("Perimeter", out var perimeter))
                AddInfoRow("Perimeter:", $"{perimeter:F2} pixels");

            if (data.TryGetValue("Rotation", out var rotation))
                AddInfoRow("Rotation:", $"{rotation:F2}°");

            if (data.TryGetValue("Center", out var center) && center is Point centerPoint)
                AddInfoRow("Center:", $"({centerPoint.X:F1}, {centerPoint.Y:F1})");
        }

        private void DisplayGenericInfo(Dictionary<string, object> data)
        {
            foreach (var kvp in data)
            {
                AddInfoRow(kvp.Key + ":", kvp.Value.ToString());
            }
        }

        private void AddInfoRow(string label, string value)
        {
            int rowIndex = _infoGrid.RowDefinitions.Count;
            _infoGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var labelText = new Label
            {
                Content = label,
                FontWeight = FontWeights.SemiBold
            };
            Grid.SetRow(labelText, rowIndex);
            Grid.SetColumn(labelText, 0);

            var valueText = new Label { Content = value };
            Grid.SetRow(valueText, rowIndex);
            Grid.SetColumn(valueText, 1);

            _infoGrid.Children.Add(labelText);
            _infoGrid.Children.Add(valueText);
        }

        public void Clear()
        {
            UnsubscribeFromCurrentObject();
            _infoGrid.Children.Clear();
            _infoGrid.RowDefinitions.Clear();
            AddInfoRow("No object selected", "");
        }
    }
}