using Autodesk.Revit.DB;
using GOSTSpec.Core.Handlers;
using GOSTSpec.Helpers;
using GOSTSpec.Constants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GOSTSpec.Core.Services
{
    /// <summary>
    /// Сервис для копирования параметров элементов
    /// </summary>
    public class ParameterCopyService
    {
        private readonly ElementHandlerFactory _handlerFactory;
        private readonly RevitElementHelper _elementHelper;
        private readonly ParameterHelper _parameterHelper;

        public ParameterCopyService()
        {
            _handlerFactory = new ElementHandlerFactory();
            _elementHelper = new RevitElementHelper();
            _parameterHelper = new ParameterHelper();
        }

        /// <summary>
        /// Выполнить копирование параметров для всех элементов
        /// </summary>
        public void Execute(Document document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            // Получить коэффициент запаса
            var reserveCoefficient = _elementHelper.GetGlobalParameter(document, ParameterNames.GlobalReserve, 1.0);

            // Обработать системные имена для вложенных семейств
            ProcessSystemNames(document);

            // Обработать каждую категорию
            foreach (var category in _handlerFactory.GetSupportedCategories())
            {
                ProcessCategory(document, category, reserveCoefficient);
            }
        }

        /// <summary>
        /// Копирование имен систем для вложенных семейств
        /// </summary>
        private void ProcessSystemNames(Document document)
        {
            foreach (var category in _handlerFactory.GetSupportedCategories())
            {
                var elements = _elementHelper.GetElementsByCategory(document, category);

                foreach (var element in elements)
                {
                    if (element is FamilyInstance familyInstance)
                    {
                        var rootFamily = _elementHelper.GetRootFamily(familyInstance);

                        if (familyInstance.Id != rootFamily.Id)
                        {
                            try
                            {
                                var rootSystemName = _parameterHelper.GetStringValue(rootFamily, ParameterNames.System);
                                if (!string.IsNullOrEmpty(rootSystemName))
                                {
                                    _parameterHelper.TrySetValue(familyInstance, ParameterNames.System, rootSystemName);
                                }
                            }
                            catch
                            {
                                // Игнорируем ошибки
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Обработка элементов одной категории
        /// </summary>
        private void ProcessCategory(Document document, BuiltInCategory category, double reserveCoefficient)
        {
            var handler = _handlerFactory.GetHandler(category);
            if (handler == null)
                return;

            var elements = _elementHelper.GetElementsByCategory(document, category);

            foreach (var element in elements)
            {
                try
                {
                    handler.ProcessElement(element, document, reserveCoefficient);
                }
                catch
                {
                    // Продолжаем обработку даже при ошибках
                }
            }
        }
    }
}