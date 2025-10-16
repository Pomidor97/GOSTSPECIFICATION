using Autodesk.Revit.DB;
using GOSTSpec.Core.Handlers;
using GOSTSpec.Helpers;
using GOSTSpec.Constants;
using System;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;

namespace GOSTSpec.Core.Services
{
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

        public void Execute(Document document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            // ВРЕМЕННО: Диагностика (потом удалить)
            DiagnoseNestedFamilies(document);

            var reserveCoefficient = _elementHelper.GetGlobalParameter(document, ParameterNames.GlobalReserve, 1.0);

            // Копирование систем (ТОЛЬКО для ВК/ОВ, БЕЗ электрики и слаботочки)
            int nestedCopied = ProcessSystemNames(document);
            int revitSystemCopied = CopyRevitSystemNameToCustomParameter(document);
            int connectorCopied = CopySystemFromHostElements(document);

            // Обработка параметров элементов (ДЛЯ ВСЕХ категорий, включая электрику)
            foreach (var category in _handlerFactory.GetSupportedCategories())
            {
                ProcessCategory(document, category, reserveCoefficient);
            }

            // Финальное сообщение
            TaskDialog.Show("Успех",
                $"Копирование параметров завершено!\n\n" +
                $"Системы (ВК/ОВ):\n" +
                $"• Вложенные: {nestedCopied}\n" +
                $"• Из Revit (MEP): {revitSystemCopied}\n" +
                $"• Через коннекторы: {connectorCopied}\n\n" +
                $"Запас: {reserveCoefficient:F2}");
        }

