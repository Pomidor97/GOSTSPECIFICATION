using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;

namespace GOSTSpec.Helpers
{
    /// <summary>
    /// Вспомогательный класс для работы с элементами Revit
    /// </summary>
    public class RevitElementHelper
    {
        /// <summary>
        /// Получить все элементы заданной категории
        /// </summary>
        public IList<Element> GetElementsByCategory(Document document, BuiltInCategory category)
        {
            var collector = new FilteredElementCollector(document);
            return collector
                .OfCategory(category)
                .WhereElementIsNotElementType()
                .ToElements();
        }

        /// <summary>
        /// Получить корневое семейство для вложенного элемента
        /// </summary>
        public FamilyInstance GetRootFamily(FamilyInstance familyInstance)
        {
            if (familyInstance == null)
                return null;

            var superComponent = familyInstance.SuperComponent as FamilyInstance;
            
            if (superComponent != null)
            {
                return GetRootFamily(superComponent);
            }

            return familyInstance;
        }

        /// <summary>
        /// Получить глобальный параметр по имени
        /// </summary>
        public double GetGlobalParameter(Document document, string parameterName, double defaultValue = 1.0)
        {
            var globalParameter = new FilteredElementCollector(document)
                .OfClass(typeof(GlobalParameter))
                .Cast<GlobalParameter>()
                .FirstOrDefault(gp => gp.GetDefinition().Name.Equals(parameterName));

            if (globalParameter != null && globalParameter.GetValue() is DoubleParameterValue doubleValue)
            {
                return doubleValue.Value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Проверка, является ли элемент трубой
        /// </summary>
        public bool IsPipe(Element element)
        {
            return element is Pipe;
        }

        /// <summary>
        /// Проверка, является ли элемент воздуховодом
        /// </summary>
        public bool IsDuct(Element element)
        {
            return element is Duct;
        }

        /// <summary>
        /// Получить тип элемента
        /// </summary>
        public ElementType GetElementType(Element element)
        {
            if (element?.Document == null)
                return null;

            return element.Document.GetElement(element.GetTypeId()) as ElementType;
        }

        /// <summary>
        /// Получить элементы из спецификации
        /// </summary>
        public IList<Element> GetElementsFromSchedule(Document document, ViewSchedule schedule)
        {
            if (schedule == null)
                return new List<Element>();

            var collector = new FilteredElementCollector(document, schedule.Id);
            return collector.WhereElementIsNotElementType().ToElements();
        }
    }
}