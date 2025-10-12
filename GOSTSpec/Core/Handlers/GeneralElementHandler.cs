using Autodesk.Revit.DB;
using GOSTSpec.Constants;
using System;
using GOSTSpec.Helpers;

namespace GOSTSpec.Core.Handlers
{
    /// <summary>
    /// Обработчик для общих элементов (арматура, оборудование, спринклеры и т.д.)
    /// </summary>
    public class GeneralElementHandler : BaseElementHandler
    {
        private readonly BuiltInCategory _category;

        public override BuiltInCategory Category => _category;

        public GeneralElementHandler(BuiltInCategory category)
        {
            _category = category;
        }

        public override void ProcessElement(Element element, Document document, double reserveCoefficient)
        {
            // Наименование
            ProcessName(element);

            // Стандартные параметры
            CopyStandardParameters(element);

            // Количество
            ProcessCount(element);
        }

        private void ProcessName(Element element)
        {
            try
            {
                var sourceName = _parameterHelper.GetStringValue(element, ParameterNames.SourceName);
                
                // Для соединительных деталей воздуховодов добавляем размер
                if (Category == CategoryConstants.DuctAccessories)
                {
                    var size = _parameterHelper.GetStringValue(element, ParameterNames.Size);
                    if (!string.IsNullOrEmpty(size))
                    {
                        sourceName = $"{sourceName}, {size}";
                    }
                }

                SetName(element, sourceName);
            }
            catch (Exception)
            {
                // Игнорируем ошибки
            }
        }

        private void ProcessCount(Element element)
        {
            try
            {
                // Проверяем специальный тип подсчета для сантехнических приборов
                if (Category == CategoryConstants.PlumbingFixtures)
                {
                    var countingType = _parameterHelper.GetStringValue(element, ParameterNames.CountingType, typeParameter: true);
                    
                    if (!string.IsNullOrEmpty(countingType) && 
                        countingType.Trim().Equals("Д", StringComparison.OrdinalIgnoreCase))
                    {
                        var length = _parameterHelper.GetDoubleValue(element, ParameterNames.SourceLength);
                        if (length > 0)
                        {
                            var lengthMm = _unitHelper.ToMillimeters(length);
                            SetCount(element, lengthMm / 1000); // в метры
                            return;
                        }
                    }
                }

                // По умолчанию количество = 1
                SetCount(element, 1);
            }
            catch (Exception)
            {
                // Игнорируем ошибки
            }
        }
    }
}