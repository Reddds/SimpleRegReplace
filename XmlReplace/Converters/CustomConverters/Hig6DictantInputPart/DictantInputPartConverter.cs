using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using IsSilverlightUtils;
using WPFToXmlBase;
using XmlReplace.Converters.CustomConverters.Hig6GramInputPart;

namespace XmlReplace.Converters.CustomConverters.Hig6DictantInputPart
{
    public class DictantInputPartConverter : IsConverter
    {
        public DictantInputPartConverter()
        {
            ConverterType = ConverterTypes.Input;
            ConverterRequreParams = true;

            _properties = new DictantInputPartConverterProperties();
        }
        private DictantInputPartConverterProperties _properties;
        private Base _base;

        [XmlType("DictantInputPartConverterProperties")]
        public class DictantInputPartConverterProperties
        {
            [Category("Путь к базе звкуов")]
            [XmlElement("SoundBasePath")]
            public string SoundBasePath { get; set; }
        }

        public override Object ParamsList
        {
            get
            {
                return _properties;
            }
            set
            {
                _properties = (DictantInputPartConverterProperties)value;
            }
        }

        private class Gap
        {
            public int Size { get; set; }
            public string Placeholder { get; set; }
            public string Right { get; set; }
            public string Text { get; set; }
        }

        enum SoundTypes
        {
            None,
            Phoneme,
            Word,
            Phrase
        }

