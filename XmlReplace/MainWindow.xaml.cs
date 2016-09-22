using System;
//using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml.Serialization;
using Microsoft.Win32;
using PropertyTools.Wpf;
using XmlReplace.Converters.CustomConverters.Hig6DictantInputPart;
using XmlReplace.Converters.CustomConverters.Hig6GramInputPart;
using XmlReplace.Converters.ReplaceValue;
using XmlReplace.Converters.SimpleInput;
using XmlReplace.Converters.SimpleOutput;

namespace XmlReplace
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        public MainWindow()
        {
            InitializeComponent();

            var convItem = new ConverterItem();
            var type = typeof (SimpleInputConverter);
            convItem.Init(type.Assembly.FullName, type.FullName, type.Name, SimpleInputConverter.StaticDescription);
            ConvertersList.Items.Add(convItem);

            convItem = new ConverterItem();
            type = typeof(InputPartConverter);
            convItem.Init(type.Assembly.FullName, type.FullName, type.Name, InputPartConverter.StaticDescription);
            ConvertersList.Items.Add(convItem);

            // Dictant
            convItem = new ConverterItem();
            type = typeof(DictantInputPartConverter);
            convItem.Init(type.Assembly.FullName, type.FullName, type.Name, DictantInputPartConverter.StaticDescription);
            ConvertersList.Items.Add(convItem);


            convItem = new ConverterItem();
            type = typeof (XsltConverter);
            convItem.Init(type.Assembly.FullName, type.FullName, type.Name, XsltConverter.StaticDescription);
            ConvertersList.Items.Add(convItem);

            convItem = new ConverterItem();
            type = typeof(SimpleOutputConverter);
            convItem.Init(type.Assembly.FullName, type.FullName, type.Name, SimpleOutputConverter.StaticDescription);
            ConvertersList.Items.Add(convItem);

            convItem = new ConverterItem();
            type = typeof(SplitOutputConverter);
            convItem.Init(type.Assembly.FullName, type.FullName, type.Name, SplitOutputConverter.StaticDescription);
            ConvertersList.Items.Add(convItem);

            convItem = new ConverterItem();
            type = typeof(ReplaceValueConverter);
            convItem.Init(type.Assembly.FullName, type.FullName, type.Name, ReplaceValueConverter.StaticDescription);
            ConvertersList.Items.Add(convItem);

            LoadFavorites();
        }

        // Convert the book price to a new price using the conversion factor. 
        public class BookPrice
        {

            private decimal newprice = 0;

            public decimal NewPriceFunc(decimal price, decimal conv)
            {
                decimal tmp = price * conv;
                newprice = decimal.Round(tmp, 2);
                return newprice;
            }
        }

        private void BConvertClick(object sender, RoutedEventArgs e)
        {
            var text = "";// TbSource.Text;
            LogOut.Document = new FlowDocument
            {
                FontFamily = new FontFamily("Colibri"),
                FontSize = 12
            };


            foreach (WorkItem workItem in ConvertersSequence.Items)
            {
                try
                {
                    if (workItem.Converter.ConverterType == IsConverter.ConverterTypes.Input)
                    {// Если это входной конвертер
                        text = workItem.Converter.InputData();
                        continue;
                    }
                    if (workItem.Converter.ConverterType == IsConverter.ConverterTypes.Output)
                    {// Если это выходной конвертер
                        workItem.Converter.ConvertToOutput(text);
                        break;
                    }
                    text = workItem.Converter.Convert(text);
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.ToString());
                    break;
                }
            }
            //OutpTextBox.Text = text;
        }

        private void MetroWindowClosed(object sender, EventArgs e)
        {
            SaveFavorites();
            Properties.Settings.Default.Save();
        }

        private void AddConverterClick(object sender, RoutedEventArgs e)
        {
            if (ConvertersList.SelectedItem == null)
                return;

            var addingItem = (ConverterItem) ConvertersList.SelectedItem;


            var ttt = new XsltConverter();

            var converterHandle = Activator.CreateInstance(addingItem.AssemblyName, addingItem.ConverterFullName);
            var converter = (IsConverter)converterHandle.Unwrap();

//            if (converter.RequreParams)
            {
                var propWnd = new ParamsWindow {DataContext = converter};
                propWnd.ShowDialog();
            }
            converter.OnLog -= converter_OnLog;
            converter.OnLog += converter_OnLog;
            converter.Init();
            ConvertersSequence.Items.Add(new  WorkItem(converter));
        }

        void converter_OnLog(object sender, IsConverter.LogEventArgs e)
        {

            var line = new Run(e.Msg);
            switch (e.MsgType)
            {
                case IsConverter.LogEventArgs.MsgTypes.Text:
                    break;
                case IsConverter.LogEventArgs.MsgTypes.Info:
                    line.Foreground = Brushes.DodgerBlue;
                    break;
                case IsConverter.LogEventArgs.MsgTypes.Warning:
                    line.Foreground = Brushes.Goldenrod;
                    break;
                case IsConverter.LogEventArgs.MsgTypes.Error:
                    line.Foreground = Brushes.Red;
                    break;
            }
            var para = new Paragraph(line);
            para.Margin = new Thickness(0);
            LogOut.Document.Blocks.Add(para);
        }

        private void BAbout_OnClick(object sender, RoutedEventArgs e)
        {
            var aboutWin = new AboutDialog(this);
            aboutWin.ShowDialog();
        }

        
        [XmlType("ConverterParamForSerial")]
        [XmlInclude(typeof(XsltConverter.XsltConverterProperties)), 
        XmlInclude(typeof(XsltConverter.XsltConverterProperties)),
        XmlInclude(typeof(SplitOutputConverter.SplitOutputConverterProperties)),
        XmlInclude(typeof(ReplaceValueConverter.ReplaceValueConverterProperties)),
        XmlInclude(typeof(DictantInputPartConverter.DictantInputPartConverterProperties))]
        public class ConverterParamForSerial
        {
            [XmlElement("AssemblyName")]
            public string AssemblyName;
            [XmlElement("CurrentName")]
            public string CurrentName;
            [XmlElement("FullName")]
            public string FullName;
            [XmlElement("Param")]
            public object Param;
        }

        [XmlType("ConverterContainerForSerial")]
        public class ConverterContainerForSerial
        {
            [XmlElement("Name")]
            public string Name;
            [XmlElement("OneContainer")]
            public ConverterParamForSerial OneContainer;
            [XmlArray("ContainerSet", ElementName = "Container")]
            public ConverterParamForSerial[] ContainerSet;
        }

        private void LoadFavorites()
        {
            var serializer = CreateSerializerForFavorites();

            ConverterContainerForSerial[] deSer;

            using (var fs = new FileStream("favorites.fvrts", FileMode.Open))
            {
                try
                {
                    deSer = (ConverterContainerForSerial[])serializer.Deserialize(fs);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    return;
                }
                finally
                {
                    fs.Close();
                }
            }
            if (deSer == null)
            {
                MessageBox.Show("Ошибка открытия");
                return;
            }

            foreach (var converterParamForSerial in deSer)
            {
                if (converterParamForSerial.OneContainer != null)
                {
                    var converter = CreateConverter(converterParamForSerial.OneContainer);
                    var converterContainer = CreateConverterContainer(converter);

                    LbFavorites.Items.Add(converterContainer);
                }
                else if (converterParamForSerial.ContainerSet != null && converterParamForSerial.ContainerSet.Length > 0)
                {
                    var set = new IsConverter[converterParamForSerial.ContainerSet.Length];
                    for (int i = 0; i < converterParamForSerial.ContainerSet.Length; i++)
                    {
                        var paramForSerial = converterParamForSerial.ContainerSet[i];
                        var converter = CreateConverter(paramForSerial);
                        set[i] = converter;
                    }
                    var converterContainer = new ConverterContainer
                    {
                        Name = converterParamForSerial.Name,
                        ConverterSet = set
                    };
                    LbFavorites.Items.Add(converterContainer);
                }
            }
        }

        private IsConverter CreateConverter(ConverterParamForSerial converterData)
        {
            var converterHandle = Activator.CreateInstance(converterData.AssemblyName,
                converterData.FullName);
            var converter = (IsConverter) converterHandle.Unwrap();
            converter.Name = converterData.CurrentName;
            if (converter.RequreParams)
                converter.ParamsList = converterData.Param;
            converter.OnLog += converter_OnLog;
            converter.Init();
            return converter;
        }

        class ConverterContainer
        {
            public string Name { get; set; }
            public IsConverter Converter { get; set; }
            public IsConverter[] ConverterSet { get; set; }
        }

        private void DeserialiseListBox(ListBox listBox, string fileName)
        {
            var serializer = CreateSerializer();

            ConverterParamForSerial[] deSer;

            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                try
                {
                    deSer = (ConverterParamForSerial[])serializer.Deserialize(fs);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    return;
                }
                finally
                {
                    fs.Close();                    
                }
            }
            if (deSer == null)
            {
                MessageBox.Show("Ошибка открытия");
                return;
            }

            foreach (var converterParamForSerial in deSer)
            {
                var converter = CreateConverter(converterParamForSerial);
//                var converterHandle = Activator.CreateInstance(converterParamForSerial.AssemblyName, converterParamForSerial.FullName);
//                var converter = (IsConverter)converterHandle.Unwrap();
//                converter.Name = converterParamForSerial.CurrentName;
//                if (converter.RequreParams)
//                    converter.ParamsList = converterParamForSerial.Param;
//                converter.Init();

                listBox.Items.Add(new WorkItem(converter));
            }

        }

        private void SaveFavorites()
        {
            var convList = (from ConverterContainer kvp in LbFavorites.Items
                            select CreateConverterContainerForSerial(kvp)).ToArray();

            var serializer = CreateSerializerForFavorites();
            using (var fs = new FileStream("favorites.fvrts", FileMode.Create))
            {
                serializer.Serialize(fs, convList);
                fs.Close();
            }
        }

        private ConverterContainerForSerial CreateConverterContainerForSerial(ConverterContainer converterContainer)
        {
            if (converterContainer.Converter != null)
            {
                var converterParamForSerial = CreateConverterParamForSerial(converterContainer.Converter);
                return new ConverterContainerForSerial
                {
                    Name = converterParamForSerial.CurrentName,
                    OneContainer = converterParamForSerial
                };
            }
            else if (converterContainer.ConverterSet != null && converterContainer.ConverterSet.Length > 0)
            {
                var set = new ConverterParamForSerial[converterContainer.ConverterSet.Length];
                for (var i = 0; i < converterContainer.ConverterSet.Length; i++)
                {
                    var converterParamForSerial = CreateConverterParamForSerial(converterContainer.ConverterSet[i]);
                    set[i] = converterParamForSerial;
                }
                return new ConverterContainerForSerial
                {
                    Name = converterContainer.Name,
                    ContainerSet = set
                };
            }
            throw new ApplicationException();
        }


        private void BSaveConvSequenceClick(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "*.convSeq|*.convSeq", 
                DefaultExt = ".convSeq",
                InitialDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
            };

            if (saveDialog.ShowDialog() != true)
                return;

            SerializeListBox(ConvertersSequence, saveDialog.FileName);
        }

        private void SerializeListBox(ListBox listBox, string fileName)
        {
            var convList = (from WorkItem kvp in listBox.Items
                            select CreateConverterParamForSerial(kvp.Converter)).ToArray();

            var serializer = CreateSerializer();
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                serializer.Serialize(fs, convList);
                fs.Close();
            }
        }

        private ConverterParamForSerial CreateConverterParamForSerial(IsConverter converter)
        {
            return new ConverterParamForSerial
            {
                AssemblyName = converter.GetType().Assembly.FullName, //!!!!!!!!!
                FullName = converter.GetType().FullName,
                CurrentName = converter.Name,
                Param = converter.ParamsList
            };
        }

        private XmlSerializer CreateSerializerForFavorites()
        {
            return new XmlSerializer(typeof(ConverterContainerForSerial[]));
        }


        private XmlSerializer CreateSerializer()
        {
            return new XmlSerializer(typeof(ConverterParamForSerial[]), new[]
            {
                typeof(XsltConverter.XsltConverterProperties),
                typeof(SplitOutputConverter.SplitOutputConverterProperties)
            });
        }

        private void BLoadConvSequenceClick(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog()
            {
                Filter = "*.convSeq|*.convSeq",
                DefaultExt = ".convSeq",
                InitialDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
            };
            if (openDialog.ShowDialog() != true)
                return;

            ConvertersSequence.Items.Clear();
            DeserialiseListBox(ConvertersSequence, openDialog.FileName);
        }

        private void MenuItemParamsOnClick(object sender, RoutedEventArgs e)
        {
            var mnu = (MenuItem)sender;
            var listBox = (ListBox) ((ContextMenu) mnu.Parent).PlacementTarget;

            if (listBox.SelectedItem == null)
                return;

            var convItem = ((WorkItem)listBox.SelectedItem).Converter;
            var propWnd = new ParamsWindow {DataContext = convItem};

            propWnd.ShowDialog();
            convItem.OnLog -= converter_OnLog;
            convItem.OnLog += converter_OnLog;
            convItem.Init();

        }

        private void MenuItemRemoveOnClick(object sender, RoutedEventArgs e)
        {
            var mnu = (MenuItem)sender;
            var listBox = (ListBox)((ContextMenu)mnu.Parent).PlacementTarget;

            if (listBox.SelectedItem == null)
                return;

            listBox.Items.Remove(listBox.SelectedItem);
        }

        private void BAddToFavoritesClick(object sender, RoutedEventArgs e)
        {
            if (ConvertersSequence.SelectedItem == null)
            {
                MessageBox.Show("Выберите конвертер из списка");
                return;
            }

            var converter = ((WorkItem) ConvertersSequence.SelectedItem).Converter;

            LbFavorites.Items.Add(CreateConverterContainer(converter));
            SaveFavorites();
        }

        private ConverterContainer CreateConverterContainer(IsConverter converter)
        {
            return new ConverterContainer
            {
                Name = converter.Name,
                Converter = converter
            };
        }

        private void BAddFromFavorites_OnClick(object sender, RoutedEventArgs e)
        {
            AddFromFavorites();
        }

        private void AddFromFavorites()
        {
            if (LbFavorites.SelectedItem == null)
            {
                MessageBox.Show("Выберите конвертер из списка");
                return;
            }

            var converterContainer = (ConverterContainer)LbFavorites.SelectedItem;
            if (converterContainer.Converter != null)
            {
                ConvertersSequence.Items.Add(new WorkItem(converterContainer.Converter));
            }
            else if (converterContainer.ConverterSet != null && converterContainer.ConverterSet.Length > 0)
            {
                foreach (var isConverter in converterContainer.ConverterSet)
                {
                    ConvertersSequence.Items.Add(new WorkItem(isConverter));
                }
            }
        }

        private void MenuItemParamsFavoriteOnClick(object sender, RoutedEventArgs e)
        {
            var mnu = (MenuItem)sender;
            var listBox = (ListBox)((ContextMenu)mnu.Parent).PlacementTarget;

            if (listBox.SelectedItem == null)
                return;

            var convContainerItem = (ConverterContainer)listBox.SelectedItem;
            if (convContainerItem.Converter != null)
            {
                var propWnd = new ParamsWindow {DataContext = convContainerItem.Converter};
                propWnd.ShowDialog();
                convContainerItem.Name = convContainerItem.Converter.Name;
                convContainerItem.Converter.OnLog -= converter_OnLog;
                convContainerItem.Converter.OnLog += converter_OnLog;
                convContainerItem.Converter.Init();
                listBox.Items.Refresh();
                SaveFavorites();
            }
            else if (convContainerItem.ConverterSet != null && convContainerItem.ConverterSet.Length > 0)
            {
                var nameWin = new InputNameWindow
                {
                    SetName = convContainerItem.Name
                };
                if (nameWin.ShowDialog() == true)
                {
                    convContainerItem.Name = nameWin.SetName;
                    listBox.Items.Refresh();
                    SaveFavorites();
                }

            }
            else
            {
                return;
            }

        }

        private void MenuItemRemoveFavoriteOnClick(object sender, RoutedEventArgs e)
        {
            var mnu = (MenuItem)sender;
            var listBox = (ListBox)((ContextMenu)mnu.Parent).PlacementTarget;

            if (listBox.SelectedItem == null)
                return;

            listBox.Items.Remove(listBox.SelectedItem);
            SaveFavorites();
        }

        private void BAddAllToFavoritesClick(object sender, RoutedEventArgs e)
        {
            if (ConvertersSequence.Items.Count == 0)
            {
                MessageBox.Show("Список пуст");
                return;
            }

            var nameWin = new InputNameWindow();
            if (nameWin.ShowDialog() != true)
                return;

            var convSet = new IsConverter[ConvertersSequence.Items.Count];
            for (var i = 0; i < ConvertersSequence.Items.Count; i++)
            {
                var converter = ((WorkItem)ConvertersSequence.Items[i]).Converter;
                convSet[i] = converter;
            }

            var cont = new ConverterContainer
            {
                Name = "[" + nameWin.SetName + "]",
                ConverterSet = convSet
            };

            LbFavorites.Items.Add(cont);
            SaveFavorites();
        }

        private void BMoveUp_OnClick(object sender, RoutedEventArgs e)
        {
            if (ConvertersSequence.SelectedItem == null)
            {
                MessageBox.Show("Выберите конвертер из списка");
                return;
            }

            var selectedIndex = ConvertersSequence.SelectedIndex;

            if (selectedIndex > 0)
            {
                var itemToMoveUp = ConvertersSequence.Items[selectedIndex];
                ConvertersSequence.Items.RemoveAt(selectedIndex);
                ConvertersSequence.Items.Insert(selectedIndex - 1, itemToMoveUp);
                ConvertersSequence.SelectedIndex = selectedIndex - 1;
            }
        }

        private void BMoveDown_OnClick(object sender, RoutedEventArgs e)
        {
            if (ConvertersSequence.SelectedItem == null)
            {
                MessageBox.Show("Выберите конвертер из списка");
                return;
            }

            var selectedIndex = ConvertersSequence.SelectedIndex;

            if (selectedIndex + 1 < ConvertersSequence.Items.Count)
            {
                var itemToMoveDown = ConvertersSequence.Items[selectedIndex];
                ConvertersSequence.Items.RemoveAt(selectedIndex);
                ConvertersSequence.Items.Insert(selectedIndex + 1, itemToMoveDown);
                ConvertersSequence.SelectedIndex = selectedIndex + 1;
            }
        }

        private void LbFavorites_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AddFromFavorites();
        }
    }
}
