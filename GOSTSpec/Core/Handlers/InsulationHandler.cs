using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Mechanical;
using GOSTSpec.Constants;
using System;
using GOSTSpec.Helpers;

namespace GOSTSpec.Core.Handlers
{
    /// <summary>
    /// Обработчик для изоляции труб
    /// </summary>
    public class PipeInsulationHandler : BaseElementHandler
    {
        public override BuiltInCategory Category => CategoryConstants.PipeInsulation;

        public override void ProcessElement(Element element, Document document, double reserveCoefficient)
        {
            if (!(element is PipeInsulation insulation))
                return;

            // Проверка хоста
            var hostElement = document.GetElement(insulation.HostElementId);
            if (!(hostElement is Pipe))
            {
                SetName(insulation, ParameterNames.ExcludeValue);
                return;
            }

            var insulationType = document.GetElement(insulation.GetTypeId());

            // Наименование
            ProcessName(insulation, insulationType);

            // Стандартные параметры
            CopyStandardParameters(insulation, insulationType);

            // Количество
            ProcessCount(insulation, insulationType, reserveCoefficient);
        }

        private void ProcessName(Element insulation, Element insulationType)
        {
            try
            {
                var sourceName = _parameterHelper.GetStringValue(insulation, ParameterNames.SourceName);
                var pipeSize = _parameterHelper.GetStringValue(insulation, ParameterNames.PipeSize);
                var insulationTypeValue = _parameterHelper.GetStringValue(insulationType, ParameterNames.SourceInsulationType);
                
                var thickness = _parameterHelper.GetDoubleValue(insulation, ParameterNames.Thickness);
                var thicknessMm = _unitHelper.ToMillimeters(thickness);

                string targetName = sourceName;

                // Тип 1 - по длине
                if (insulationTypeValue.Contains("1"))
                {
                    targetName = $"{sourceName} {thicknessMm}мм, для труб {pipeSize}";
                }
                // Тип 2 - по площади/объему
                else if (insulationTypeValue.Contains("2"))
                {
                    var insulationSize = _parameterHelper.GetDoubleValue(insulation, ParameterNames.SourceInsulationSize);
                    targetName = $"{sourceName} {thicknessMm}мм, диаметром {insulationSize:0.#}мм";
                }

                SetName(insulation, targetName);
            }
            catch (Exception)
            {
                // Игнорируем ошибки
            }
        }

        private void ProcessCount(Element insulation, Element insulationType, double reserveCoefficient)
        {
            try
            {
                var insulationTypeValue = _parameterHelper.GetStringValue(insulationType, ParameterNames.SourceInsulationType);
                
                if (string.IsNullOrEmpty(insulationTypeValue))
                {
                    SetCount(insulation, -999999);
                    return;
                }

                var thickness = _parameterHelper.GetDoubleValue(insulation, ParameterNames.Thickness);
                var thicknessMm = _unitHelper.ToMillimeters(thickness);

                // Длина (Д)
                if (insulationTypeValue.Contains("Д"))
                {
                    var length = _parameterHelper.GetDoubleValue(insulation, ParameterNames.Length);
                    var lengthMm = _unitHelper.ToMillimeters(length);
                    SetCount(insulation, (lengthMm * reserveCoefficient) / 1000);
                }
                // Площадь (П)
                else if (insulationTypeValue.Contains("П"))
                {
                    var area = _parameterHelper.GetDoubleValue(insulation, ParameterNames.Area);
                    var areaSqM = _unitHelper.ToSquareMeters(area);
                    SetCount(insulation, areaSqM * reserveCoefficient);
                }
                // Объем (О)
                else if (insulationTypeValue.Contains("О"))
                {
                    var area = _parameterHelper.GetDoubleValue(insulation, ParameterNames.Area);
                    var areaSqM = _unitHelper.ToSquareMeters(area);
                    SetCount(insulation, reserveCoefficient * areaSqM * thicknessMm / 1000);
                }
            }
            catch (Exception)
            {
                // Игнорируем ошибки
            }
        }
    }

    /// <summary>
    /// Обработчик для изоляции воздуховодов
    /// </summary>
    public class DuctInsulationHandler : BaseElementHandler
    {
        public override BuiltInCategory Category => CategoryConstants.DuctInsulation;

        public override void ProcessElement(Element element, Document document, double reserveCoefficient)
        {
            if (!(element is DuctInsulation insulation))
                return;

            // Проверка хоста
            var hostElement = document.GetElement(insulation.HostElementId);
            if (!(hostElement is Duct))
            {
                SetName(insulation, ParameterNames.ExcludeValue);
                return;
            }

            // Наименование
            ProcessName(insulation);

            // Стандартные параметры
            CopyStandardParameters(insulation);

            // Количество
            ProcessCount(insulation, reserveCoefficient);
        }

        private void ProcessName(Element insulation)
        {
            try
            {
                var sourceName = _parameterHelper.GetStringValue(insulation, ParameterNames.SourceName);
                var thickness = _parameterHelper.GetDoubleValue(insulation, ParameterNames.Thickness);
                var thicknessMm = _unitHelper.ToMillimeters(thickness);

                var targetName = $"{sourceName} δ={thicknessMm}мм";
                SetName(insulation, targetName);
            }
            catch (Exception)
            {
                // Игнорируем ошибки
            }
        }

        private void ProcessCount(Element insulation, double reserveCoefficient)
        {
            try
            {
                var area = _parameterHelper.GetDoubleValue(insulation, ParameterNames.Area);
                var areaSqM = _unitHelper.ToSquareMeters(area);
                SetCount(insulation, areaSqM * reserveCoefficient);
            }
            catch (Exception)
            {
                // Игнорируем ошибки
            }
        }
    }
}