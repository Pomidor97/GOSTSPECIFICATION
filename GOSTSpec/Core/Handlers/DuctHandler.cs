using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using GOSTSpec.Constants;
using System;
using GOSTSpec.Helpers;

namespace GOSTSpec.Core.Handlers
{
    /// <summary>
    /// Обработчик для воздуховодов
    /// </summary>
    public class DuctHandler : BaseElementHandler
    {
        public override BuiltInCategory Category => CategoryConstants.Ducts;

        public override void ProcessElement(Element element, Document document, double reserveCoefficient)
        {
            if (!(element is Duct duct))
                return;

            // Наименование с расчетом толщины
            ProcessName(duct);

            // Стандартные параметры
            CopyStandardParameters(duct);

            // Количество (длина с запасом)
            ProcessCount(duct, reserveCoefficient);
        }

        private void ProcessName(Element duct)
        {
            try
            {
                var sourceName = _parameterHelper.GetStringValue(duct, ParameterNames.SourceName);
                var ductSize = _parameterHelper.GetStringValue(duct, ParameterNames.Size);
                var typeName = _parameterHelper.GetStringValue(duct, ParameterNames.TypeName, typeParameter: true);

                var thickness = CalculateThickness(duct, typeName);
                var targetName = $"{sourceName}, {ductSize} δ={thickness} мм";

                SetName(duct, targetName);
            }
            catch (Exception)
            {
                // Игнорируем ошибки формирования наименования
            }
        }

        private double CalculateThickness(Element duct, string typeName)
        {
            var thickness = 0.5;
            var isMinThickness08 = typeName.ToLower().Contains("транзит") || 
                                   typeName.ToLower().Contains("огнезащитой");

            var diameterParam = duct.LookupParameter(ParameterNames.Diameter);

            // Круглый воздуховод
            if (diameterParam != null && diameterParam.HasValue)
            {
                var diameter = _unitHelper.ToMillimeters(diameterParam.AsDouble());
                
                if (diameter <= 200)
                    thickness = 0.5;
                else if (diameter <= 450)
                    thickness = 0.6;
                else if (diameter <= 800)
                    thickness = 0.7;
                else if (diameter <= 1250)
                    thickness = 1.0;
                else if (diameter <= 1600)
                    thickness = 1.2;
                else if (diameter <= 2000)
                    thickness = 1.4;
            }
            // Прямоугольный воздуховод
            else
            {
                var height = _unitHelper.ToMillimeters(_parameterHelper.GetDoubleValue(duct, ParameterNames.Height));
                var width = _unitHelper.ToMillimeters(_parameterHelper.GetDoubleValue(duct, ParameterNames.Width));
                var bigSize = Math.Max(height, width);

                if (bigSize <= 250)
                    thickness = 0.5;
                else if (bigSize <= 1000)
                    thickness = 0.7;
                else if (bigSize <= 2000)
                    thickness = 0.9;
                else
                    thickness = 1.2;
            }

            // Минимальная толщина 0.8 для транзитных и огнезащищенных
            if (isMinThickness08 && thickness < 0.8)
                thickness = 0.8;

            return thickness;
        }

        private void ProcessCount(Element duct, double reserveCoefficient)
        {
            try
            {
                var length = _parameterHelper.GetDoubleValue(duct, ParameterNames.Length);
                var lengthM = _unitHelper.ToMeters(length);

                SetCount(duct, lengthM * reserveCoefficient);
            }
            catch (Exception)
            {
                // Игнорируем ошибки расчета количества
            }
        }
    }
}