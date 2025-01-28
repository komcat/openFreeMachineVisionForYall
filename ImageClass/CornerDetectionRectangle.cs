using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace OpenCVwpf.ImageClass
{
    public class CornerDetectionRectangle : CustomRectangle
    {
        private readonly PointMarker _cornerMarker;
        private readonly OpenCVCornerDetector _cornerDetector;

        public CornerDetectionRectangle(Canvas canvas, Point topLeft, double width, double height, ObjectManager objectManager)
            : base(canvas, topLeft, width, height, objectManager)
        {
            _cornerMarker = new PointMarker(canvas);
            _cornerDetector = new OpenCVCornerDetector(new CornerDetectionParameters());

            RectanglePositionChanged += CornerDetectionRectangle_PositionChanged;
        }

        public override string Purpose => "CornerDetection";

        private void CornerDetectionRectangle_PositionChanged(object sender, EventArgs e)
        {
            UpdateCorners();
        }

        private void UpdateCorners()
        {
            var corners = _cornerDetector.DetectCorners(GetRectangleImage());
            _cornerMarker.UpdatePoints(corners.Select(p => (0, "Corner", p)).ToList());
        }

        private BitmapSource GetRectangleImage()
        {
            var canvas = MainRectangle.Parent as Canvas;
            if (canvas == null) return null;

            var backgroundImage = canvas.Children.OfType<Image>().FirstOrDefault();
            if (backgroundImage?.Source is not BitmapSource source) return null;

            double left = Canvas.GetLeft(MainRectangle);
            double top = Canvas.GetTop(MainRectangle);

            int x = Math.Max(0, (int)left);
            int y = Math.Max(0, (int)top);
            int width = Math.Min((int)MainRectangle.Width, source.PixelWidth - x);
            int height = Math.Min((int)MainRectangle.Height, source.PixelHeight - y);

            var croppedBitmap = new CroppedBitmap(source, new Int32Rect(x, y, width, height));
            return croppedBitmap;
        }

        public override Dictionary<string, object> GetOutputData()
        {
            var data = base.GetOutputData();
            var corners = _cornerDetector.DetectCorners(GetRectangleImage());
            data["DetectedCorners"] = corners;
            return data;
        }

        public override void RemoveFromCanvas()
        {
            base.RemoveFromCanvas();
            _cornerMarker.ClearMarkers();
        }
    }
}
