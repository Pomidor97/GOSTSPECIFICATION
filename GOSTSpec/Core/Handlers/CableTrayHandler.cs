using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using GOSTSpec.Constants;
using System;

namespace GOSTSpec.Core.Handlers
{
    /// <summary>
    /// Обработчик для кабельных лотков
    /// </summary>
    public class CableTrayHandler : BaseElementHandler
    {
        public override BuiltInCategory Category => CategoryConstants.CableTray;

        public override void ProcessElement(Element element, Document document, double reserveCoefficient)
        {
            if (!(element is CableTray cableTray))
                return;

            var cableTrayType = document.GetElement(cableTray.GetTypeId());
            if (cableTrayType == null)
                return;

            // Наименование
            ProcessName(cableTray, cableTrayType);

            // Стандартные параметры
            CopyStandardParameters(cableTray, cableTrayType);

            // Количество (длина с запасом)
            ProcessCount(cableTray, reserveCoefficient);
        }

        private void ProcessName(Element cableTray, Element cableTrayType)
        {
            try
            {
                var sourceName = _parameterHelper.GetStringValue(cableTrayType, ParameterNames.SourceName);
                var width = _parameterHelper.GetDoubleValue(cableTray, ParameterNames.Width);
                var height = _parameterHelper.GetDoubleValue(cableTray, ParameterNames.Height);

                if (width > 0 && height > 0)
                {
                    var widthMm = _unitHelper.ToMillimeters(width);
                    var heightMm = _unitHelper.ToMillimeters(height);
                    var targetName = $"{sourceName}, {widthMm:0}x{heightMm:0} мм";
                    SetName(cableTray, targetName);
                }
                else
                {
                    SetName(cableTray, sourceName);
                }
            }
            catch (Exception)
            {
                // Игнорируем ошибки формирования наименования
            }
        }

        private void ProcessCount(Element cableTray, double reserveCoefficient)
        {
            try
            {
                var length = _parameterHelper.GetDoubleValue(cableTray, ParameterNames.Length);
                var lengthM = _unitHelper.ToMeters(length);

                SetCount(cableTray, lengthM * reserveCoefficient);
            }
            catch (Exception)
            {
                // Игнорируем ошибки расчета количества
            }
        }
    }
}