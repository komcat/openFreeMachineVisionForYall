using Serilog;
using Microsoft.Win32;
using System;
using System.Windows.Media.Imaging;
using System.IO;

namespace OpenCVwpf.ImageClass
{
    public class ImageHandler
    {
        private CanvasDisplay _canvasDisplay;

        public ImageHandler(CanvasDisplay canvasDisplay)
        {
            _canvasDisplay = canvasDisplay;
        }

        public BitmapSource? LoadImageFromFile()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp|All files (*.*)|*.*",
                    Title = "Select an image file"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;
                    Log.Information($"Loading image from: {filePath}");

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(filePath);
                    bitmap.EndInit();

                    _canvasDisplay.DisplayBitmap(bitmap);
                    Log.Information("Image loaded successfully");
                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading image");
            }
            return null;
        }

        public void RemoveImage()
        {
            try
            {
                _canvasDisplay.Clear();
                Log.Information("Image removed from canvas");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error removing image");
            }
        }

        public BitmapSource? LoadImageFromPath(string imagePath)
        {
            try
            {
                Log.Information($"Loading image from path: {imagePath}");
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(imagePath);
                bitmap.EndInit();

                _canvasDisplay.DisplayBitmap(bitmap);
                Log.Information("Image loaded successfully");
                return bitmap;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading image from path");
                return null;
            }
        }

        public BitmapSource? LoadImageFromAppPath(string relativeImagePath)
        {
            try
            {
                // Get the application's base directory
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string fullPath = Path.Combine(baseDir, relativeImagePath);

                Log.Information($"Loading image from app path: {fullPath}");

                if (!File.Exists(fullPath))
                {
                    Log.Warning($"Image file not found at: {fullPath}");
                    return null;
                }

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                bitmap.EndInit();

                _canvasDisplay.DisplayBitmap(bitmap);
                Log.Information("Image loaded successfully from app path");
                return bitmap;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading image from app path");
                return null;
            }
        }
    }
}