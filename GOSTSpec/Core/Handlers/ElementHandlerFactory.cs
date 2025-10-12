using Autodesk.Revit.DB;
using GOSTSpec.Constants;
using System.Collections.Generic;

namespace GOSTSpec.Core.Handlers
{
    /// <summary>
    /// Фабрика для создания обработчиков элементов
    /// </summary>
    public class ElementHandlerFactory
    {
        private readonly Dictionary<BuiltInCategory, IElementHandler> _handlers;

        public ElementHandlerFactory()
        {
            _handlers = new Dictionary<BuiltInCategory, IElementHandler>
            {
                // Трубопроводы
                { CategoryConstants.Pipes, new PipeHandler() },
                { CategoryConstants.FlexPipes, new FlexPipeHandler() },
                { CategoryConstants.PipeInsulation, new PipeInsulationHandler() },
                { CategoryConstants.PipeFittings, new GeneralElementHandler(CategoryConstants.PipeFittings) },
                { CategoryConstants.PipeAccessories, new GeneralElementHandler(CategoryConstants.PipeAccessories) },
                { CategoryConstants.MechanicalEquipment, new GeneralElementHandler(CategoryConstants.MechanicalEquipment) },
                { CategoryConstants.PlumbingFixtures, new GeneralElementHandler(CategoryConstants.PlumbingFixtures) },
                { CategoryConstants.Sprinklers, new GeneralElementHandler(CategoryConstants.Sprinklers) },

                // Вентиляция
                { CategoryConstants.Ducts, new DuctHandler() },
                { CategoryConstants.FlexDucts, new FlexDuctHandler() },
                { CategoryConstants.DuctInsulation, new DuctInsulationHandler() },
                { CategoryConstants.DuctFittings, new GeneralElementHandler(CategoryConstants.DuctFittings) },
                { CategoryConstants.DuctAccessories, new GeneralElementHandler(CategoryConstants.DuctAccessories) },
                { CategoryConstants.AirTerminals, new GeneralElementHandler(CategoryConstants.AirTerminals) }
            };
        }

        /// <summary>
        /// Получить обработчик для категории
        /// </summary>
        public IElementHandler GetHandler(BuiltInCategory category)
        {
            return _handlers.TryGetValue(category, out var handler) ? handler : null;
        }

        /// <summary>
        /// Получить обработчик для элемента
        /// </summary>
        public IElementHandler GetHandler(Element element)
        {
            if (element?.Category == null)
                return null;

            var category = (BuiltInCategory)element.Category.Id.IntegerValue;
            return GetHandler(category);
        }

        /// <summary>
        /// Получить все доступные обработчики
        /// </summary>
        public IEnumerable<IElementHandler> GetAllHandlers()
        {
            return _handlers.Values;
        }

        /// <summary>
        /// Получить все категории, для которых есть обработчики
        /// </summary>
        public IEnumerable<BuiltInCategory> GetSupportedCategories()
        {
            return _handlers.Keys;
        }
    }
}