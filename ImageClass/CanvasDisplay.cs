using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OpenCVwpf.ImageClass
{
    public class CanvasDisplay
    {
        private Canvas _canvas;
        private Image _canvasImage;

        public CanvasDisplay(Canvas canvas)
        {
            _canvas = canvas;
            _canvasImage = new Image();
            _canvasImage.Stretch = Stretch.Uniform;

            // Bind the Image size to Canvas size
            _canvasImage.Width = canvas.Width;
            _canvasImage.Height = canvas.Height;

            // Make image resize with canvas
            _canvas.SizeChanged += (s, e) =>
            {
                _canvasImage.Width = _canvas.ActualWidth;
                _canvasImage.Height = _canvas.ActualHeight;
            };

            Canvas.SetTop(_canvasImage, 0);
            Canvas.SetLeft(_canvasImage, 0);
            _canvas.Children.Add(_canvasImage);
        }

        // Method to display a bitmap image
        public void DisplayBitmap(BitmapSource bitmap)
        {
            if (_canvasImage != null)
            {
                _canvasImage.Source = bitmap;
                _canvasImage.Width = _canvas.ActualWidth;
                _canvasImage.Height = _canvas.ActualHeight;
            }
        }

        // Method to get the current bitmap
        public BitmapSource? GetCurrentBitmap()
        {
            return _canvasImage?.Source as BitmapSource;
        }

        // Method to clear the display
        public void Clear()
        {
            if (_canvasImage != null)
            {
                _canvasImage.Source = null;
            }
        }

        // Method to get canvas size
        public (double Width, double Height) GetCanvasSize()
        {
            return (_canvas.ActualWidth, _canvas.ActualHeight);
        }
    }
}