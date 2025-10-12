using Autodesk.Revit.DB;
using GOSTSpec.Helpers;
using GOSTSpec.Constants;
using System;
using System.Collections.Generic;

namespace GOSTSpec.Core.Services
{
    /// <summary>
    /// Сервис для автоматической нумерации позиций в спецификации
    /// </summary>
    public class NumberingService
    {
        private readonly RevitElementHelper _elementHelper;
        private readonly ParameterHelper _parameterHelper;

        public NumberingService()
        {
            _elementHelper = new RevitElementHelper();
            _parameterHelper = new ParameterHelper();
        }

        /// <summary>
        /// Выполнить нумерацию позиций
        /// </summary>
        /// <param name="schedule">Спецификация для нумерации</param>
        /// <returns>True если успешно, false если не удалось</returns>
        public bool Execute(ViewSchedule schedule)
        {
            if (schedule == null)
                return false;

            var document = schedule.Document;
            var orderDict = BuildOrderDictionary(schedule);

            if (orderDict.Count == 0)
                return false;

            var elements = _elementHelper.GetElementsFromSchedule(document, schedule);
            ApplyNumbering(elements, orderDict);

            return true;
        }

        /// <summary>
        /// Построить словарь порядковых номеров из спецификации
        /// </summary>
        private Dictionary<string, int> BuildOrderDictionary(ViewSchedule schedule)
        {
            var orderDict = new Dictionary<string, int>();
            var tableData = schedule.GetTableData();
            var tableSectionData = tableData.GetSectionData(SectionType.Body);

            if (tableSectionData.NumberOfRows == 0)
                return orderDict;

            var currentSystem = "ImpossibleSystemName";
            var position = 0;

            for (int rowIndex = 0; rowIndex < tableSectionData.NumberOfRows; rowIndex++)
            {
                var system = schedule.GetCellText(SectionType.Body, rowIndex, 0);
                var order = schedule.GetCellText(SectionType.Body, rowIndex, 1);
                var name = schedule.GetCellText(SectionType.Body, rowIndex, 3);
                var mark = schedule.GetCellText(SectionType.Body, rowIndex, 4);

                // Сброс счетчика при смене системы
                if (currentSystem != system)
                {
                    position = 0;
                    currentSystem = system;
                }

                var key = BuildKey(system, order, name, mark);
                orderDict[key] = position;

                position++;
            }

            return orderDict;
        }

        /// <summary>
        /// Применить нумерацию к элементам
        /// </summary>
        private void ApplyNumbering(IList<Element> elements, Dictionary<string, int> orderDict)
        {
            foreach (var element in elements)
            {
                try
                {
                    var order = _parameterHelper.GetStringValue(element, ParameterNames.Order);
                    var system = _parameterHelper.GetStringValue(element, ParameterNames.System);
                    var name = _parameterHelper.GetStringValue(element, ParameterNames.TargetName);
                    var mark = _parameterHelper.GetStringValue(element, ParameterNames.TargetMark);

                    var key = BuildKey(system, order, name, mark);

                    if (orderDict.TryGetValue(key, out var position))
                    {
                        _parameterHelper.TrySetValue(element, ParameterNames.Position, position.ToString());
                    }
                }
                catch
                {
                    // Игнорируем ошибки для отдельных элементов
                }
            }
        }

        /// <summary>
        /// Построить уникальный ключ для элемента
        /// </summary>
        private string BuildKey(string system, string order, string name, string mark)
        {
            return $"{system}{order}{name}{mark}";
        }
    }
}