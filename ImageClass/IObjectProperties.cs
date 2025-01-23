namespace OpenCVwpf.ImageClass
{
    public interface IObjectProperties
    {
        string GetPropertyValue(string propertyName);
        List<(string Label, string Value)> GetAllProperties();
    }

    public interface IPropertyHandler
    {
        void UpdateProperties(IDrawableObject obj);
        List<(string Label, string Value)> GetProperties();
    }
}