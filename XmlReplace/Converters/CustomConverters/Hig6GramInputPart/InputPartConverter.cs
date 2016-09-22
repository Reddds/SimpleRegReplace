using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml;

namespace XmlReplace.Converters.CustomConverters.Hig6GramInputPart
{
    internal class InputPartConverter : IsConverter
    {
        public InputPartConverter()
        {
            ConverterType = ConverterTypes.Input;
            ConverterRequreParams = false;
        }

        private class Gap
        {
            public int Size { get; set; }
            public string Placeholder { get; set; }
            public string Right { get; set; }
            public string Text { get; set; }
        }

        public override string InputData()
        {
            var win = new InputPartConverterWindow();
            if (win.ShowDialog() != true)
                throw new Exception("Ввод отменён");

            var useGroups = win.UseGrops;

            var xmlPath = win.XmlPath;
            var iniPath = Path.ChangeExtension(xmlPath, ".ini");

            var exerXmlStr = IsSilverlightUtils.TextUtils.ReadFileToEnd(xmlPath);
            exerXmlStr = exerXmlStr.Replace("xml:stylesheet", "xml-stylesheet");
            var exerXml = new XmlDocument();
            exerXml.LoadXml(exerXmlStr);
            if (exerXml.DocumentElement == null)
                throw new Exception("Ошибка загрузки Xml упражнения (DocumentElement == null)");

            var iniFile = new IniFile(iniPath);
            var count = iniFile.ReadInteger("Common", "Count", 0);

            var xDoc = new XmlDocument();
            xDoc.LoadXml("<Settings/>");
            var docEl = xDoc.DocumentElement;


            // Поиск задания

            var taskEls = exerXml.DocumentElement.SelectNodes("/topic/page/task");
            var taskImageEl = exerXml.DocumentElement.SelectSingleNode("/topic/page/unit/image");
            if ((taskEls != null && taskEls.Count > 0) || taskImageEl != null)
            {
                var titleEls = xDoc.CreateElement("p");
                titleEls.SetAttribute("class", "exer-task");
                docEl.AppendChild(titleEls);

//                var resStr = "";

                if (taskEls != null)
                {
                    foreach (XmlElement xmlElement in taskEls)
                    {
                        var lang = xmlElement.GetAttribute("language");
                        lang = IsSilverlightUtils.TextUtils.OldHigginsLangIdToNormal(lang);
                        var langEl = xDoc.CreateElement("lang");
                        langEl.SetAttribute("id", lang);
                        langEl.InnerXml = xmlElement.InnerXml.Trim();
                        titleEls.AppendChild(langEl);
                        //resStr += "<lang id=\"" + lang + "\">" + xmlElement.InnerXml.Trim() + "</lang>\n";
                    }
                }
                if (taskImageEl != null)
                {
                    var imageEl = xDoc.CreateElement("img");
                    imageEl.SetAttribute("src", taskImageEl.InnerText + ".gif");
                    titleEls.AppendChild(imageEl);
                }
//                titleEls.InnerText = resStr;
            }


            // Поиск кусков для вставки

            List<string> partsToInsertList = null;
            var partsToInsertEls = exerXml.DocumentElement.SelectNodes("/topic/page/unit/list_of_words/list_multi");
            if (partsToInsertEls != null && partsToInsertEls.Count > 0)
            {
                partsToInsertList = new List<string>();
                var insertPartsEl = xDoc.CreateElement("InsertParts");
                docEl.AppendChild(insertPartsEl);
                var counter = 0;
                foreach (XmlElement partsToInsertEl in partsToInsertEls)
                {
                    var partValStr = partsToInsertEl.GetAttribute("value").Trim();
                    partValStr = partValStr.Replace('’', '\''); 
                    partsToInsertList.Add(partValStr);

                    var partEl = xDoc.CreateElement("Part");
                    insertPartsEl.AppendChild(partEl);
                    partEl.SetAttribute("Id", counter.ToString());
                    partEl.InnerText = partValStr;
                    counter++;
                }
            }

            var sentenceTypeDescription = false;

            // Поиск примеров

            var samplesInExer = exerXml.DocumentElement.SelectNodes("/topic/page/unit/example/ex");
            if (samplesInExer != null && samplesInExer.Count > 0)
            {
                var samplesEls = xDoc.CreateElement("Samples");
                docEl.AppendChild(samplesEls);
                var typeTypeSamples = false;
                foreach (XmlNode sampleInExer in samplesInExer)
                {
                    var sentenceTypeStr = "";
                    var resStr = "";
                    var sentenceEl = xDoc.CreateElement("Sentence");
                    samplesEls.AppendChild(sentenceEl);

                    foreach (XmlNode node in sampleInExer.ChildNodes)
                    {

                        if (node.Name == "sentence")
                        {
                            if (!node.HasChildNodes)
                            {
//                                MessageBox.Show("В примерах пустой тэг <Sentence/>!!!");
                                Log("В примерах пустой тэг <Sentence/>!!!", LogEventArgs.MsgTypes.Info);
                                continue;
                            }
                            var sentenceChilds = node.ChildNodes;
                            if (sentenceChilds[0].NodeType == XmlNodeType.Text)
                            {//Всякие переносы наверное и т.д. в строке
                                resStr += node.InnerXml;
                                continue;
                            }

                            if (sentenceChilds.Count > 1)
                            {

                                if (sentenceChilds[0].Name == "plus")
                                {
                                    sentenceTypeStr = "Positive";
                                    typeTypeSamples = true;
                                }
                                else if (sentenceChilds[0].Name == "minus")
                                {
                                    sentenceTypeStr = "Negative";
                                    typeTypeSamples = true;
                                }
                                else if (sentenceChilds[0].Name == "vopros")
                                {
                                    sentenceTypeStr = "Question";
                                    typeTypeSamples = true;
                                }
                                else
                                {
                                    MessageBox.Show("Неизвестный тег " + sentenceChilds[0].Name);
                                }
                                resStr += sentenceChilds[1].InnerText.Trim();
                                continue;
                            }
                        }
                        if (node.Name == "gap")
                        {
                            var gapValue = ((XmlElement)node).GetAttribute("value").Trim();
                            resStr += " [" + gapValue + "] ";
                        }
                    }

                    if (!string.IsNullOrEmpty(sentenceTypeStr))
                        sentenceEl.SetAttribute("SentenceType", sentenceTypeStr);
                    sentenceEl.InnerText = resStr;

                    //<Sentence SentenceType="Positive">I am having a rest now. [So is] my husband.</Sentence>
                }
                if (typeTypeSamples)
                {
                    samplesEls.SetAttribute("Type", "TypeSamples");
                    sentenceTypeDescription = true;
                }
            }

            // Поиск самих предложений

            var sentencesEl = xDoc.CreateElement("Sentences");
            docEl.AppendChild(sentencesEl);
            var blocks = exerXml.DocumentElement.SelectNodes("/topic/page/unit/block");
            if (blocks == null || blocks.Count == 0)
                throw new Exception("Ошибка загрузки Xml упражнения (Блоки не найдены)");
            var gapSize = 0;
            var exerCounter = 0;
            // спецкостыль "c) <vopros/> "
            var sentRegEx = new Regex(@"^\s*\w\)\s*<(?<SentType>\w+)\s*/>(?<Other>.*)$");
            for (var i = 0; i < blocks.Count; i++)
            {

                var mainEls = blocks[i].SelectNodes("main");
                if (mainEls == null || mainEls.Count == 0)
                {
                    // Возможно, упражнение на сборку из кусков
                    //MessageBox.Show("В блоке отсутствуют main");
                    //Выбираем слова для вставки
                    var stringEls = blocks[i].SelectNodes("string");
                    if (stringEls == null || stringEls.Count == 0)
                        continue;
                    var idInIniStr = ((XmlElement)stringEls[0]).GetAttribute("num");
                    string idInIni = idInIniStr.Trim();
//                    if (!int.TryParse(idInIniStr, out idInIni))
//                    {
//                        throw new Exception("Id num не найден!");
//                    }
                    var combineEl = xDoc.CreateElement("Combine");
                    foreach (var stringEl in stringEls)
                    {
                        var wordEl = xDoc.CreateElement("Word");
                        wordEl.InnerText = ((XmlElement) stringEl).GetAttribute("content");
                        combineEl.AppendChild(wordEl);
                    }

                    var sentenceEl = CreateSent(xDoc, iniFile, null, idInIni, null);
                    sentenceEl.AppendChild(combineEl);
                    sentencesEl.AppendChild(sentenceEl);
                    continue;
                }
                XmlElement groupEl = null;
                // Предложение с несколькими пробелами
                List<Gap> megaSentance = null;
                if (useGroups && mainEls.Count > 1)
                {
//                    if(useGroups)
                        groupEl = xDoc.CreateElement("Group");
                    sentencesEl.AppendChild(groupEl);
//                    else 
//                        megaSentance = new List<Gap>();
                }
                foreach (XmlNode mainEl in mainEls)
                {
                    var idInIniStr = ((XmlElement) mainEl).GetAttribute("num");
                    string idInIni = idInIniStr.Trim();
//                    if (!int.TryParse(idInIniStr, out idInIni))
//                    {
//                        throw new Exception("Id num не найден!");
//                    }

                    var resStr = "";
                    var sentenceTypeStr = "";
                    foreach (XmlNode node in mainEl.ChildNodes)
                    {
                        if (node.Name == "br")
                        {
                            resStr += "<br />";
                            continue;
                        }
                        if (node.Name == "sentence")
                        {
                            var nodeText = node.InnerXml.Trim();
                            var m = sentRegEx.Match(nodeText);
                            if (m.Success)
                            {
                                switch (m.Groups["SentType"].Value)
                                {
                                    case "plus":
                                        sentenceTypeStr = "Positive";
                                        sentenceTypeDescription = true;
                                        break;
                                    case "minus":
                                        sentenceTypeStr = "Negative";
                                        sentenceTypeDescription = true;
                                        break;
                                    case "vopros":
                                        sentenceTypeStr = "Question";
                                        sentenceTypeDescription = true;
                                        break;
                                    default:
                                        MessageBox.Show("Неизвестный тег " + m.Groups["SentType"].Value);
                                        break;
                                }
                                resStr += m.Groups["Other"].Value.Trim();
                                continue;
                            }
                            if (node.ChildNodes.Count == 0)
                            {
                                // Пустой тэг
                                Log("Пустой тэг <sentence/>!!!", LogEventArgs.MsgTypes.Info);
                                continue;
                            }


                            var sentenceChilds = node.ChildNodes;

                            if (sentenceChilds[0].NodeType == XmlNodeType.Text)
                            {
                                
                                
                                //Всякие переносы наверное и т.д. в строке
                                if (string.IsNullOrEmpty(sentenceTypeStr))
                                    sentenceTypeStr = "None";
                                resStr += node.InnerXml.Trim();
                                continue;
                            }
                            if (sentenceChilds.Count > 1)
                            {
                                if (sentenceChilds[0].Name == "plus")
                                {
                                    sentenceTypeStr = "Positive";
                                    sentenceTypeDescription = true;
                                }
                                else if (sentenceChilds[0].Name == "minus")
                                {
                                    sentenceTypeStr = "Negative";
                                    sentenceTypeDescription = true;
                                }
                                else if (sentenceChilds[0].Name == "vopros")
                                {
                                    sentenceTypeStr = "Question";
                                    sentenceTypeDescription = true;
                                }
                                else
                                {
                                    MessageBox.Show("Неизвестный тег " + sentenceChilds[0].Name);
                                }
                                resStr += sentenceChilds[1].InnerText.Trim();
                            }
                        }
                        else if (node.Name == "gap_paste_multi" || node.Name == "gap")
                        {
                            var sizeStr = ((XmlElement)node).GetAttribute("size");
                            int size;
                            var curGapSize = "#";
                            if (int.TryParse(sizeStr, out size))
                            {
                                if (size > gapSize)
                                    gapSize = size;
                                curGapSize = curGapSize + size;
                            }
                            else
                            {
                                size = gapSize;
                            }



                            var numStr = ((XmlElement)node).GetAttribute("num");
                            int num;
                            var curPlaceholder = "";
                            var curRight = "";
                            if (int.TryParse(numStr, out num))
                            {
                                var iniValues = GetIniValues(iniFile, num, partsToInsertList);
                                curPlaceholder = iniValues.Item1;
                                curRight = iniValues.Item2;
                            }

                            var gapValue = ((XmlElement)node).GetAttribute("value");
                            if (!string.IsNullOrEmpty(gapValue))
                                curPlaceholder = gapValue.Trim();

                            curGapSize = curPlaceholder + curGapSize;
                            resStr += " [" + curGapSize + "] ";
//                            if (megaSentance != null)
//                            {
//                                megaSentance.Add(new Gap
//                                {
//                                    Size = size,
//                                    Placeholder = curPlaceholder,
//                                    Right = curRight
//                                });
//                            }
                        }
                    }

                    var sentenceEl = CreateSent(xDoc, iniFile, sentenceTypeStr, idInIni, partsToInsertList);
                    if (groupEl != null)
                        groupEl.AppendChild(sentenceEl);
                    else if (megaSentance != null)
                    {
                        
                    }
                    else 
                        sentencesEl.AppendChild(sentenceEl);
                    var insertEl = xDoc.CreateElement("Insert");
                    sentenceEl.AppendChild(insertEl);
                    insertEl.InnerText = resStr;

                    exerCounter++;
                }
//                if(useGroups)
//                    sentencesEl.AppendChild(groupEl);
//                else
//                {// Объединяем в одно предложение в несколькими пробелами
//                    groupEl.FirstChild
//                }
                //MessageBox.Show(resStr);
                //<Sentence SentenceType="Positive" Right="2">The children are playing football. [] my son.</Sentence>
            }

            docEl.SetAttribute("GapSize", gapSize.ToString());
            docEl.SetAttribute("Mix", "true");
            if (sentenceTypeDescription)
            {
                docEl.SetAttribute("SentenceTypeDescription", "true");
            }
            //MessageBox.Show(iniFile.IniReadValue("Common", "Count"));

            return xDoc.OuterXml;
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
        public const string StaticDescription = "Ввод упражнения вставки части предложения";

        public override string Description
        {
            get
            {
                return StaticDescription;
            }
        }
    }
}
