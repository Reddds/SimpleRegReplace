using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.Win32;

namespace XmlReplace.Converters.CustomConverters.Hig6GramInputPart
{
    /// <summary>
    /// Логика взаимодействия для InputPartConverterWindow.xaml
    /// </summary>
    public partial class InputPartConverterWindow
    {
        public InputPartConverterWindow()
        {
            InitializeComponent();
        }

        public bool UseGrops
        {
            get
            {
                return cbUseGroups.IsChecked == true;
            }
        }

        public string XmlPath { get; set; }

        private void ChooseXmlOnClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog {Filter = "*.xml|*.xml"};
            if (ofd.ShowDialog() != true)
            {
                DialogResult = false;
                return;
            }

            XmlPath = ofd.FileName;
            DialogResult = true;
        }
    }
}
