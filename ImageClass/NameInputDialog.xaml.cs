using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OpenCVwpf.ImageClass
{
    public partial class NameInputDialog : Window
    {
        public string ObjectName { get; private set; }

        public NameInputDialog(string defaultName)
        {
            InitializeComponent();
            NameTextBox.Text = defaultName;
            NameTextBox.SelectAll();
            NameTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                ObjectName = NameTextBox.Text;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please enter a name.", "Invalid Name", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
