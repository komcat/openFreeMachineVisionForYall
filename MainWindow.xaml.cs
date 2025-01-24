using OpenCVwpf.ImageClass;
using Serilog;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OpenCVwpf
{
    public partial class MainWindow : Window
    {
        private CanvasDisplay _canvasDisplay;
        private ImageHandler _imageHandler;
        private LineDrawer _lineDrawer;
        private RectangleDrawer _rectangleDrawer;
        private ObjectManager _objectManager;
        private PixelValueAnalyzer _pixelAnalyzer;
        private bool _isLineDrawingMode = false;
        private bool _isRectangleDrawingMode = false;
        private ObjectInfoPanel _objectInfoPanel;

        private DetectionParameterManager _parameterManager;

        public MainWindow()
        {
            InitializeComponent();
            Log.Information("MainWindow initialized");

            // Initialize main components
            _canvasDisplay = new CanvasDisplay(DisplayCanvas);
            _objectManager = new ObjectManager(DisplayCanvas);
            _imageHandler = new ImageHandler(_canvasDisplay);
            _lineDrawer = new LineDrawer(DisplayCanvas, _objectManager, this);
            _pixelAnalyzer = new PixelValueAnalyzer(PixelPlot);
            _rectangleDrawer = new RectangleDrawer(DisplayCanvas, _objectManager, this);
            _objectInfoPanel = new ObjectInfoPanel(ObjectInfoStackPanel);
            // Add after other initializations
            _parameterManager = new DetectionParameterManager(ObjectDectionParameter);



            // Load initial image
            LoadSampleImage();

            // Wire up button events
            InitializeToolButtons();

            // Subscribe to ObjectManager events for pixel analysis
            _objectManager.ObjectSelected += ObjectManager_ObjectSelected;
            _objectManager.ObjectDeleted += ObjectManager_ObjectDeleted;

            

            
            // Ensure the canvas can receive keyboard focus
            DisplayCanvas.Focusable = true;
            DisplayCanvas.Focus();
        }

        private void ObjectManager_ObjectSelected(object sender, IDrawableObject obj)
        {
            if (obj == null)
            {
                _objectInfoPanel.UpdateInfo(null);
                _pixelAnalyzer.DetachFromCurrentLine();
                _parameterManager.UpdateParameters(null);
                return;
            }

            Log.Information("Object selected: {Type}", obj.GetType().Name);

            // Update info panel
            _objectInfoPanel.UpdateInfo(obj);
            _parameterManager.UpdateParameters(obj);

            if (obj is CustomLine line)
            {
                var currentBitmap = _canvasDisplay.GetCurrentBitmap();
                if (currentBitmap != null)
                {
                    _parameterManager.SubscribeToParameters("PointDetection",
                        (s, parameters) => line.UpdateDetectionParameters(parameters));
                    Log.Information("Attaching pixel analyzer to line: {LineName}", line.Name);
                    _pixelAnalyzer.AttachToLine(line, currentBitmap);
                }
            }
        }

        private void ObjectManager_ObjectDeleted(object sender, IDrawableObject obj)
        {
            if (obj is CustomLine line)
            {
                _parameterManager.UnsubscribeFromParameters("PointDetection",
                    (s, parameters) => line.UpdateDetectionParameters(parameters));
                _pixelAnalyzer.DetachFromCurrentLine();
            }
        }
        private void InitializeToolButtons()
        {
            // Line button setup
            LineButton.Click += LineButton_Click;
            RectangleButton.Click += RectangleButton_Click;
            CircleButton.Click += CircleButton_Click;
        }

        private void LineButton_Click(object sender, RoutedEventArgs e)
        {
            SetDrawingMode(_currentDrawingMode == DrawingMode.Line ? DrawingMode.None : DrawingMode.Line);
            DisplayCanvas.Focus();
        }

        private void RectangleButton_Click(object sender, RoutedEventArgs e)
        {
            SetDrawingMode(_currentDrawingMode == DrawingMode.Rectangle ? DrawingMode.None : DrawingMode.Rectangle);
            DisplayCanvas.Focus();
        }

        private void CircleButton_Click(object sender, RoutedEventArgs e)
        {
            SetDrawingMode(_currentDrawingMode == DrawingMode.Circle ? DrawingMode.None : DrawingMode.Circle);
            DisplayCanvas.Focus();
        }

        private void LoadSampleImage()
        {
            // Load an image from the application directory
            _imageHandler.LoadImageFromAppPath("sample.png");
        }

        public enum DrawingMode
        {
            None,
            Line,
            Rectangle,
            Circle
        }

        // Add as class field
        private DrawingMode _currentDrawingMode = DrawingMode.None;

        private void SetDrawingMode(DrawingMode newMode)
        {
            // Disable current mode
            switch (_currentDrawingMode)
            {
                case DrawingMode.Line:
                    _lineDrawer.StopDrawingMode();
                    LineButton.Background = SystemColors.ControlBrush;
                    break;
                case DrawingMode.Rectangle:
                    _rectangleDrawer.StopDrawingMode();
                    RectangleButton.Background = SystemColors.ControlBrush;
                    break;
                case DrawingMode.Circle:
                    // CircleDrawer.StopDrawingMode(); // To be implemented
                    CircleButton.Background = SystemColors.ControlBrush;
                    break;
            }

            // Enable new mode
            _currentDrawingMode = newMode;
            DisplayCanvas.Cursor = newMode == DrawingMode.None ? Cursors.Arrow : Cursors.Cross;

            switch (newMode)
            {
                case DrawingMode.Line:
                    _lineDrawer.StartDrawingMode();
                    LineButton.Background = Brushes.LightBlue;
                    break;
                case DrawingMode.Rectangle:
                    _rectangleDrawer.StartDrawingMode();
                    RectangleButton.Background = Brushes.LightBlue;
                    break;
                case DrawingMode.Circle:
                    // CircleDrawer.StartDrawingMode(); // To be implemented
                    CircleButton.Background = Brushes.LightBlue;
                    break;
            }
        }
    }
}