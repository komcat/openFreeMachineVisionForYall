using System.Windows.Controls;

namespace OpenCVwpf.ImageClass
{
    public interface IDrawableObject
    {
        string Name { get; set; }
        string ObjectType { get; }
        bool IsSelected { get; }
        void Select();
        void Deselect();
        void AddToCanvas();
        void RemoveFromCanvas();
        void UpdatePosition();
    }
}