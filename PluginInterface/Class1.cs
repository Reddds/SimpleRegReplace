using System.Text.RegularExpressions;

namespace PluginInterface
{
    public interface IRRPlugin
    {
        //        /// <summary>
        //        /// Загружает в плагин начальные значения
        //        /// </summary>
        //        /// <param name="settings"></param>
        //        /// <param name="updateSettings">Если плагин изменил начальные значения, 
        //        /// то сохранить в настройках то, что он вернул</param>
        //        /// <returns></returns>
        //        void InitSettings(string settings, out bool updateSettings);

        /// <summary>
        /// Инициализация плагина 
        /// </summary>
        /// <returns>Если инициализация успешна, то true</returns>
        bool Init();

        string MatchEvaluator(Match match);

        /// <summary>
        /// Вызывается после сканирования всего документа
        /// Для выполнения последующих операций
        /// </summary>
        void AfterReplaces(string replacingResult);
    }
}