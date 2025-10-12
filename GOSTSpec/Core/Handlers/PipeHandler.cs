using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using GOSTSpec.Constants;
using System;
using GOSTSpec.Helpers;

namespace GOSTSpec.Core.Handlers
{
    /// <summary>
    /// Обработчик для труб
    /// </summary>
    public class PipeHandler : BaseElementHandler
    {
        public override BuiltInCategory Category => CategoryConstants.Pipes;

        public override void ProcessElement(Element element, Document document, double reserveCoefficient)
        {
            if (!(element is Pipe pipe))
                return;

            var pipeType = document.GetElement(pipe.GetTypeId());
            if (pipeType == null)
                return;

            // Наименование
            ProcessName(pipe, pipeType);

            // Стандартные параметры
            CopyStandardParameters(pipe, pipeType);

            // Количество (длина с запасом)
            ProcessCount(pipe, reserveCoefficient);
        }

        private void ProcessName(Element pipe, Element pipeType)
        {
            try
            {
                var sourceName = _parameterHelper.GetStringValue(pipeType, ParameterNames.SourceName);
                var pipeSize = _parameterHelper.GetStringValue(pipe, ParameterNames.Size);
                var pipeTypeValue = _parameterHelper.GetDoubleValue(pipe, ParameterNames.SourcePipeType);

                var outerDiameter = _parameterHelper.GetDoubleValue(pipe, ParameterNames.OuterDiameter);
                var innerDiameter = _parameterHelper.GetDoubleValue(pipe, ParameterNames.InnerDiameter);
                var thickness = _unitHelper.ToMillimeters((outerDiameter - innerDiameter) / 2);

                outerDiameter = _unitHelper.ToMillimeters(outerDiameter);
                innerDiameter = _unitHelper.ToMillimeters(innerDiameter);

                string targetName;

                switch ((int)pipeTypeValue)
                {
                    case 1:
                        targetName = $"{sourceName}, {pipeSize}х{thickness:0.#}";
                        break;
                    case 2:
                        targetName = $"{sourceName}, Ø{outerDiameter}х{thickness:0.#}";
                        break;
                    case 3:
                        targetName = $"{sourceName}, {pipeSize}";
                        break;
                    default:
                        targetName = "ТАКОЙ ТИП ТРУБЫ НЕ СУЩЕСТВУЕТ. ДОСТУПНЫЕ ТИПЫ 1-3";
                        break;
                }

                SetName(pipe, targetName);
            }
            catch (Exception)
            {
                // Игнорируем ошибки формирования наименования
            }
        }

        private void ProcessCount(Element pipe, double reserveCoefficient)
        {
            try
            {
                var length = _parameterHelper.GetDoubleValue(pipe, ParameterNames.Length);
                var lengthMm = _unitHelper.ToMillimeters(length);
                var lengthM = lengthMm / 1000.0;

                SetCount(pipe, lengthM * reserveCoefficient);
            }
            catch (Exception)
            {
                // Игнорируем ошибки расчета количества
            }
        }
    }
}