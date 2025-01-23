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
        private ObjectManager _objectManager;
        private PixelValueAnalyzer _pixelAnalyzer;
        private bool _isLineDrawingMode = false;
        private ObjectInfoPanel _objectInfoPanel;
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

            // Load initial image
            LoadSampleImage();

            // Wire up button events
            InitializeToolButtons();

            // Subscribe to ObjectManager events for pixel analysis
            _objectManager.ObjectSelected += ObjectManager_ObjectSelected;
            _objectManager.ObjectDeleted += ObjectManager_ObjectDeleted;

            _objectInfoPanel = new ObjectInfoPanel(ObjectInfoStackPanel);

            
            // Ensure the canvas can receive keyboard focus
            DisplayCanvas.Focusable = true;
            DisplayCanvas.Focus();
        }

        private void ObjectManager_ObjectSelected(object sender, IDrawableObject obj)
        {
            if(obj == null)
            {
                
                _objectInfoPanel.UpdateInfo(null);  // Clear panel
                _pixelAnalyzer.DetachFromCurrentLine();
                return;
            }
            Log.Information("Object selected: {Type}", obj.GetType().Name);

            // Update info panel
            _objectInfoPanel.UpdateInfo(obj);

            if (obj is CustomLine line)
            {
                var currentBitmap = _canvasDisplay.GetCurrentBitmap();
                if (currentBitmap != null)
                {
                    Log.Information("Attaching pixel analyzer to line: {LineName}", line.Name);
                    _pixelAnalyzer.AttachToLine(line, currentBitmap);
                }
                else
                {
                    Log.Warning("No bitmap available for analysis");
                }
            }
            else
            {
                _pixelAnalyzer.DetachFromCurrentLine();
            }
        }
        private void ObjectManager_ObjectDeleted(object sender, IDrawableObject obj)
        {
            if (obj is CustomLine)
            {
                _pixelAnalyzer.DetachFromCurrentLine();
            }
        }

        private void InitializeToolButtons()
        {
            // Line button setup
            LineButton.Click += LineButton_Click;
        }

        private void LineButton_Click(object sender, RoutedEventArgs e)
        {
            _isLineDrawingMode = !_isLineDrawingMode;

            if (_isLineDrawingMode)
            {
                Log.Information("Line drawing mode activated");
                _lineDrawer.StartDrawingMode();
                LineButton.Background = Brushes.LightBlue;
                DisplayCanvas.Cursor = Cursors.Cross;
            }
            else
            {
                Log.Information("Line drawing mode deactivated");
                _lineDrawer.StopDrawingMode();
                LineButton.Background = SystemColors.ControlBrush;
                DisplayCanvas.Cursor = Cursors.Arrow;
            }

            // Ensure canvas keeps focus after button click
            DisplayCanvas.Focus();
        }

        private void LoadSampleImage()
        {
            // Load an image from the application directory
            _imageHandler.LoadImageFromAppPath("sample.png");
        }
    }
}