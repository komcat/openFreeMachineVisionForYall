using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OpenCVwpf.ImageClass
{
    public interface IRectangleFactory
    {
        IDrawableObject CreateRectangle(Canvas canvas, Point topLeft, double width, double height, ObjectManager objectManager);
    }

    public class RectangleFactories
    {
        private static readonly Dictionary<string, IRectangleFactory> _factories = new()
        {
            ["AreaMeasurement"] = new AreaRectangleFactory(),
            ["CornerDetection"] = new CornerDetectionRectangleFactory(),
            //["LineDetection"] = new LineDetectionRectangleFactory(),
            //["PatternTeach"] = new PatternTeachRectangleFactory(),
            //["PatternFind"] = new PatternFindRectangleFactory()
        };

        public static IRectangleFactory GetFactory(string purpose) =>
            _factories.TryGetValue(purpose, out var factory) ? factory : _factories["AreaMeasurement"];
    }

    // Example factory implementations
    public class AreaRectangleFactory : IRectangleFactory
    {
        public IDrawableObject CreateRectangle(Canvas canvas, Point topLeft, double width, double height, ObjectManager objectManager) =>
            new CustomRectangle(canvas, topLeft, width, height, objectManager);
    }

    public class CornerDetectionRectangleFactory : IRectangleFactory
    {
        public IDrawableObject CreateRectangle(Canvas canvas, Point topLeft, double width, double height, ObjectManager objectManager) =>
            new CornerDetectionRectangle(canvas, topLeft, width, height, objectManager);
    }
}
