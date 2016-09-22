using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleRegReplace
{
    /// <summary>
    /// Интерфейс для плагинов, которые выполняются после анализа 
    /// </summary>
    interface IAfterPlugin
    {
        /// <summary>
        /// Инициализация плагина 
        /// </summary>
        /// <returns>Если инициализация успешна, то true</returns>
        bool Init();

        /// <summary>
        /// Вызывается после сканирования всего документа
        /// Для выполнения последующих операций
        /// </summary>
        void AfrerReplaces(string replacingResult);
    }
}
