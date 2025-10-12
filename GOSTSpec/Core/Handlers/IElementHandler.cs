using Autodesk.Revit.DB;

namespace GOSTSpec.Core.Handlers
{
    /// <summary>
    /// Интерфейс для обработчиков различных типов элементов
    /// </summary>
    public interface IElementHandler
    {
        /// <summary>
        /// Категория элементов, которые обрабатывает handler
        /// </summary>
        BuiltInCategory Category { get; }

        /// <summary>
        /// Обработка элемента - копирование и расчет параметров
        /// </summary>
        /// <param name="element">Элемент для обработки</param>
        /// <param name="document">Документ Revit</param>
        /// <param name="reserveCoefficient">Коэффициент запаса</param>
        void ProcessElement(Element element, Document document, double reserveCoefficient);

        /// <summary>
        /// Проверка, может ли handler обработать данный элемент
        /// </summary>
        bool CanHandle(Element element);
    }
}