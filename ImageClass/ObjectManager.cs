using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Serilog;

namespace OpenCVwpf.ImageClass
{
    public class ObjectManager
    {
        private readonly Canvas _canvas;
        private readonly Dictionary<string, Dictionary<string, int>> _objectCounters;
        private readonly List<IDrawableObject> _objects;
        private IDrawableObject _selectedObject;

        public event EventHandler<IDrawableObject> ObjectSelected;
        public event EventHandler<IDrawableObject> ObjectDeleted;

        public ObjectManager(Canvas canvas)
        {
            _canvas = canvas;
            _objects = new List<IDrawableObject>();
            _objectCounters = new Dictionary<string, Dictionary<string, int>>();

            _canvas.MouseLeftButtonDown += Canvas_Background_Click;
            _canvas.KeyDown += Canvas_KeyDown;
            _canvas.Focusable = true;
        }
        public string GetNextDefaultName(string objectType)
        {
            if (!_objectCounters.ContainsKey(objectType))
            {
                _objectCounters[objectType] = new Dictionary<string, int>();
            }

            var typeCounters = _objectCounters[objectType];
            int counter = 1;

            while (typeCounters.ContainsValue(counter) ||
                   _objects.Any(obj => obj.Name == $"{objectType}{counter}"))
            {
                counter++;
            }

            typeCounters[objectType] = counter;
            return $"{objectType}{counter}";
        }

        public bool IsNameUnique(string name)
        {
            return !_objects.Any(obj => obj.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public string ShowNameInputDialog(IDrawableObject obj, Window owner)
        {
            string defaultName = GetNextDefaultName(obj.ObjectType);

            var dialog = new NameInputDialog(defaultName)
            {
                Owner = owner
            };

            if (dialog.ShowDialog() == true)
            {
                string newName = dialog.ObjectName;

                // Check if name is unique
                if (!IsNameUnique(newName))
                {
                    MessageBox.Show("This name is already in use. Using default name instead.",
                                  "Duplicate Name",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                    return defaultName;
                }

                return newName;
            }

            return defaultName;
        }

        public void AddObject(IDrawableObject drawableObject, Window owner)
        {
            string name = ShowNameInputDialog(drawableObject, owner);
            drawableObject.Name = name;
            _objects.Add(drawableObject);
            Log.Information($"Added new object '{name}'. Total objects: {_objects.Count}");
        }
        private void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && _selectedObject != null)
            {
                DeleteSelectedObject();
                e.Handled = true;
            }
        }

        private void Canvas_Background_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is Canvas || e.Source is Image)
            {
                DeselectCurrentObject();
                _canvas.Focus();
            }
        }

        public void AddObject(IDrawableObject drawableObject)
        {
            _objects.Add(drawableObject);
            Log.Information($"Added new object. Total objects: {_objects.Count}");
        }

        public void RemoveObject(IDrawableObject drawableObject)
        {
            if (_objects.Contains(drawableObject))
            {
                drawableObject.RemoveFromCanvas();
                _objects.Remove(drawableObject);
                Log.Information($"Removed object. Remaining objects: {_objects.Count}");
            }
        }

        public void SelectObject(IDrawableObject drawableObject)
        {
            DeselectCurrentObject();

            _selectedObject = drawableObject;
            _selectedObject.Select();
            ObjectSelected?.Invoke(this, drawableObject);
            Log.Information("New object selected");

            _canvas.Focus();
        }

        public void DeselectCurrentObject()
        {
            if (_selectedObject != null)
            {
                _selectedObject.Deselect();
                _selectedObject = null;
                Log.Information("Object deselected");
                ObjectSelected?.Invoke(this, null); // Trigger event with null to clear plot
            }
        }

        public void DeleteSelectedObject()
        {
            if (_selectedObject != null)
            {
                var objectToDelete = _selectedObject;
                DeselectCurrentObject();
                RemoveObject(objectToDelete);
                ObjectDeleted?.Invoke(this, objectToDelete);
                Log.Information("Selected object deleted");
            }
        }

        public void ClearAllObjects()
        {
            DeselectCurrentObject();
            foreach (var obj in _objects.ToList())
            {
                RemoveObject(obj);
            }
            Log.Information("All objects cleared");
        }

        public IDrawableObject GetSelectedObject()
        {
            return _selectedObject;
        }

        public List<IDrawableObject> GetAllObjects()
        {
            return new List<IDrawableObject>(_objects);
        }
    }
}