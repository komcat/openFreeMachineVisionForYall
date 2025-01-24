using System.Windows;
using System.Windows.Controls;

namespace OpenCVwpf.ImageClass
{
    public interface IParameterHandler
    {
        void UpdatePanel(StackPanel panel);
        event EventHandler<Dictionary<string, double>> ParametersChanged;
    }

    public class PointDetectionParameters : IParameterHandler
    {
        private double _threshold = 20;
        private double _windowSize = 5;
        public event EventHandler<Dictionary<string, double>> ParametersChanged;

        public void UpdatePanel(StackPanel panel)
        {
            panel.Children.Clear();

            // Title
            var titleLabel = new Label
            {
                Content = "Point Detection Parameters",
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 10)
            };
            panel.Children.Add(titleLabel);

            // Threshold Slider
            AddSlider(panel, "Threshold", 5, 50, _threshold, (sender, e) =>
            {
                var slider = sender as Slider;
                if (slider != null)
                {
                    _threshold = slider.Value;
                    NotifyParametersChanged();
                }
            });

            // Window Size Slider
            AddSlider(panel, "Window Size", 3, 15, _windowSize, (sender, e) =>
            {
                var slider = sender as Slider;
                if (slider != null)
                {
                    _windowSize = slider.Value;
                    NotifyParametersChanged();
                }
            });
        }

        private void AddSlider(StackPanel panel, string label, double min, double max, double value,
            RoutedPropertyChangedEventHandler<double> handler)
        {
            var labelText = new Label { Content = label };
            panel.Children.Add(labelText);

            var slider = new Slider
            {
                Minimum = min,
                Maximum = max,
                Value = value,
                IsSnapToTickEnabled = true,
                TickFrequency = 1,
                Margin = new Thickness(10, 0, 10, 10)
            };
            slider.ValueChanged += handler;
            panel.Children.Add(slider);

            var valueText = new TextBlock
            {
                Text = value.ToString("F1"),
                Margin = new Thickness(0, 0, 0, 10)
            };
            slider.ValueChanged += (s, e) => valueText.Text = e.NewValue.ToString("F1");
            panel.Children.Add(valueText);
        }

        private void NotifyParametersChanged()
        {
            var parameters = new Dictionary<string, double>
            {
                { "threshold", _threshold },
                { "windowSize", _windowSize }
            };
            ParametersChanged?.Invoke(this, parameters);
        }
    }

    public class DetectionParameterManager
    {
        private readonly Dictionary<string, IParameterHandler> _handlers;
        private readonly StackPanel _panel;

        public DetectionParameterManager(StackPanel panel)
        {
            _panel = panel;
            _handlers = new Dictionary<string, IParameterHandler>
            {
                { "PointDetection", new PointDetectionParameters() }
            };
        }

        public void UpdateParameters(IDrawableObject obj)
        {
            _panel.Children.Clear();
            if (obj != null && _handlers.ContainsKey(obj.Purpose))
            {
                _handlers[obj.Purpose].UpdatePanel(_panel);
            }
        }

        public void SubscribeToParameters(string purpose, EventHandler<Dictionary<string, double>> handler)
        {
            if (_handlers.ContainsKey(purpose))
            {
                _handlers[purpose].ParametersChanged += handler;
            }
        }

        public void UnsubscribeFromParameters(string purpose, EventHandler<Dictionary<string, double>> handler)
        {
            if (_handlers.ContainsKey(purpose))
            {
                _handlers[purpose].ParametersChanged -= handler;
            }
        }
    }
}