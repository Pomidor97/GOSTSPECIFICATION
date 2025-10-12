using Autodesk.Revit.DB;

namespace GOSTSpec.Helpers
{
    /// <summary>
    /// Вспомогательный класс для работы с параметрами элементов
    /// </summary>
    public class ParameterHelper
    {
        /// <summary>
        /// Получить параметр элемента (экземпляра или типа)
        /// </summary>
        /// <param name="element">Элемент</param>
        /// <param name="parameterName">Имя параметра</param>
        /// <param name="typeParameter">Искать в параметрах типа</param>
        /// <returns>Параметр или null</returns>
        public Parameter GetParameter(Element element, string parameterName, bool typeParameter = false)
        {
            if (element == null || string.IsNullOrEmpty(parameterName))
                return null;

            // Получить параметр экземпляра
            var param = element.LookupParameter(parameterName);

            // Если не найден или запрошен параметр типа
            if (param == null || typeParameter)
            {
                var elementType = element.Document.GetElement(element.GetTypeId());
                if (elementType != null)
                {
                    return elementType.LookupParameter(parameterName);
                }
            }

            return param;
        }

        /// <summary>
        /// Безопасное получение строкового значения параметра
        /// </summary>
        public string GetStringValue(Element element, string parameterName, string defaultValue = "")
        {
            var param = GetParameter(element, parameterName);
            return param?.HasValue == true ? param.AsString() ?? defaultValue : defaultValue;
        }

        /// <summary>
        /// Безопасное получение числового значения параметра
        /// </summary>
        public double GetDoubleValue(Element element, string parameterName, double defaultValue = 0.0)
        {
            var param = GetParameter(element, parameterName);
            return param?.HasValue == true ? param.AsDouble() : defaultValue;
        }

        /// <summary>
        /// Безопасное получение целочисленного значения параметра
        /// </summary>
        public int GetIntValue(Element element, string parameterName, int defaultValue = 0)
        {
            var param = GetParameter(element, parameterName);
            return param?.HasValue == true ? param.AsInteger() : defaultValue;
        }

        /// <summary>
        /// Проверка существования параметра
        /// </summary>
        public bool HasParameter(Element element, string parameterName)
        {
            return GetParameter(element, parameterName) != null;
        }

        /// <summary>
        /// Установка строкового значения параметра
        /// </summary>
        public bool TrySetValue(Element element, string parameterName, string value)
        {
            try
            {
                var param = GetParameter(element, parameterName);
                if (param != null && !param.IsReadOnly)
                {
                    return param.Set(value);
                }
            }
            catch
            {
                // Игнорируем ошибки
            }
            return false;
        }

        /// <summary>
        /// Установка числового значения параметра
        /// </summary>
        public bool TrySetValue(Element element, string parameterName, double value)
        {
            try
            {
                var param = GetParameter(element, parameterName);
                if (param != null && !param.IsReadOnly)
                {
                    return param.Set(value);
                }
            }
            catch
            {
                // Игнорируем ошибки
            }
            return false;
        }
    }
}