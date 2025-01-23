using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ScottPlot;
using ScottPlot.WPF;
using Colors = ScottPlot.Colors;

namespace OpenCVwpf.ImageClass
{
    public class PixelValueAnalyzer
    {
        private readonly WpfPlot _plot;
        private CustomLine? _currentLine;
        private BitmapSource? _currentImage;

        public PixelValueAnalyzer(WpfPlot plot)
        {
            _plot = plot;
            InitializePlot();
        }

        private void InitializePlot()
        {
            _plot.Plot.Title("Pixel Values Along Line");
            _plot.Plot.XLabel("Position");
            _plot.Plot.YLabel("Pixel Intensity");
            _plot.Plot.Axes.SetLimitsY(0, 255);
            _plot.Plot.Axes.SetLimitsX(0, 100);
            _plot.Plot.Grid.IsVisible = true;
            _plot.Refresh();
        }

        private (double[] positions, double[] values) SamplePixelsAlongLine(System.Windows.Shapes.Line line, BitmapSource image)
        {
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
                values[i] = index < pixels.Length ? pixels[index] : 0;
                positions[i] = i;
            }

            return (positions, values);
        }

        public void AttachToLine(CustomLine line, BitmapSource image)
        {
            _currentLine = line;
            _currentImage = image;
            _currentLine.LinePositionChanged += Line_PositionChanged;
            UpdatePlot();
        }

        private void Line_PositionChanged(object? sender, EventArgs e)
        {
            UpdatePlot();
        }

        private void UpdatePlot()
        {
            if (_currentLine == null || _currentImage == null) return;

            var (positions, values) = SamplePixelsAlongLine(_currentLine.MainLine, _currentImage);

            _plot.Plot.Clear();
            _plot.Plot.Add.ScatterLine(positions, values);
            _plot.Plot.Axes.SetLimitsX(positions.Min(), positions.Max());
            _plot.Plot.Axes.SetLimitsY(0, 255);
            _plot.Refresh();
        }

        public void DetachFromCurrentLine()
        {
            if (_currentLine != null)
            {
                _currentLine.LinePositionChanged -= Line_PositionChanged;
                _currentLine = null;
            }
            _currentImage = null;
            ClearPlot();
        }

        public void ClearPlot()
        {
            _plot.Plot.Clear();
            _plot.Refresh();
        }
    }
}