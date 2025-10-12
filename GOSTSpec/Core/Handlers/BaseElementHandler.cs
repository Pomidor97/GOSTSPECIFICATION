using Autodesk.Revit.DB;
using GOSTSpec.Helpers;
using GOSTSpec.Constants;
using System;

namespace GOSTSpec.Core.Handlers
{
    /// <summary>
    /// Базовый класс для обработчиков элементов
    /// Содержит общую логику копирования стандартных параметров
    /// </summary>
    public abstract class BaseElementHandler : IElementHandler
    {
        protected readonly ParameterHelper _parameterHelper;
        protected readonly UnitConversionHelper _unitHelper;

        public abstract BuiltInCategory Category { get; }

        protected BaseElementHandler()
        {
            _parameterHelper = new ParameterHelper();
            _unitHelper = new UnitConversionHelper();
        }

        public abstract void ProcessElement(Element element, Document document, double reserveCoefficient);

        public virtual bool CanHandle(Element element)
        {
            if (element == null) return false;
            
            var category = element.Category;
            if (category == null) return false;

            return category.Id.IntegerValue == (int)Category;
        }

        /// <summary>
        /// Копирование стандартных параметров (марка, код, завод и т.д.)
        /// </summary>
        protected void CopyStandardParameters(Element element, Element elementType = null)
        {
            var sourceElement = elementType ?? element;

            // Марка
            TryCopyParameter(sourceElement, element, 
                ParameterNames.SourceMark, ParameterNames.TargetMark);

            // Код изделия
            TryCopyParameter(sourceElement, element, 
                ParameterNames.SourceCode, ParameterNames.TargetCode);

            // Завод-изготовитель
            TryCopyParameter(sourceElement, element, 
                ParameterNames.SourceManufacturer, ParameterNames.TargetManufacturer);

            // Единица измерения
            TryCopyParameter(sourceElement, element, 
                ParameterNames.SourceUnit, ParameterNames.TargetUnit);

            // Примечание
            TryCopyParameter(sourceElement, element, 
                ParameterNames.SourceNote, ParameterNames.TargetNote);

            // Масса
            TryCopyParameter(sourceElement, element, 
                ParameterNames.SourceMass, ParameterNames.TargetMass);
        }

        /// <summary>
        /// Безопасное копирование параметра
        /// </summary>
        protected void TryCopyParameter(Element source, Element target, 
            string sourceParamName, string targetParamName)
        {
            try
            {
                var sourceParam = _parameterHelper.GetParameter(source, sourceParamName);
                var targetParam = _parameterHelper.GetParameter(target, targetParamName);

                if (sourceParam?.HasValue == true && targetParam != null)
                {
                    if (sourceParam.StorageType == StorageType.String)
                    {
                        var value = sourceParam.AsString();
                        if (!string.IsNullOrEmpty(value))
                        {
                            targetParam.Set(value);
                        }
                    }
                    else if (sourceParam.StorageType == StorageType.Double)
                    {
                        targetParam.Set(sourceParam.AsDouble());
                    }
                    else if (sourceParam.StorageType == StorageType.Integer)
                    {
                        targetParam.Set(sourceParam.AsInteger());
                    }
                }
            }
            catch (Exception)
            {
                // Игнорируем ошибки копирования отдельных параметров
            }
        }

        /// <summary>
        /// Установка количества в целевой параметр
        /// </summary>
        protected void SetCount(Element element, double count)
        {
            try
            {
                var countParam = _parameterHelper.GetParameter(element, ParameterNames.TargetCount);
                countParam?.Set(count);
            }
            catch (Exception)
            {
                // Игнорируем ошибки
            }
        }

        /// <summary>
        /// Установка наименования в целевой параметр
        /// </summary>
        protected void SetName(Element element, string name)
        {
            try
            {
                var nameParam = _parameterHelper.GetParameter(element, ParameterNames.TargetName);
                nameParam?.Set(name);
            }
            catch (Exception)
            {
                // Игнорируем ошибки
            }
        }
    }
}