using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace SimpleRegReplace
{
    /// <summary>
    /// Логика взаимодействия для OpenPattern.xaml
    /// </summary>
    public partial class OpenPattern
    {
        class PatternPart
        {
            public string Name { get; set; }
            public string Value { get; set; }

            public PatternPart(string name, string val)
            {
                Name = name;
                Value = val;
            }
        }

        private readonly List<PatternPart> _patternParts = new List<PatternPart>();

        public Pattern Pattern;
        public string FileName { get; private set; }

        public OpenPattern()
        {
            InitializeComponent();

            
        }

        public OpenPattern(string fileName)
            : this()
        {
            OpenFile(fileName);
        }

        private Binding patternsBinding;

        void OpenFile(string fileName)
        {
            FileName = fileName;
            Pattern = Pattern.LoadFromFile(FileName);
            BindPatterns();

        }

//        private ObservableCollection<PatternItem> dataSource; 

        void BindPatterns()
        {
            dataGrid1.ItemsSource = null;
            dataGrid1.ItemsSource = Pattern.Item;// viewSource.View;            
        }

        public OpenPattern(bool openFile):this()
        {
            

            _patternParts.Add(new PatternPart("Именованная группа", "(?<name>.)"));
            cbInsertMacros.ItemsSource = _patternParts;

            if (openFile)
            {
                var ofd = new Microsoft.Win32.OpenFileDialog
                              {
                                  Filter = "Xml файлы|*.xml"
                              };
                if (ofd.ShowDialog() == true)
                {
                    OpenFile(ofd.FileName);
                }
                else
                {
                    Pattern = new Pattern();
                    BindPatterns();
                }
                //dataGrid1.ItemsSource = Pattern.Item;
            }
            else
            {
                Pattern = new Pattern();
                BindPatterns();
            }
        }

        private void Button1Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button2Click(object sender, RoutedEventArgs e)
        {
            if(String.IsNullOrEmpty(FileName))
            {
                var sfd = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Xml файлы|*.xml"
                };
                if (sfd.ShowDialog() == true)
                {
                    FileName = sfd.FileName;
                }
                else
                {
                    return;
                }
            }
            //Pattern = (Pattern)
            dataGrid1.Items.Refresh();
            Pattern.SaveToFile(FileName);
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            if(dataGrid1.SelectedValue == null)
            {
                MessageBox.Show("Выберите шаблон в списке");
                return;
            }
            var selectedItem = dataGrid1.SelectedValue as PatternItem;
            if(selectedItem == null)
            {
                MessageBox.Show("Выберите шаблон в списке");
                return;
            }

            if(selectedItem.SearchString == null)
            {
                MessageBox.Show("Введите шаблон поиска");
                return;
            } 
            if(selectedItem.ReplaceString == null)
            {
                MessageBox.Show("Введите строку замены");
                return;
            }
            var exp = new Regex(selectedItem.SearchString);

            var watchDogCounter = 0;
            var text = tbSource.Text;
            // Поиск по одну и тому же паттерну пока ищется
            do
            {
                if (exp.IsMatch(text))
                {
                    text = exp.Replace(text, selectedItem.ReplaceString);
                    watchDogCounter++;
                    continue;
                }
                break;
            } while (watchDogCounter < 1000);
            if (watchDogCounter >= 1000)
            {
                MessageBox.Show("Слишком много итераций. Возможно зацикливание!");
            }
            tbResult.Text = text;
        }

        private void cbInsertMacros_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        static void Swap<T>(IList<T> list, int index1, int index2)
        {
            var temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
        }

        private void BUpClick(object sender, RoutedEventArgs e)
        {
            var obj = ((FrameworkElement)sender).DataContext as PatternItem;
            if (obj == null)
                return;

            var ind = Pattern.Item.IndexOf(obj);
            if (ind == 0)
                return;

            Swap(Pattern.Item, ind, ind - 1);
            BindPatterns();
        }

        private void BDownClick(object sender, RoutedEventArgs e)
        {
            var obj = ((FrameworkElement)sender).DataContext as PatternItem;
            if (obj == null)
                return;

            var ind = Pattern.Item.IndexOf(obj);
            if (ind == Pattern.Item.Count - 1)
                return;

            Swap(Pattern.Item, ind, ind + 1);
            BindPatterns();
        }
    }
}
