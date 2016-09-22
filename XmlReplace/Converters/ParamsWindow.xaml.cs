using System.Windows;

namespace XmlReplace
{
    /// <summary>
    /// Логика взаимодействия для ParamsWindow.xaml
    /// </summary>
    public partial class ParamsWindow
    {
        public ParamsWindow()
        {
            InitializeComponent();
        }

        private void BOkClick(object sender, RoutedEventArgs e)
        {
            //var bex =GetBindingExpression(DataContextProperty);
            //bex.UpdateTarget();
            DialogResult = true;
        }

        private void BCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