        public override string InputData()
        {
            _base = Base.LoadFromFile(_properties.SoundBasePath);

            var win = new DictantInputPartConverterWindow();
            if (win.ShowDialog() != true)
                throw new Exception("Ввод отменён");


            var sectionId = win.SectionId;


            //var useGroups = win.UseGrops;

            var xmlPath = win.XmlPath;
            var iniPath = Path.ChangeExtension(xmlPath, ".ini");

            var exerXmlStr = IsSilverlightUtils.TextUtils.ReadFileToEnd(xmlPath);
            exerXmlStr = exerXmlStr.Replace("xml:stylesheet", "xml-stylesheet");
            var exerXml = new XmlDocument();
            exerXml.LoadXml(exerXmlStr);
            if (exerXml.DocumentElement == null)
                throw new Exception("Ошибка загрузки Xml упражнения (DocumentElement == null)");

            var iniFile = new IniFile(iniPath);
            var listFile = iniFile.ReadValue("Common", "List");

            var curDir = Path.GetDirectoryName(xmlPath);
            Debug.Assert(curDir != null);
            var listPath = Path.Combine(curDir, listFile);
            var list = IsSilverlightUtils.TextUtils.ReadFileToEnd(listPath);
            var listArray = list.Split('\n');


            var settingsXml = "<Settings>" + Environment.NewLine;

            foreach (var listItem in listArray)
            {
                var phraseText = listItem.Trim();
//                SoundTypes soundType;
                var curPhrase = _base.Phrase.Find(p => p.PhraseText == phraseText);
                if (curPhrase != null)
                {
                    string newPhraseId;
                    if (!string.IsNullOrEmpty(curPhrase.Reference))
                        newPhraseId = curPhrase.Reference;
                    else
                        newPhraseId = curPhrase.phraseId;
//                    soundType = SoundTypes.Phrase;
                    settingsXml += string.Format("<Item Type=\"Phrase\" Id=\"{0}\" Right=\"{1}\" />\n",
                    newPhraseId, phraseText);
                }
                else
                {
                    var curWord = _base.Word.Find(w => w.WordText == phraseText);
                    if (curWord != null)
                    {
                        string newPhraseId;
                        if (!string.IsNullOrEmpty(curWord.Reference))
                            newPhraseId = curWord.Reference;
                        else
                            newPhraseId = curWord.wordId;
//                        soundType = SoundTypes.Word;
                        settingsXml += string.Format("<Item Type=\"Word\" Id=\"{0}\" Right=\"{1}\" />\n",
                    newPhraseId, phraseText);

                    }
                    else
                    {
                        var curPhoneme = _base.Phoneme.Find(w => w.PhonemeText == phraseText);
                        if (curPhoneme != null)
                        {
                            string newPhraseId;
                            if (!string.IsNullOrEmpty(curPhoneme.Reference))
                                newPhraseId = curPhoneme.Reference;
                            else
                                newPhraseId = curPhoneme.phonemeId;
//                            soundType = SoundTypes.Phoneme;
                            settingsXml += string.Format("<Item Type=\"Phoneme\" Id=\"{0}\" Right=\"{1}\" />\n",
                    newPhraseId, phraseText);
                        }
                        
                    }
                }
            }
            settingsXml += "</Settings>";

            string urlName = null;
            // Название упражнения -------------------------------------------

            var titles = "";
            var pageTitle = "";
            var nameRegEx =
                new Regex(
                    "<word spelling=\"(?<Name>[^\"]+)\" pronounce=\"[^\"]+\" language=\"(?<LangId>\\w+)\" id=\"\\d+\"/>");


            var mm = nameRegEx.Matches(win.ExerTitle);
            if (mm.Count == 0)
            {
                titles = win.ExerTitle;
                if (urlName == null)
                {
                    urlName = TextUtils.UnicodeTranslit(titles, true, true);
//                    additionalData.tbUrlName.Text = urlName;
                }
            }
            else
            {
                bool firstTitle = true;
                //var langDic = new Dictionary<string, string>();
                foreach (Match m in mm)
                {
                    var langId = m.Groups["LangId"].Value;
                    var title = m.Groups["Name"].Value;
                    langId = langId.ToLower();
                    switch (langId)
                    {
                        case "ch":
                            langId = "zh";
                            break;
                        case "sp":
                            langId = "es";
                            break;
                        case "gb":
                        case "all":
                            langId = "en";
                            if (urlName == null)
                            {
                                urlName = TextUtils.UnicodeTranslit(title, true, true);
//                                additionalData.tbUrlName.Text = urlName;
                            }
                            break;

                    }
                    pageTitle += string.Format("<title id=\"{0}\" value=\"{1}\" />\n", langId, title);

                    titles += (!firstTitle ? "||" : "") + langId + "|" + title;
                    firstTitle = false;
                    //<title langId="ru" value="Заголовок" />
                }

            }


            // ---------------------------------------------------------------

            var description = titles;

            var translitName = "";
            var exerName = "Dic_" + urlName;
            if (exerName.Length > 20)
                exerName = exerName.Substring(0, 20);
            var pageData = "<page>" + Environment.NewLine + pageTitle 
                + "<content><exer Name=\"" + exerName + "\" /></content></page>";


            using (var main26 = new main26Entities())
            {
                var nextOrder = GetNextOrder(main26, sectionId);

                var newExer = new is_exercises
                {
                    Name = exerName,
                    Category = 5, //Диктант
                    Description = description,
                    ExType = 3, //Диктант
                    Visible = 3,
                    LastChange = DateTime.UtcNow,
                    XmlData = settingsXml
                };
                main26.is_exercises.Add(newExer);


                var newPage = new is_Pages
                {
                    Name = description,
                    SectionId = sectionId,
                    UrlName = urlName,
                    Visible = 3,
                    LastChange = DateTime.UtcNow,
                    PageData = pageData,
                    Template = "main",
                    Order = nextOrder
                };
                main26.is_Pages.Add(newPage);
                main26.SaveChanges();
            }
            /*
             *  <page>
	                <title id="ru" value="Все слова" />
	                <content>
		                <exer Name="DictationAllWords" />
	                </content>
                </page>
             */

//            var xDoc = new XmlDocument();
//            xDoc.LoadXml("<Settings/>");
//            var docEl = xDoc.DocumentElement;




            return settingsXml;
        }

