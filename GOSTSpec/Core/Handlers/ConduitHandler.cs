using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using GOSTSpec.Constants;
using System;

namespace GOSTSpec.Core.Handlers
{
    /// <summary>
    /// Обработчик для коробов электропроводки
    /// </summary>
    public class ConduitHandler : BaseElementHandler
    {
        public override BuiltInCategory Category => CategoryConstants.Conduits;

        public override void ProcessElement(Element element, Document document, double reserveCoefficient)
        {
            if (!(element is Conduit conduit))
                return;

            var conduitType = document.GetElement(conduit.GetTypeId());
            if (conduitType == null)
                return;

            // Наименование
            ProcessName(conduit, conduitType);

            // Стандартные параметры
            CopyStandardParameters(conduit, conduitType);

            // Количество (длина с запасом)
            ProcessCount(conduit, reserveCoefficient);
        }

        private void ProcessName(Element conduit, Element conduitType)
        {
            try
            {
                var sourceName = _parameterHelper.GetStringValue(conduitType, ParameterNames.SourceName);
                var diameter = _parameterHelper.GetDoubleValue(conduit, ParameterNames.Diameter);

                if (diameter > 0)
                {
                    var diameterMm = _unitHelper.ToMillimeters(diameter);
                    var targetName = $"{sourceName}, Ø{diameterMm:0} мм";
                    SetName(conduit, targetName);
                }
                else
                {
                    SetName(conduit, sourceName);
                }
            }
            catch (Exception)
            {
                // Игнорируем ошибки формирования наименования
            }
        }

        private void ProcessCount(Element conduit, double reserveCoefficient)
        {
            try
            {
                var length = _parameterHelper.GetDoubleValue(conduit, ParameterNames.Length);
                var lengthM = _unitHelper.ToMeters(length);

                SetCount(conduit, lengthM * reserveCoefficient);
            }
            catch (Exception)
            {
                // Игнорируем ошибки расчета количества
            }
        }
    }
}