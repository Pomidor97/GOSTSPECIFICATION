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


            var reserveCoefficient = _elementHelper.GetGlobalParameter(document, ParameterNames.GlobalReserve, 1.0);

            // Копирование систем (ТОЛЬКО для ВК/ОВ, БЕЗ электрики и слаботочки)
            int nestedCopied = ProcessSystemNames(document);
            int revitSystemCopied = CopyRevitSystemNameToCustomParameter(document);
            int connectorCopied = CopySystemFromHostElements(document);

            // ✅ НОВОЕ: Копирование подгруппы для ЭЛ/СС
            int subgroupCopied = CopySubgroupForElectricalAndLowVoltage(document);

            // Обработка параметров элементов (ДЛЯ ВСЕХ категорий, включая электрику)
            foreach (var category in _handlerFactory.GetSupportedCategories())
            {
                ProcessCategory(document, category, reserveCoefficient);
            }

            // Финальное сообщение
            TaskDialog.Show("Успех",
                $"Копирование параметров завершено!\n\n" +
                $"ВК/ОВ (Имя системы → С_Система):\n" +
                $"• Вложенные: {nestedCopied}\n" +
                $"• Из Revit (MEP): {revitSystemCopied}\n" +
                $"• Через коннекторы: {connectorCopied}\n\n" +
                $"ЭЛ/СС (Подгруппа → С_Система): {subgroupCopied}\n\n" +
                $"Запас: {reserveCoefficient:F2}");
        }

        /// <summary>
        /// Копирование подгруппы для электрики и слаботочки
        /// Берем из встроенного параметра "Подгруппа" → записываем в "С_Система"
        /// Также обрабатывает вложенные семейства ЭЛ/СС
        /// </summary>
        private int CopySubgroupForElectricalAndLowVoltage(Document document)
        {
            int copiedCount = 0;

            // ✅ ТОЛЬКО категории ЭЛ/СС
            var electricalCategories = new[]
            {
                // Электрика
                CategoryConstants.CableTray,
                CategoryConstants.CableTrayFitting,
                CategoryConstants.Conduits,
                CategoryConstants.ConduitFittings,
                CategoryConstants.ElectricalFixtures,
                CategoryConstants.ElectricalEquipment,
                CategoryConstants.LightingFixtures,
                
                // Слаботочка
                CategoryConstants.DataDevices,
                CategoryConstants.TelephoneDevices,
                CategoryConstants.SecurityDevices,
                CategoryConstants.FireAlarmDevices,
                CategoryConstants.NurseCallDevices,
                CategoryConstants.CommunicationDevices
            };

            foreach (var category in electricalCategories)
            {
                var elements = _elementHelper.GetElementsByCategory(document, category);

                foreach (var element in elements)
                {
                    try
                    {
                        // Проверяем, не заполнен ли уже параметр С_Система
                        var existingSubgroup = _parameterHelper.GetStringValue(element, ParameterNames.System);
                        
                        if (string.IsNullOrEmpty(existingSubgroup))
                        {
                            string subgroupValue = null;
                            
                            // ═══ ОБРАБОТКА ВЛОЖЕННЫХ СЕМЕЙСТВ ═══
                            if (element is FamilyInstance familyInstance)
                            {
                                var rootFamily = _elementHelper.GetRootFamily(familyInstance);
                                
                                // Если это вложенное семейство - берем подгруппу из root
                                if (familyInstance.Id != rootFamily.Id)
                                {
                                    // Пробуем взять С_Система из root (там уже может быть подгруппа)
                                    subgroupValue = _parameterHelper.GetStringValue(rootFamily, ParameterNames.System);
                                    
                                    // Если в root нет С_Система, берем подгруппу из root
                                    if (string.IsNullOrEmpty(subgroupValue))
                                    {
                                        subgroupValue = GetSubgroupFromElement(rootFamily);
                                    }
                                    
                                    if (!string.IsNullOrEmpty(subgroupValue))
                                    {
                                        if (_parameterHelper.TrySetValue(familyInstance, ParameterNames.System, subgroupValue))
                                        {
                                            copiedCount++;
                                        }
                                        continue; // Переходим к следующему элементу
                                    }
                                }
                            }
                            
                            // ═══ ОБЫЧНЫЕ ЭЛЕМЕНТЫ (не вложенные) ═══
                            subgroupValue = GetSubgroupFromElement(element);
                            
                            // Если нашли подгруппу - записываем
                            if (!string.IsNullOrEmpty(subgroupValue))
                            {
                                if (_parameterHelper.TrySetValue(element, ParameterNames.System, subgroupValue))
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
        /// Получение подгруппы из элемента (несколько способов)
        /// </summary>
        private string GetSubgroupFromElement(Element element)
        {
            string subgroupValue = null;
            
            // Вариант 1: Прямой параметр "Подгруппа"
            subgroupValue = _parameterHelper.GetStringValue(element, "Подгруппа");
            
            // Вариант 2: Через LookupParameter
            if (string.IsNullOrEmpty(subgroupValue))
            {
                var subgroupParam = element.LookupParameter("Подгруппа");
                if (subgroupParam != null && subgroupParam.HasValue)
                {
                    subgroupValue = subgroupParam.AsString();
                    if (string.IsNullOrEmpty(subgroupValue))
                        subgroupValue = subgroupParam.AsValueString();
                }
            }
            
            // Вариант 3: Через встроенный параметр ALL_MODEL_FAMILY_NAME (Подгруппа)
            if (string.IsNullOrEmpty(subgroupValue))
            {
                var builtInParam = element.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME);
                if (builtInParam != null && builtInParam.HasValue)
                {
                    subgroupValue = builtInParam.AsString();
                    if (string.IsNullOrEmpty(subgroupValue))
                        subgroupValue = builtInParam.AsValueString();
                }
            }
            
            // Вариант 4: Для семейств - берем из типа
            if (string.IsNullOrEmpty(subgroupValue) && element is FamilyInstance familyInstance)
            {
                var familySymbol = familyInstance.Symbol;
                if (familySymbol != null)
                {
                    var familyNameParam = familySymbol.get_Parameter(BuiltInParameter.ALL_MODEL_FAMILY_NAME);
                    if (familyNameParam != null && familyNameParam.HasValue)
                    {
                        subgroupValue = familyNameParam.AsString();
                    }
                }
            }
            
            return subgroupValue;
        }

        /// <summary>
        /// Копирование системы из вложенных семейств (только ВК/ОВ)
        /// Для ЭЛ/СС вложенные обрабатываются отдельно в CopySubgroupForElectricalAndLowVoltage
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
                
                // ❌ ЭЛ/СС категории НЕ включены - для них своя логика
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
    }
}