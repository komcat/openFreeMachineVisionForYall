using System.Windows;
using Serilog;

namespace OpenCVwpf.ImageClass
{
    public class PointDetector
    {
        private double _threshold;
        private int _windowSize;


        public PointDetector(double threshold = 20, int windowSize = 5)
        {
            _threshold = threshold;
            _windowSize = windowSize;
        }
        public void UpdateParameters(double threshold, int windowSize)
        {
            _threshold = threshold;
            _windowSize = windowSize;
        }
        public List<(int Position, string Type, Point Location)> DetectPoints(double[] positions, double[] values, Point start, Point end)
        {
            var points = new List<(int Position, string Type, Point Location)>();
            if (values.Length < 2) return points;

            // Calculate direction vector
            double dx = end.X - start.X;
            double dy = end.Y - start.Y;
            double length = Math.Sqrt(dx * dx + dy * dy);
            if (length < 1e-6) return points;

            // Normalize direction
            dx /= length;
            dy /= length;

            // Calculate gradients
            var gradients = new List<(int index, double gradient, double position)>();
            for (int i = 1; i < values.Length; i++)
            {
                double gradient = Math.Abs(values[i] - values[i - 1]);
                if (gradient >= _threshold)
                {
                    gradients.Add((i, gradient, positions[i]));
                }
            }

            // Sort gradients by strength
            gradients.Sort((a, b) => b.gradient.CompareTo(a.gradient));

            // Filter out weaker points near stronger points
            var strongPoints = new List<(int index, double gradient, double position)>();
            foreach (var point in gradients)
            {
                bool isNearStrongerPoint = strongPoints.Any(sp =>
                    Math.Abs(point.index - sp.index) < _windowSize);

                if (!isNearStrongerPoint)
                {
                    strongPoints.Add(point);
                }
            }

            // Sort points by position along line
            strongPoints.Sort((a, b) => a.position.CompareTo(b.position));

            // Convert to screen coordinates and determine rise/fall
            foreach (var point in strongPoints)
            {
                double x = start.X + dx * point.position;
                double y = start.Y + dy * point.position;
                string type = (point.index < values.Length - 1 &&
                             values[point.index] < values[point.index + 1]) ? "Rise" : "Fall";

                points.Add((point.index, type, new Point(x, y)));
                Log.Debug($"{type} point detected at ({x:F1}, {y:F1}) with gradient {point.gradient:F1}");
            }

            return points;
        }
    }
}