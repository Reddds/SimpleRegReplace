using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using PluginInterface;
using Clipboard = System.Windows.Clipboard;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using DragEventHandler = System.Windows.DragEventHandler;
using MessageBox = System.Windows.MessageBox;
using TextChangedEventArgs = FastColoredTextBoxNS.TextChangedEventArgs;

namespace SimpleRegReplace
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Pattern _pattern;
        private string _patternFileName;


        private readonly SortedList<string, string> _knownTemplatesDic = new SortedList<string, string>();
        private readonly BindingSource _knownTemplatesBindingSource = new BindingSource();

        public MainWindow()
        {
            InitializeComponent();

            KnownTemplates.ItemsSource = _knownTemplatesBindingSource;
            var knownTemplatesStr = Properties.Settings.Default.KnownTemplates;
            if (!string.IsNullOrEmpty(knownTemplatesStr))
            {
                char[] pairSep = {';'};
                char[] valSep = {','};
                var pairs = knownTemplatesStr.Split(pairSep, StringSplitOptions.RemoveEmptyEntries);
                foreach (var pair in pairs)
                {
                    var val = pair.Split(valSep, StringSplitOptions.RemoveEmptyEntries);
                    if (val.Length != 2)
                        continue;

                    _knownTemplatesDic.Add(val[0], val[1]);
                }
                _knownTemplatesBindingSource.DataSource = _knownTemplatesDic;
            }
            // Add using System.Windows.Controls;
            richTextBox1.AddHandler(DragOverEvent, new DragEventHandler(RichTextBox_DragOver), true);
            richTextBox1.AddHandler(DropEvent, new DragEventHandler(RichTextBox_Drop), true);

            //superBox.Font = new Font("Arial", 14);

        }
        private void RichTextBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var docPath = (string[])e.Data.GetData(DataFormats.FileDrop);

                // By default, open as Rich Text (RTF).
                var dataFormat = DataFormats.Text;

                //                // If the Shift key is pressed, open as plain text.
                //                if (e.KeyStates == DragDropKeyStates.ShiftKey)
                //                {
                //                    dataFormat = DataFormats.Text;
                //                }

                if (File.Exists(docPath[0]))
                {
                    try
                    {
                        // Open the document in the RichTextBox.
                        var range = new TextRange(richTextBox1.Document.ContentStart, richTextBox1.Document.ContentEnd);
                        var fStream = new FileStream(docPath[0], FileMode.OpenOrCreate);
                        range.Load(fStream, dataFormat);
                        fStream.Close();
                    }
                    catch (System.Exception)
                    {
                        MessageBox.Show("File could not be opened. Make sure the file is a text file.");
                    }
                }
            }
        }

        private void RichTextBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ?
                DragDropEffects.All : DragDropEffects.None;
            e.Handled = false;
        }

        private void Button2Click(object sender, RoutedEventArgs e)
        {
            var openPattern = new OpenPattern(true);
            openPattern.ShowDialog();
            _patternFileName = openPattern.FileName;
            _pattern = openPattern.Pattern;
            if (_knownTemplatesDic.ContainsKey(_pattern.Name))
                _knownTemplatesDic.Remove(_pattern.Name);
            _knownTemplatesDic.Add(_pattern.Name, _patternFileName);
            //_knownTemplatesDic[_pattern.Name] = _patternFileName;
            var index = _knownTemplatesDic.IndexOfKey(_pattern.Name);
            _knownTemplatesBindingSource.DataSource = null;
            _knownTemplatesBindingSource.DataSource = _knownTemplatesDic;
            KnownTemplates.SelectedIndex = index;
            StoreKnownTemplates();
            lReplacePattern.Content = "(Редактировать...)";//_pattern.Name + 
        }

        private void StoreKnownTemplates()
        {
            if (_knownTemplatesDic.Count == 0)
                return;

            var str = _knownTemplatesDic.Aggregate("", (i1, i2) => i1 + ';' + i2.Key + ',' + i2.Value);
            Properties.Settings.Default.KnownTemplates = str;
            Properties.Settings.Default.Save();
        }


        private void Button1Click(object sender, RoutedEventArgs e)
        {
            if (_pattern == null)
                return;

            var textRange = new TextRange(richTextBox1.Document.ContentStart, richTextBox1.Document.ContentEnd);
            var text = textRange.Text;

            var plugins = from pi in _pattern.Item
                          where pi.PluginName != null
                          group pi.PluginName by pi.PluginName;
            //select pi.PluginName;

            IRRPlugin curPlugin = null;
            if (plugins.Any())
            {// TODO пока костыль
                var tmpPlug = new WordListPlugin.WordList();
                if (tmpPlug.Init())
                    curPlugin = tmpPlug;
            }

            bool changed;
            do
            {
                changed = false;

                foreach (var item in _pattern.Item)
                {
                    if (!item.Enabled)
                        continue;
                    if (item.SearchString == null)
                        continue;



                    var exp = new Regex(item.SearchString, RegexOptions.Multiline | RegexOptions.Singleline);
                    // Поиск по одну и тому же паттерну пока ищется
                    var replaseStr = item.ReplaceString ?? "";
                    do
                    {
                        if (exp.IsMatch(text))
                        {
                            changed = true;
                            if (item.PluginName == null)
                            {
                                text = exp.Replace(text, replaseStr);
                            }
                            else
                            {
                                text = exp.Replace(text, curPlugin.MatchEvaluator);
                            }
                            continue;
                        }
                        break;
                    } while (true);
                }
            } while (changed);
            if (curPlugin != null)
                curPlugin.AfterReplaces(text);
            //textBlock1.Text = text;

            superBox.Text = text;
            //            superBox.TextSource
            //            if (text.IndexOf("span") >= 0)
            //            {
            //                MessageBox.Show("Ещё не всё!");
            //            }
        }

        private void Button3Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(superBox.Text);
        }

        private void Button4Click(object sender, RoutedEventArgs e)
        {
            richTextBox1.Document = new FlowDocument();
            richTextBox1.Paste();
        }

        private void Button5Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                DefaultExt = ".xml",
                Filter = "Шаблоны замены (.xml)|*.xml"
            };
            if (dlg.ShowDialog() != true)
            {
                return;
            }
            _patternFileName = dlg.FileName;
            var emptyPat = new Pattern();
            emptyPat.Name = dlg.SafeFileName;
            emptyPat.SaveToFile(_patternFileName);
            var openPattern = new OpenPattern(_patternFileName);
            openPattern.ShowDialog();
            _pattern = openPattern.Pattern;
            lReplacePattern.Content = _pattern.Name + "(Редактировать...)";
        }

        private void LReplacePatternClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_patternFileName))
                return;

            var openPattern = new OpenPattern(_patternFileName);
            openPattern.ShowDialog();
            _pattern = openPattern.Pattern;
            lReplacePattern.Content = _pattern.Name + "(Редактировать...)";
        }

        readonly EllipseStyle ellipseStyle = new EllipseStyle();

        private void SuperBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            //clear old styles of chars
            e.ChangedRange.ClearStyle(ellipseStyle);
            //append style for word 'Babylon'
            e.ChangedRange.SetStyle(ellipseStyle, @"\<module[^\>]+\>", RegexOptions.IgnoreCase);

        }

        private void MenuItemClick1(object sender, RoutedEventArgs e)
        {
            WordListPlugin.WordList.ReplaceAllLanguagesToNormal();
        }

        private void KnownTemplatesSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

            var fileName = ((KeyValuePair<string, string>) KnownTemplates.SelectedItem).Value;
            _patternFileName = fileName;

            var openPattern = new OpenPattern(fileName);
            openPattern.ShowDialog();
            _pattern = openPattern.Pattern;
            lReplacePattern.Content = "(Редактировать...)";
        }


    }
    /// <summary>
    /// This style will drawing ellipse around of the word
    /// </summary>
    class EllipseStyle : FastColoredTextBoxNS.Style
    {
        public override void Draw(Graphics gr, System.Drawing.Point position,
            Range range)
        {
            //get size of rectangle
            var size = GetSizeOfRange(range);
            //create rectangle
            var rect = new RectangleF(position, size);
            rect.Inflate(1, 1);

            var topRight = position;
            topRight.Offset(size.Width, 0);

            var bottomLeft = position;
            bottomLeft.Offset(0, size.Height);

            var bottomRight = position;
            bottomRight.Offset(size.Width, size.Height);


            var pen = new System.Drawing.Pen(System.Drawing.Color.Red, 2);

            gr.DrawLine(pen, position, topRight);
            gr.DrawLine(pen, bottomLeft, bottomRight);
            //gr.FillRectangle(Brushes.Salmon, rect);

            //            //inflate it
            //            rect.Inflate(2, 2);
            //            //get rounded rectangle
            //            var path = GetRoundedRectangle(rect, 7);
            //            //draw rounded rectangle
            //            gr.DrawPath(Pens.Red, path);
        }

    }

}
