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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace XmlReplace
{
    /// <summary>
    /// Логика взаимодействия для WorkItem.xaml
    /// </summary>
    public partial class WorkItem
    {
        private IsConverter _converter;
        public WorkItem(IsConverter converter)
        {
            _converter = converter;
            InitializeComponent();

            TbName.Text = _converter.Name;
            TbDescription.Text = _converter.Description;

            var type = converter.GetType();
            SetImage(type.Name);
        }

        public IsConverter Converter
        {
            get
            {
                return _converter;
            }
        }

        private void SetImage(string converterName)
        {
            var bi3 = new BitmapImage();
            bi3.BeginInit();//pack://application:,,,
            bi3.UriSource = new Uri("/XmlReplace;component/Resources/Images/" + converterName + ".png", UriKind.Relative);
            bi3.EndInit();

            MainIcon.Source = bi3;
        }
    }
}
