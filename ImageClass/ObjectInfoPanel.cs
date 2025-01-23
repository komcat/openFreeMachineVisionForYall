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
            _infoGrid.Children.Clear();
            _infoGrid.RowDefinitions.Clear();

            if (obj == null)
            {
                AddInfoRow("No object selected", "");
                return;
            }

            // Display common properties
            AddInfoRow("Type:", obj.ObjectType);
            AddInfoRow("Name:", obj.Name);

            // Get specific properties using handler
            if (_propertyHandlers.TryGetValue(obj.ObjectType, out var handler))
            {
                handler.UpdateProperties(obj);
                foreach (var (label, value) in handler.GetProperties())
                {
                    AddInfoRow($"{label}:", value);
                }
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
            _infoGrid.Children.Clear();
            _infoGrid.RowDefinitions.Clear();
            AddInfoRow("No object selected", "");
        }
    }
}