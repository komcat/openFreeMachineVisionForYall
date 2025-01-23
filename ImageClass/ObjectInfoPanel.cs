using System.Windows.Controls;
using System.Windows;

namespace OpenCVwpf.ImageClass
{
    public class ObjectInfoPanel
    {
        private readonly StackPanel _panel;
        private readonly Label _titleLabel;
        private readonly Grid _infoGrid;

        public ObjectInfoPanel(StackPanel panel)
        {
            _panel = panel;
            _panel.Background = System.Windows.Media.Brushes.WhiteSmoke;

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
        }

        public void UpdateInfo(IDrawableObject obj)
        {
            _infoGrid.Children.Clear();
            _infoGrid.RowDefinitions.Clear();

            if (obj == null)
            {
                AddInfoRow("No object selected", "");
                return;
            }

            AddInfoRow("Type:", obj.ObjectType);
            AddInfoRow("Name:", obj.Name);

            if (obj is CustomLine line)
            {
                var startPoint = new Point(line.MainLine.X1, line.MainLine.Y1);
                var endPoint = new Point(line.MainLine.X2, line.MainLine.Y2);

                double length = CalculateLength(startPoint, endPoint);
                double angle = CalculateAngle(startPoint, endPoint);

                AddInfoRow("Length:", $"{length:F2} pixels");
                AddInfoRow("Angle:", $"{angle:F2}°");
                AddInfoRow("Start:", $"({line.MainLine.X1:F1}, {line.MainLine.Y1:F1})");
                AddInfoRow("End:", $"({line.MainLine.X2:F1}, {line.MainLine.Y2:F1})");
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

        public void Clear()
        {
            _infoGrid.Children.Clear();
            _infoGrid.RowDefinitions.Clear();
            AddInfoRow("No object selected", "");
        }
    }
}