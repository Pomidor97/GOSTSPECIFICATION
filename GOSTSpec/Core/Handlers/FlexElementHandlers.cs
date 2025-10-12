using Autodesk.Revit.DB;
using GOSTSpec.Constants;
using System;
using GOSTSpec.Helpers;

namespace GOSTSpec.Core.Handlers
{
    /// <summary>
    /// Обработчик для гибких труб
    /// </summary>
    public class FlexPipeHandler : BaseElementHandler
    {
        public override BuiltInCategory Category => CategoryConstants.FlexPipes;

        public override void ProcessElement(Element element, Document document, double reserveCoefficient)
        {
            var elementType = document.GetElement(element.GetTypeId());

            // Наименование (просто из типа)
            ProcessName(element, elementType);

            // Стандартные параметры
            CopyStandardParameters(element, elementType);

            // Количество = 1 (штучно)
            SetCount(element, 1);
        }

        private void ProcessName(Element element, Element elementType)
        {
            try
            {
                var sourceName = _parameterHelper.GetStringValue(elementType, ParameterNames.SourceName);
                SetName(element, sourceName);
            }
            catch (Exception)
            {
                // Игнорируем ошибки
            }
        }
    }

    /// <summary>
    /// Обработчик для гибких воздуховодов
    /// </summary>
    public class FlexDuctHandler : BaseElementHandler
    {
        public override BuiltInCategory Category => CategoryConstants.FlexDucts;

        public override void ProcessElement(Element element, Document document, double reserveCoefficient)
        {
            var elementType = document.GetElement(element.GetTypeId());

            // Наименование с диаметром
            ProcessName(element, elementType);

            // Стандартные параметры
            CopyStandardParameters(element, elementType);

            // Количество (длина с запасом)
            ProcessCount(element, reserveCoefficient);
        }

        private void ProcessName(Element element, Element elementType)
        {
            try
            {
                var sourceName = _parameterHelper.GetStringValue(elementType, ParameterNames.SourceName);
                var diameterParam = element.LookupParameter(ParameterNames.Diameter);

                if (diameterParam?.HasValue == true)
                {
                    var diameter = _unitHelper.ToMillimeters(diameterParam.AsDouble());
                    var targetName = $"{sourceName} Ø{Math.Round(diameter)}";
                    SetName(element, targetName);
                }
                else
                {
                    SetName(element, sourceName);
                }
            }
            catch (Exception)
            {
                // Игнорируем ошибки
            }
        }

        private void ProcessCount(Element element, double reserveCoefficient)
        {
            try
            {
                var length = _parameterHelper.GetDoubleValue(element, ParameterNames.Length);
                var lengthM = _unitHelper.ToMeters(length);
                SetCount(element, lengthM * reserveCoefficient);
            }
            catch (Exception)
            {
                // Игнорируем ошибки
            }
        }
    }
}
