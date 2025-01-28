using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace OpenCVwpf.ImageClass
{
    public class CornerDetector
    {
        private readonly List<Point> _detectedCorners = new();
        private readonly int _blockSize = 3;
        private readonly double _qualityLevel = 0.01;
        private readonly double _minDistance = 10;
        private readonly Dictionary<Point, double> _cornerStrengths = new();

        public List<Point> DetectCorners(BitmapSource image)
        {
            _detectedCorners.Clear();
            _cornerStrengths.Clear();
            // Convert BitmapSource to grayscale byte array
            byte[] pixels = ConvertToGrayscale(image);
            int width = image.PixelWidth;
            int height = image.PixelHeight;

            // Harris corner response matrix
            double[,] harrisResponse = CalculateHarrisResponse(pixels, width, height);

            // Non-maximum suppression
            List<Point> corners = FindLocalMaxima(harrisResponse, width, height);

            // Sort corners by response strength
            corners.Sort((a, b) =>
                harrisResponse[(int)b.Y, (int)b.X].CompareTo(harrisResponse[(int)a.Y, (int)a.X]));

            // Store corner strengths before filtering
            foreach (var corner in corners)
            {
                _cornerStrengths[corner] = harrisResponse[(int)corner.Y, (int)corner.X];
            }

            // Filter corners by minimum distance
            _detectedCorners.AddRange(FilterByMinDistance(corners));

            return _detectedCorners;
        }

        private double[,] CalculateHarrisResponse(byte[] pixels, int width, int height)
        {
            double[,] response = new double[height, width];
            double[,] Ix = new double[height, width];
            double[,] Iy = new double[height, width];

            // Calculate gradients
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int index = y * width + x;
                    Ix[y, x] = pixels[index + 1] - pixels[index - 1];
                    Iy[y, x] = pixels[index + width] - pixels[index - width];
                }
            }

            // Calculate Harris response
            for (int y = _blockSize; y < height - _blockSize; y++)
            {
                for (int x = _blockSize; x < width - _blockSize; x++)
                {
                    double sumIx2 = 0, sumIy2 = 0, sumIxIy = 0;

                    // Sum over block
                    for (int dy = -_blockSize; dy <= _blockSize; dy++)
                    {
                        for (int dx = -_blockSize; dx <= _blockSize; dx++)
                        {
                            double ix = Ix[y + dy, x + dx];
                            double iy = Iy[y + dy, x + dx];
                            sumIx2 += ix * ix;
                            sumIy2 += iy * iy;
                            sumIxIy += ix * iy;
                        }
                    }

                    // Harris corner response
                    double det = sumIx2 * sumIy2 - sumIxIy * sumIxIy;
                    double trace = sumIx2 + sumIy2;
                    response[y, x] = det - 0.04 * trace * trace;
                }
            }

            return response;
        }

        private List<Point> FindLocalMaxima(double[,] response, int width, int height)
        {
            var maxima = new List<Point>();
            double threshold = GetResponseThreshold(response);

            for (int y = _blockSize; y < height - _blockSize; y++)
            {
                for (int x = _blockSize; x < width - _blockSize; x++)
                {
                    if (response[y, x] > threshold && IsLocalMaximum(response, x, y))
                    {
                        maxima.Add(new Point(x, y));
                    }
                }
            }

            return maxima;
        }

        private double GetResponseThreshold(double[,] response)
        {
            double maxResponse = 0;
            foreach (double r in response)
            {
                maxResponse = Math.Max(maxResponse, r);
            }
            return maxResponse * _qualityLevel;
        }

        private bool IsLocalMaximum(double[,] response, int x, int y)
        {
            double value = response[y, x];
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    if (response[y + dy, x + dx] >= value) return false;
                }
            }
            return true;
        }

        private List<Point> FilterByMinDistance(List<Point> corners)
        {
            var filtered = new List<Point>();
            bool[] used = new bool[corners.Count];

            for (int i = 0; i < corners.Count; i++)
            {
                if (used[i]) continue;

                filtered.Add(corners[i]);
                used[i] = true;

                // Suppress corners within minDistance
                for (int j = i + 1; j < corners.Count; j++)
                {
                    if (used[j]) continue;
                    double dx = corners[i].X - corners[j].X;
                    double dy = corners[i].Y - corners[j].Y;
                    if (dx * dx + dy * dy < _minDistance * _minDistance)
                    {
                        used[j] = true;
                    }
                }
            }

            return filtered;
        }

        private byte[] ConvertToGrayscale(BitmapSource image)
        {
            int stride = (image.PixelWidth * image.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[image.PixelHeight * stride];
            image.CopyPixels(pixels, stride, 0);
            return pixels;
        }

        // Add method to get corner strengths
        public Dictionary<Point, double> GetCornerStrengths() => _cornerStrengths;
    }
}
