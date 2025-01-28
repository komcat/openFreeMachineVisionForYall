using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using System.Windows;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;
using Size = OpenCvSharp.Size;
using Serilog;
using System.Runtime.InteropServices;
namespace OpenCVwpf.ImageClass
{


    public class OpenCVCornerDetector
    {
        private readonly CornerDetectionParameters _params;

        public OpenCVCornerDetector(CornerDetectionParameters parameters)
        {
            _params = parameters;
        }

        public List<Point> DetectCorners(BitmapSource image)
        {
            var points = new List<Point>();

            try
            {
                using var sourceMat = ConvertBitmapSourceToMat(image);
                if (sourceMat == null) return points;

                using var gray = new Mat();
                using var blurred = new Mat();
                using var corners = new Mat();

                // Convert to grayscale
                Cv2.CvtColor(sourceMat, gray, ColorConversionCodes.BGR2GRAY);

                // Apply Gaussian blur to reduce noise
                Cv2.GaussianBlur(gray, blurred, new Size(3, 3), 0);

                // Detect corners
                var cornersPoints =Cv2.GoodFeaturesToTrack(
                    blurred,
                    maxCorners: 50,
                    qualityLevel: _params.QualityLevel,
                    minDistance: _params.MinDistance,
                    mask: null,
                    blockSize: _params.BlockSize,
                    useHarrisDetector: true,
                    k: 0.04
                );

                // Refine corners to subpixel accuracy
                var criteria = new TermCriteria(CriteriaTypes.Eps | CriteriaTypes.MaxIter, 40, 0.001);
                Cv2.CornerSubPix(gray, cornersPoints, new OpenCvSharp.Size(5, 5), new Size(-1, -1), criteria);

                // Convert OpenCV points to WPF points
                Point2f[] cornerArray = cornersPoints.ToArray<Point2f>();
                foreach (var corner in cornerArray)
                {
                    points.Add(new Point(corner.X, corner.Y));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error detecting corners");
            }

            return points;
        }

        private static Mat ConvertBitmapSourceToMat(BitmapSource source)
        {
            if (source == null) return null;

            try
            {
                // Convert BitmapSource to byte array
                int stride = (source.PixelWidth * source.Format.BitsPerPixel + 7) / 8;
                byte[] data = new byte[stride * source.PixelHeight];
                source.CopyPixels(data, stride, 0);

                // Create Mat and copy data
                var mat = new Mat(source.PixelHeight, source.PixelWidth, MatType.CV_8UC4);
                Marshal.Copy(data, 0, mat.Data, data.Length);
                return mat;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error converting BitmapSource to Mat: {Message}", ex.Message);
                return null;
            }
        }
    }
}
