using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using IsSilverlightUtils;
using PluginInterface;
using IsUtils;
using WPFToXmlBase;

namespace WordListPlugin
{
    public class WordList : IRRPlugin
    {
        private Base _base;

        private readonly List<is_exercises> _exers = new List<is_exercises>();

        private string strOut = "";

        public bool Init()
        {

            string basePath;
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.SoundBasePath))
            {
                var ofd = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Xml файлы базы|*.xml",
                    Title = "Выберите базу звуков"
                };
                if (ofd.ShowDialog() == true)
                {
                    basePath = ofd.FileName;
                    Properties.Settings.Default.SoundBasePath = basePath;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    return false;
                }

            }
            else
            {
                basePath = Properties.Settings.Default.SoundBasePath;
            }
            _base = Base.LoadFromFile(basePath);
            return true;
        }

        enum SoundTypes
        {
            None,
            Phoneme,
            Word,
            Phrase
        }
        //soundType="Phoneme"
        public string MatchEvaluator(Match match)
        {
            var newPhraseId = "!!!!!!!!!!!!!!!!";
            var oldPhraseId = match.Groups["PhraseId"].Value;
            var title = match.Groups["Title"].Value;
            SoundTypes soundType;
            var res = "";

            var curPhrase = _base.Phrase.Find(p => p.PhraseText == oldPhraseId);
            if (curPhrase != null)
            {
                if (!string.IsNullOrEmpty(curPhrase.Reference))
                    newPhraseId = curPhrase.Reference;
                else
                    newPhraseId = curPhrase.phraseId;
                soundType = SoundTypes.Phrase;
                res = string.Format("<module type=\"sound\" title=\"{0}\" phraseId=\"{1}\" soundType=\"Phrase\" />",
                title, newPhraseId);
            }
            else
            {
                var curWord = _base.Word.Find(w => w.WordText == oldPhraseId);
                if (curWord != null)
                {
                    if (!string.IsNullOrEmpty(curWord.Reference))
                        newPhraseId = curWord.Reference;
                    else
                        newPhraseId = curWord.wordId;
                    soundType = SoundTypes.Word;
                    res = string.Format("<module type=\"sound\" title=\"{0}\" phraseId=\"{1}\" soundType=\"Word\" />", title, newPhraseId);

                }
                else
                {
                    var curPhoneme = _base.Phoneme.Find(w => w.PhonemeText == oldPhraseId);
                    if (curPhoneme != null)
                    {
                        if (!string.IsNullOrEmpty(curPhoneme.Reference))
                            newPhraseId = curPhoneme.Reference;
                        else
                            newPhraseId = curPhoneme.phonemeId;
                        soundType = SoundTypes.Phoneme;
                        res = string.Format("<module type=\"sound\" title=\"{0}\" phraseId=\"{1}\" soundType=\"Phoneme\" />", title, newPhraseId);

                    }
                    else
                    {
                        return string.Format("<module type=\"sound\" title=\"{0}\" phraseId=\"{1}\" />",
                                             title, newPhraseId);
                    }
                }
            }

            if (soundType != SoundTypes.Phoneme)
            {
                _exers.Add(new is_exercises
                {
                    ExType = 1,
                    Name = newPhraseId,
                    Category = 3,
                    Description = title,
                    Visible = 3,
                    XmlData = "<Settings><Sound Id=\"" + newPhraseId + "\" /></Settings>"
                });
            }

            //            strOut += "-------------------------------------------\n";
            //            strOut += "Id = " + newPhraseId + "\n";
            //            strOut += "Type = PronCheckSound\n";
            //            strOut += "Category = 2\n";
            //            strOut += "Description = " + title + "\n";
            //            strOut += "Settings = <Settings><Sound Id=\"" + newPhraseId + "\" /></Settings>\n";
            //            strOut += "Visible = 3\n\n";


            return res;
        }

        private IQueryable<is_exercises> _siteExers;

        public void AfterReplaces(string replacingResult)
        {


            if (_exers.Count > 0)
            {
                try
                {
                    // Добавляем упражнения в базу
                    var justAdded = new HashSet<string>();
                    using (var main26 = new main26Entities1())
                    {
                        if (_siteExers == null)
                            _siteExers = from ex in main26.is_exercises
                                        select ex;
                        foreach (var exer in _exers)
                        {
                            if (justAdded.Contains(exer.Name))
                                continue;
                            var exer1 = exer;
                            var exExists = from ex in _siteExers
                                           where ex.Name == exer1.Name
                                           select ex;
                            if (!exExists.Any())
                            {
                                main26.is_exercises.Add(exer);
                                justAdded.Add(exer.Name);
                            }
                        }
                        main26.SaveChanges();
                    }
                }
                catch (Exception ee)
                {
                    MessageBox.Show("Ошибка добавления упражнений\n" + ee);
                }
            }

            var additionalData = new AdditionalData();
            //additionalData.tbUrlName = 
            do
            {

                if (additionalData.ShowDialog() != DialogResult.OK)
                    return;

                if (string.IsNullOrWhiteSpace(additionalData.tbName.Text))
                {
                    MessageBox.Show("Введите заголовки страницы");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(additionalData.tbSectionId.Text))
                {
                    MessageBox.Show("Введите номер раздела");
                    continue;
                }
                long sectionId;
                if (!long.TryParse(additionalData.tbSectionId.Text, out sectionId))
                {
                    MessageBox.Show("Введите номер раздела");
                    continue;
                }
                string urlName = null;

                if (!string.IsNullOrWhiteSpace(additionalData.tbUrlName.Text))
                {
                    urlName = additionalData.tbUrlName.Text;
                }


                var titles = "";
                var nameRegEx =
                    new Regex(
                        "<word spelling=\"(?<Name>[^\"]+)\" pronounce=\"[^\"]+\" language=\"(?<LangId>\\w+)\" id=\"\\d+\"/>");


                var mm = nameRegEx.Matches(additionalData.tbName.Text);
                if (mm.Count == 0)
                {
                    titles = additionalData.tbName.Text;
                    if (urlName == null)
                    {
                        urlName = IsSilverlightUtils.TextUtils.UnicodeTranslit(titles, true, true);
                        additionalData.tbUrlName.Text = urlName;
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
                                    additionalData.tbUrlName.Text = urlName;
                                }
                                break;

                        }
                        //                    titles += string.Format("<title langId=\"{0}\" value=\"{1}\" />\n",
                        //                        langId, title);

                        titles += (!firstTitle ? "||" : "") + langId + "|" + title;
                        firstTitle = false;
                        //<title langId="ru" value="Заголовок" />
                    }

                }
                //                const string pg = "<page>";
                //                var pageInd = replacingResult.IndexOf(pg, System.StringComparison.Ordinal);
                //                if (pageInd < 0)
                //                {
                //                    MessageBox.Show(pg + " не найдено!");
                //                    return;
                //                }
                //                pageInd += pg.Length;
                //                replacingResult = replacingResult.Insert(pageInd, titles);
                //                MessageBox.Show(replacingResult);


                var xml = new XmlDocument();
                xml.LoadXml(replacingResult);

                var contentNodes = xml.SelectNodes("//content");
                if (contentNodes == null || contentNodes.Count == 0)
                {
                    MessageBox.Show("Ошибка в XML");
                    return;

                }

                try
                {

                    using (var main26 = new main26Entities1())
                    {
                        // Ищем максимальный ордер для страниц в разделе
                        var orderMax = (from p in main26.is_Pages
                            where p.SectionId == sectionId
                            select p).Max(po => (int?) po.Order);

                        // Ищем максимальный ордер для разделов-страниц в разделе
                        var sectionPagesOrderMax = (from s in main26.is_Sections
                            where s.ParentSection == sectionId && s.Type == 1
                            select s).Max(so => (int?) so.Order);

                        // выбираем из них максимальный
                        if (orderMax == null || sectionPagesOrderMax > orderMax)
                            orderMax = sectionPagesOrderMax;

                        var order = (orderMax ?? 0) + 10;



                        if (contentNodes.Count == 1) // Если внутри одна подстраница
                        {
                            replacingResult = replacingResult.Replace("encoding=\"utf-8\"", "");

                            var page = new is_Pages
                            {
                                Name = titles,
                                UrlName = urlName,
                                Order = order,
                                SectionId = sectionId,
                                Visible = 3,
                                PageData = replacingResult,
                                Template = "main"
                            };
                            main26.is_Pages.Add(page);
                        }
                        else // если больше одной подстраницы
                        {
                            var pageSection = new is_Sections
                            {
                                Name = titles,
                                UrlName = urlName,
                                Order = order,
                                ParentSection = sectionId,
                                Type = 1,
                                Visible = 3,
                                LastChange = DateTime.Now
                            };
                            main26.is_Sections.Add(pageSection);
                            main26.SaveChanges();
                            var porder = 0;
                            foreach (XmlElement contentNode in contentNodes)
                            {
                                var pageXml = "<page>" + contentNode.OuterXml + "</page>";


                                var pageTitle = contentNode.GetAttribute("title"); // Это скорей всего будут цифры так что преобразовывать нет смысла
                                string pageUrl;
                                if (porder == 0) // Если это первая страница, то url = main
                                    pageUrl = "main";
                                else
                                    pageUrl = pageTitle;
                                var page = new is_Pages
                                {
                                    Name = pageTitle,
                                    UrlName = pageUrl,
                                    Order = porder,
                                    SectionId = pageSection.Id,
                                    Visible = 3,
                                    PageData = pageXml,
                                    Template = "main"
                                };
                                porder += 10;
                                main26.is_Pages.Add(page);
                            }
                        }
                        main26.SaveChanges();
                    }
                }
                catch (Exception ee)
                {

                    MessageBox.Show("Ошибка добавления страницы. Проверьте данные.\n" + ee);
                    continue;
                }



                break;
            } while (true);


        }

        public static void ReplaceAllLanguagesToNormal()
        {
            try
            {
                using (var main26 = new main26Entities1())
                {
                    var pages = from p in main26.is_Pages
                                select p;

                    foreach (var isPagese in pages)
                    {
                        isPagese.PageData = isPagese.PageData.Replace("<lang id=\"RU\">", "<lang id=\"ru\">");
                        isPagese.PageData = isPagese.PageData.Replace("<lang id=\"CH\">", "<lang id=\"zh\">");
                        isPagese.PageData = isPagese.PageData.Replace("<lang id=\"GB\">", "<lang id=\"en\">");
                        isPagese.PageData = isPagese.PageData.Replace("<lang id=\"SP\">", "<lang id=\"es\">");
                        isPagese.PageData = isPagese.PageData.Replace("<lang id=\"DE\">", "<lang id=\"de\">");
                        isPagese.PageData = isPagese.PageData.Replace("<lang id=\"IT\">", "<lang id=\"it\">");
                        isPagese.PageData = isPagese.PageData.Replace("<lang id=\"HI\">", "<lang id=\"hi\">");
                    }


                    main26.SaveChanges();
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("Ошибка изменения страниц\n" + ee);
            }

        }
    }
}