        /// <summary>
        /// Копирование системы из вложенных семейств (только ВК/ОВ)
        /// </summary>
        private int ProcessSystemNames(Document document)
        {
            int copiedSystems = 0;

            // ✅ ТОЛЬКО категории ВК/ОВ (БЕЗ электрики и слаботочки)
            var mepCategories = new[]
            {
                // Трубопроводы
                CategoryConstants.Pipes,
                CategoryConstants.FlexPipes,
                CategoryConstants.PipeFittings,
                CategoryConstants.PipeAccessories,
                CategoryConstants.PipeInsulation,
                CategoryConstants.MechanicalEquipment,
                CategoryConstants.PlumbingFixtures,
                CategoryConstants.Sprinklers,
                
                // Вентиляция
                CategoryConstants.Ducts,
                CategoryConstants.FlexDucts,
                CategoryConstants.DuctFittings,
                CategoryConstants.DuctAccessories,
                CategoryConstants.DuctInsulation,
                CategoryConstants.AirTerminals
            };

            foreach (var category in mepCategories)
            {
                var elements = _elementHelper.GetElementsByCategory(document, category);

                foreach (var element in elements)
                {
                    if (element is FamilyInstance familyInstance)
                    {
                        try
                        {
                            var rootFamily = _elementHelper.GetRootFamily(familyInstance);

                            // Копируем систему только если это ВЛОЖЕННОЕ семейство
                            if (familyInstance.Id != rootFamily.Id)
                            {
                                var rootSystemName = _parameterHelper.GetStringValue(rootFamily, ParameterNames.System);
                                
                                if (!string.IsNullOrEmpty(rootSystemName))
                                {
                                    if (_parameterHelper.TrySetValue(familyInstance, ParameterNames.System, rootSystemName))
                                    {
                                        copiedSystems++;
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }
            }

            return copiedSystems;
        }

        /// <summary>
        /// Копирование из встроенного параметра "Имя системы классификации" (только трубы/воздуховоды)
        /// </summary>
        private int CopyRevitSystemNameToCustomParameter(Document document)
        {
            int copiedCount = 0;

            var categories = new[]
            {
                CategoryConstants.Pipes,
                CategoryConstants.FlexPipes,
                CategoryConstants.Ducts,
                CategoryConstants.FlexDucts
            };

            foreach (var category in categories)
            {
                var elements = _elementHelper.GetElementsByCategory(document, category);

                foreach (var element in elements)
                {
                    try
                    {
                        var customSystem = _parameterHelper.GetStringValue(element, ParameterNames.System);
                        
                        if (string.IsNullOrEmpty(customSystem))
                        {
                            var revitSystemName = _parameterHelper.GetStringValue(element, ParameterNames.RevitSystemName);
                            
                            if (!string.IsNullOrEmpty(revitSystemName))
                            {
                                if (_parameterHelper.TrySetValue(element, ParameterNames.System, revitSystemName))
                                {
                                    copiedCount++;
                                }
                            }
                        }
                    }
                    catch { }
                }
            }

            return copiedCount;
        }

        /// <summary>
        /// Копирование системы через коннекторы (ТОЛЬКО ВК/ОВ, БЕЗ электрики)
        /// </summary>
        private int CopySystemFromHostElements(Document document)
        {
            int copiedCount = 0;

            // ✅ ТОЛЬКО категории ВК/ОВ (БЕЗ электрики и слаботочки)
            var fittingCategories = new[]
            {
                // Трубопроводы
                CategoryConstants.PipeFittings,
                CategoryConstants.PipeAccessories,
                CategoryConstants.Sprinklers,
                CategoryConstants.PlumbingFixtures,
                CategoryConstants.MechanicalEquipment,
                
                // Вентиляция
                CategoryConstants.DuctFittings,
                CategoryConstants.DuctAccessories,
                CategoryConstants.AirTerminals
            };

            foreach (var category in fittingCategories)
            {
                var elements = _elementHelper.GetElementsByCategory(document, category);

                foreach (var element in elements)
                {
                    try
                    {
                        var currentSystem = _parameterHelper.GetStringValue(element, ParameterNames.System);
                        
                        if (string.IsNullOrEmpty(currentSystem))
                        {
                            var connectors = GetConnectors(element);
                            
                            if (connectors != null && connectors.Size > 0)
                            {
                                bool systemFound = false;
                                
                                foreach (Connector connector in connectors)
                                {
                                    if (connector.IsConnected)
                                    {
                                        foreach (Connector refConnector in connector.AllRefs)
                                        {
                                            var refElement = document.GetElement(refConnector.Owner.Id);
                                            
                                            // Пропускаем сам элемент
                                            if (refElement?.Id == element.Id)
                                                continue;
                                            
                                            // Проверяем, что это труба или воздуховод
                                            if (refElement is Pipe || refElement is Duct)
                                            {
                                                // Сначала пробуем получить из параметра "Имя системы классификации"
                                                var systemName = _parameterHelper.GetStringValue(refElement, ParameterNames.RevitSystemName);
                                                
                                                // Если не нашли, пробуем наш кастомный параметр
                                                if (string.IsNullOrEmpty(systemName))
                                                {
                                                    systemName = _parameterHelper.GetStringValue(refElement, ParameterNames.System);
                                                }
                                                
                                                // Если все еще пусто, пробуем через MEPSystem
                                                if (string.IsNullOrEmpty(systemName))
                                                {
                                                    if (refElement is Pipe pipe && pipe.MEPSystem != null)
                                                    {
                                                        systemName = pipe.MEPSystem.Name;
                                                    }
                                                    else if (refElement is Duct duct && duct.MEPSystem != null)
                                                    {
                                                        systemName = duct.MEPSystem.Name;
                                                    }
                                                }
                                                
                                                if (!string.IsNullOrEmpty(systemName))
                                                {
                                                    if (_parameterHelper.TrySetValue(element, ParameterNames.System, systemName))
                                                    {
                                                        copiedCount++;
                                                        systemFound = true;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        
                                        if (systemFound)
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    catch { }
                }
            }

            return copiedCount;
        }

        private ConnectorSet GetConnectors(Element element)
        {
            try
            {
                if (element is FamilyInstance familyInstance)
                {
                    var mepModel = familyInstance.MEPModel;
                    if (mepModel?.ConnectorManager != null)
                    {
                        return mepModel.ConnectorManager.Connectors;
                    }
                }
            }
            catch { }
            
            return null;
        }

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
                    if (IsSystemElement(element))
                        continue;

                    handler.ProcessElement(element, document, reserveCoefficient);
                }
                catch { }
            }
        }

        private bool IsSystemElement(Element element)
        {
            if (element is PipingSystem)
                return true;

            if (element is MechanicalSystem)
                return true;

            if (element is Autodesk.Revit.DB.Electrical.ElectricalSystem)
                return true;

            var categoryId = element.Category?.Id.IntegerValue;
            if (categoryId == -2008043 || 
                categoryId == -2008015 || 
                categoryId == -2001260)
            {
                return true;
            }

            return false;
        }
        
        /// <summary>
/// Диагностика вложенных семейств
/// </summary>
private void DiagnoseNestedFamilies(Document document)
{
    try
    {
        string info = "🔍 ДИАГНОСТИКА ВЛОЖЕННЫХ СЕМЕЙСТВ:\n\n";
        
        var testCategories = new[]
        {
            CategoryConstants.PipeFittings,
            CategoryConstants.PlumbingFixtures,
            CategoryConstants.MechanicalEquipment
        };
        
        int totalNested = 0;
        int withSystem = 0;
        
        foreach (var category in testCategories)
        {
            var elements = _elementHelper.GetElementsByCategory(document, category);
            
            foreach (var element in elements)
            {
                if (element is FamilyInstance familyInstance)
                {
                    var rootFamily = _elementHelper.GetRootFamily(familyInstance);
                    
                    if (familyInstance.Id != rootFamily.Id)
                    {
                        totalNested++;
                        
                        var rootSystemName = _parameterHelper.GetStringValue(rootFamily, ParameterNames.System);
                        var currentSystemName = _parameterHelper.GetStringValue(familyInstance, ParameterNames.System);
                        
                        if (!string.IsNullOrEmpty(rootSystemName))
                            withSystem++;
                        
                        if (totalNested <= 3) // Показываем первые 3
                        {
                            info += $"═══ ВЛОЖЕННОЕ #{totalNested} ═══\n";
                            info += $"Категория: {element.Category?.Name}\n";
                            info += $"Element ID: {familyInstance.Id}\n";
                            info += $"Root ID: {rootFamily.Id}\n";
                            info += $"Root система: '{rootSystemName ?? "ПУСТО"}'\n";
                            info += $"Current система: '{currentSystemName ?? "ПУСТО"}'\n\n";
                        }
                    }
                }
            }
        }
        
        info += $"═══ ИТОГО ═══\n";
        info += $"Всего вложенных: {totalNested}\n";
        info += $"С системой в root: {withSystem}\n";
        
        TaskDialog.Show("Диагностика вложенных", info);
    }
    catch (Exception ex)
    {
        TaskDialog.Show("Ошибка", ex.Message + "\n\n" + ex.StackTrace);
    }
}
    }
}