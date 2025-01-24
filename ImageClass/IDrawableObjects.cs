using System.Windows.Controls;

namespace OpenCVwpf.ImageClass
{
    public interface IDrawableObject
    {
        string Name { get; set; }
        string ObjectType { get; }
        string Purpose { get; }
        bool IsSelected { get; }
        Dictionary<string, object> GetOutputData();
        void Select();
        void Deselect();
        void AddToCanvas();
        void RemoveFromCanvas();
        void UpdatePosition();
    }


}