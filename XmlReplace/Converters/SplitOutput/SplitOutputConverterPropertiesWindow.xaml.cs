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

namespace XmlReplace
{
    /// <summary>
    /// Логика взаимодействия для SplitOutputConverterPropertiesWindow.xaml
    /// </summary>
    public partial class SplitOutputConverterPropertiesWindow : Window
    {
        public SplitOutputConverterPropertiesWindow()
        {
            InitializeComponent();
        }

        private void BCopyOnClick(object sender, RoutedEventArgs e)
        {
            var btn = (Button) sender;
            var ch = ((Grid) btn.Parent).Children;
            foreach (var child in ch)
            {
                if (!(child is TextBox))
                    continue;

                var tb = (TextBox) child;

                Clipboard.SetText(tb.Text);
                break;
            }
        }
    }
}