        private int GetNextOrder(main26Entities main26, long? sectionId)
        {
            // Ищем максимальный ордер для страниц в разделе
            var orderMax = (from p in main26.is_Pages
                            where p.SectionId == sectionId
                            select p).Max(po => (int?)po.Order);

            // Ищем максимальный ордер для разделов-страниц в разделе
            var sectionPagesOrderMax = (from s in main26.is_Sections
                                        where s.ParentSection == sectionId && s.Type == 1
                                        select s).Max(so => (int?)so.Order);

            // выбираем из них максимальный
            if (orderMax == null || sectionPagesOrderMax > orderMax)
                orderMax = sectionPagesOrderMax;

            var order = (orderMax ?? 0) + 10;
            return order;
        }

        static Tuple<string, string> GetIniValues(IniFile iniFile, int idInIni, List<string> partsToInsertList)
        {
            var iniSection = "B" + idInIni;
            var defVal = iniFile.ReadValue(iniSection, "DEF");

            var rightVal = iniFile.ReadValue(iniSection, "DATA").Trim();
            rightVal = rightVal.Replace("вЂ™", "'");

            if (partsToInsertList != null)
            {
                var rightIndex = partsToInsertList.IndexOf(rightVal);
                if (rightIndex < 0)
                {
                    MessageBox.Show("Правильный ответ не найден! " + rightVal);
                }
            }
            return new Tuple<string, string>(defVal, rightVal);
        }

        private static XmlElement CreateSent(XmlDocument xDoc, IniFile iniFile, string sentenceTypeStr, string idInIni,
            List<string> partsToInsertList)
        {
            var sentenceEl = xDoc.CreateElement("Sentence");
            if (!string.IsNullOrEmpty(sentenceTypeStr))
                sentenceEl.SetAttribute("SentenceType", sentenceTypeStr);
//            var insertEl = xDoc.CreateElement("Insert");
//            sentenceEl.AppendChild(insertEl);
//            insertEl.InnerText = resStr;

            var iniSection = "B" + idInIni;//(exerCounter + 1)

//            var defVal = iniFile.ReadValue(iniSection, "DEF");
//            if (!string.IsNullOrEmpty(defVal))
//            {
//                sentenceEl.SetAttribute("Placeholder", defVal.Trim());
//            }

            var rightVal = iniFile.ReadValue(iniSection, "DATA").Trim();
            rightVal = rightVal.Replace("вЂ™", "'");
            if (partsToInsertList == null)
                sentenceEl.SetAttribute("Right", rightVal);
            else
            {
                var rightIndex = partsToInsertList.IndexOf(rightVal);
                if (rightIndex < 0)
                {
                    MessageBox.Show("Правильный ответ не найден! " + rightVal);
                }
                sentenceEl.SetAttribute("Right", rightIndex.ToString());
            }
            return sentenceEl;
        }

//        private static XmlElement CreateSent(XmlDocument xDoc, List<Gap> megaSentance, string sentenceTypeStr, int idInIni,
//            List<string> partsToInsertList)
//        {
//            var sentenceEl = xDoc.CreateElement("Sentence");
//            if (!string.IsNullOrEmpty(sentenceTypeStr))
//                sentenceEl.SetAttribute("SentenceType", sentenceTypeStr);
//
//            foreach (var gap in megaSentance)
//            {
//                
//            }
//
//            if (partsToInsertList == null)
//                sentenceEl.SetAttribute("Right", rightVal);
//            else
//            {
//                var rightIndex = partsToInsertList.IndexOf(rightVal);
//                if (rightIndex < 0)
//                {
//                    MessageBox.Show("Правильный ответ не найден! " + rightVal);
//                }
//                sentenceEl.SetAttribute("Right", rightIndex.ToString());
//            }
//            return sentenceEl;
//        }
        public const string StaticDescription = "Ввод диктанта";

        public override string Description
        {
            get
            {
                return StaticDescription;
            }
        }
    }
}
