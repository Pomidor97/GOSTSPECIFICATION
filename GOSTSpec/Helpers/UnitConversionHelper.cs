using Autodesk.Revit.DB;

namespace GOSTSpec.Helpers
{
    /// <summary>
    /// Вспомогательный класс для конвертации единиц измерения
    /// </summary>
    public class UnitConversionHelper
    {
        /// <summary>
        /// Конвертировать из внутренних единиц Revit в миллиметры
        /// </summary>
        public double ToMillimeters(double internalValue)
        {
            return UnitUtils.ConvertFromInternalUnits(internalValue, UnitTypeId.Millimeters);
        }

        /// <summary>
        /// Конвертировать из внутренних единиц Revit в метры
        /// </summary>
        public double ToMeters(double internalValue)
        {
            return UnitUtils.ConvertFromInternalUnits(internalValue, UnitTypeId.Meters);
        }

        /// <summary>
        /// Конвертировать из внутренних единиц Revit в квадратные метры
        /// </summary>
        public double ToSquareMeters(double internalValue)
        {
            return UnitUtils.ConvertFromInternalUnits(internalValue, UnitTypeId.SquareMeters);
        }

        /// <summary>
        /// Конвертировать из миллиметров во внутренние единицы Revit
        /// </summary>
        public double FromMillimeters(double millimeters)
        {
            return UnitUtils.ConvertToInternalUnits(millimeters, UnitTypeId.Millimeters);
        }

        /// <summary>
        /// Конвертировать из метров во внутренние единицы Revit
        /// </summary>
        public double FromMeters(double meters)
        {
            return UnitUtils.ConvertToInternalUnits(meters, UnitTypeId.Meters);
        }
    }
}