using Autodesk.Revit.DB;
using GOSTSpec.Constants;
using System;

namespace GOSTSpec.Core.Handlers
{
    /// <summary>
    /// Обработчик для осветительных приборов
    /// </summary>
    public class LightingFixtureHandler : BaseElementHandler
    {
        public override BuiltInCategory Category => CategoryConstants.LightingFixtures;

        public override void ProcessElement(Element element, Document document, double reserveCoefficient)
        {
            var elementType = document.GetElement(element.GetTypeId());

            // Наименование
            ProcessName(element, elementType);

            // Стандартные параметры
            CopyStandardParameters(element, elementType);

            // Количество (штучно)
            SetCount(element, 1);
        }

        private void ProcessName(Element element, Element elementType)
        {
            try
            {
                var sourceName = _parameterHelper.GetStringValue(element, ParameterNames.SourceName);
                SetName(element, sourceName);
            }
            catch (Exception)
            {
                // Игнорируем ошибки
            }
        }
    }
}