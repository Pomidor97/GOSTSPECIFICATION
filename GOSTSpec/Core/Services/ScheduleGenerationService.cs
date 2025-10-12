using Autodesk.Revit.DB;
using GOSTSpec.Helpers;
using GOSTSpec.Constants;
using System;
using System.Collections.Generic;

namespace GOSTSpec.Core.Services
{
    /// <summary>
    /// Сервис для автоматического создания спецификаций по системам
    /// </summary>
    public class ScheduleGenerationService
    {
        private readonly RevitElementHelper _elementHelper;
        private readonly ParameterHelper _parameterHelper;

        public ScheduleGenerationService()
        {
            _elementHelper = new RevitElementHelper();
            _parameterHelper = new ParameterHelper();
        }

        /// <summary>
        /// Создать спецификации для всех систем
        /// </summary>
        /// <param name="templateSchedule">Шаблон спецификации</param>
        /// <param name="positionSchedule">Спецификация с позициями</param>
        /// <returns>Количество созданных спецификаций</returns>
        public int Execute(ViewSchedule templateSchedule, ViewSchedule positionSchedule)
        {
            if (templateSchedule == null || positionSchedule == null)
                return 0;

            var document = templateSchedule.Document;
            var systems = CollectSystemNames(positionSchedule);
            var createdCount = 0;

            foreach (var systemName in systems)
            {
                try
                {
                    CreateScheduleForSystem(document, templateSchedule, systemName);
                    createdCount++;
                }
                catch
                {
                    // Игнорируем ошибки создания отдельных спецификаций
                }
            }

            return createdCount;
        }

        /// <summary>
        /// Собрать уникальные имена систем из спецификации
        /// </summary>
        private HashSet<string> CollectSystemNames(ViewSchedule positionSchedule)
        {
            var systems = new HashSet<string>();
            var document = positionSchedule.Document;
            var elements = _elementHelper.GetElementsFromSchedule(document, positionSchedule);

            foreach (var element in elements)
            {
                try
                {
                    var systemName = _parameterHelper.GetStringValue(element, ParameterNames.System);
                    systemName = systemName?.Trim();

                    if (!string.IsNullOrEmpty(systemName))
                    {
                        systems.Add(systemName);
                    }
                }
                catch
                {
                    // Игнорируем ошибки для отдельных элементов
                }
            }

            return systems;
        }

        /// <summary>
        /// Создать спецификацию для конкретной системы
        /// </summary>
        private void CreateScheduleForSystem(Document document, ViewSchedule templateSchedule, string systemName)
        {
            // Дублировать шаблон
            var newSchedule = document.GetElement(
                templateSchedule.Duplicate(ViewDuplicateOption.Duplicate)) as ViewSchedule;

            if (newSchedule == null)
                return;

            // Установить параметр раздела проекта
            try
            {
                var projectSection = newSchedule.LookupParameter("KAZGOR_Раздел проекта");
                projectSection?.Set("Автоспецификация");
            }
            catch
            {
                // Игнорируем если параметр не найден
            }

            // Установить имя спецификации
            SetScheduleName(newSchedule, systemName);

            // Установить заголовок
            SetScheduleHeader(newSchedule, systemName);

            // Установить фильтр по системе
            SetSystemFilter(newSchedule, systemName);
        }

        /// <summary>
        /// Установить имя спецификации с обработкой дубликатов
        /// </summary>
        private void SetScheduleName(ViewSchedule schedule, string systemName)
        {
            var baseName = $"О_Спецификация_{systemName}";

            try
            {
                schedule.Name = baseName;
            }
            catch
            {
                // Если имя занято, добавляем " копия N"
                for (int copyNumber = 1; copyNumber < 10; copyNumber++)
                {
                    try
                    {
                        schedule.Name = $"{baseName} копия{copyNumber}";
                        break;
                    }
                    catch
                    {
                        // Продолжаем попытки
                    }
                }
            }
        }

        /// <summary>
        /// Установить заголовок спецификации
        /// </summary>
        private void SetScheduleHeader(ViewSchedule schedule, string systemName)
        {
            try
            {
                var tableData = schedule.GetTableData();
                var headerSection = tableData.GetSectionData(SectionType.Header);

                if (headerSection.NumberOfRows > 0)
                {
                    headerSection.SetCellText(0, 1, $"Система {systemName}");
                }
            }
            catch
            {
                // Игнорируем ошибки установки заголовка
            }
        }

        /// <summary>
        /// Установить фильтр по системе
        /// </summary>
        private void SetSystemFilter(ViewSchedule schedule, string systemName)
        {
            try
            {
                var definition = schedule.Definition;
                var filters = definition.GetFilters();

                // Предполагаем, что фильтр по системе второй (индекс 1)
                if (filters.Count > 1)
                {
                    var oldFilter = filters[1];
                    var newFilter = new ScheduleFilter(
                        oldFilter.FieldId,
                        oldFilter.FilterType,
                        systemName);

                    definition.RemoveFilter(1);
                    definition.AddFilter(newFilter);
                }
            }
            catch
            {
                // Игнорируем ошибки установки фильтра
            }
        }
    }
}