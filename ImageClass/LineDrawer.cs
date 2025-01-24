using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Serilog;
using Serilog.Context;

namespace OpenCVwpf.ImageClass
{
    public class LineDrawer
    {
        private readonly Canvas _canvas;
        private readonly ObjectManager _objectManager;
        private readonly Window _owner;
        private Point _startPoint;
        private Line _previewLine;
        private bool _isDrawingMode;
        private readonly ILogger _logger;

        public LineDrawer(Canvas canvas, ObjectManager objectManager, Window owner)
        {
            // Create a contextualized logger for this instance
            _logger = Log.ForContext<LineDrawer>();
            _logger.Debug("Initializing LineDrawer");

            _canvas = canvas;
            _objectManager = objectManager;
            _owner = owner;
            _isDrawingMode = false;
            _logger.Information("LineDrawer initialized with canvas and object manager");
        }

        public void StartDrawingMode()
        {
            try
            {
                _isDrawingMode = true;
                _canvas.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
                _canvas.MouseMove += Canvas_MouseMove;
                _canvas.MouseLeftButtonUp += Canvas_MouseLeftButtonUp;
                _canvas.Focus();
                _logger.Information("Line drawing mode activated");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to start drawing mode");
                throw;
            }
        }

        public void StopDrawingMode()
        {
            try
            {
                _isDrawingMode = false;
                _canvas.MouseLeftButtonDown -= Canvas_MouseLeftButtonDown;
                _canvas.MouseMove -= Canvas_MouseMove;
                _canvas.MouseLeftButtonUp -= Canvas_MouseLeftButtonUp;
                _logger.Information("Line drawing mode deactivated");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to stop drawing mode");
                throw;
            }
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isDrawingMode && (e.Source is Canvas || e.Source is Image))
            {
                try
                {
                    _startPoint = e.GetPosition(_canvas);
                    using (LogContext.PushProperty("Operation", "DrawStart"))
                    {
                        _logger.Debug("Starting line draw at position: ({X}, {Y})", _startPoint.X, _startPoint.Y);
                    }

                    _previewLine = new Line
                    {
                        Stroke = Brushes.Cyan,
                        StrokeThickness = 1,
                        X1 = _startPoint.X,
                        Y1 = _startPoint.Y,
                        X2 = _startPoint.X,
                        Y2 = _startPoint.Y
                    };

                    _canvas.Children.Add(_previewLine);
                    e.Handled = true;
                    _logger.Debug("Preview line created and added to canvas");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error during mouse down event in line drawing");
                }
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawingMode && _previewLine != null)
            {
                try
                {
                    Point currentPoint = e.GetPosition(_canvas);
                    using (LogContext.PushProperty("Operation", "DrawUpdate"))
                    {
                        //_logger.Debug("Preview line updated to end position: ({X}, {Y})", currentPoint.X, currentPoint.Y);
                    }
                    _previewLine.X2 = currentPoint.X;
                    _previewLine.Y2 = currentPoint.Y;
                    e.Handled = true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error updating preview line during mouse move");
                }
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDrawingMode && _previewLine != null)
            {
                try
                {
                    Point endPoint = e.GetPosition(_canvas);
                    using (LogContext.PushProperty("Operation", "DrawComplete"))
                    {
                        _logger.Debug("Finishing line draw at position: ({X}, {Y})", endPoint.X, endPoint.Y);

                        // Calculate line length for logging
                        double length = Math.Sqrt(
                            Math.Pow(endPoint.X - _startPoint.X, 2) +
                            Math.Pow(endPoint.Y - _startPoint.Y, 2));
                        _logger.Information("Line completed with length: {Length:F2} pixels", length);
                    }

                    // Remove preview line
                    _canvas.Children.Remove(_previewLine);
                    _logger.Debug("Preview line removed from canvas");

                    // Create actual custom line
                    var customLine = new CustomLine(_canvas, _startPoint, endPoint, _objectManager);
                    //customLine.SetHitAreaEnabled(false);  // Add this line
                    customLine.AddToCanvas();

                    // Add to object manager with name input
                    _objectManager.AddObject(customLine, _owner);

                    // Select the new line
                    _objectManager.SelectObject(customLine);

                    _previewLine = null;
                    e.Handled = true;

                    using (LogContext.PushProperty("LineId", customLine.Name))
                    {
                        _logger.Information("New line created and added to canvas: {LineName}", customLine.Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error finalizing line creation");
                    _previewLine = null;
                }
            }
        }
    }
}