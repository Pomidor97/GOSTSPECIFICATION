GOSTSpec/
├── Core/
│   ├── Commands/
│   │   ├── CopyParametersCommand.cs
│   │   ├── NumberingCommand.cs
│   │   └── AutoScheduleCommand.cs
│   │
│   ├── Services/
│   │   ├── ParameterCopyService.cs
│   │   ├── NumberingService.cs
│   │   └── ScheduleGenerationService.cs
│   │
│   └── Handlers/
│       ├── PipeHandler.cs
│       ├── DuctHandler.cs
│       ├── InsulationHandler.cs
│       ├── FittingHandler.cs
│       └── EquipmentHandler.cs
│
├── Models/
│   ├── ElementCategory.cs
│   ├── ParameterMapping.cs
│   └── SystemInfo.cs
│
├── Helpers/
│   ├── RevitElementHelper.cs
│   ├── ParameterHelper.cs
│   ├── UnitConversionHelper.cs
│   └── ValidationHelper.cs
│
├── Constants/
│   ├── CategoryConstants.cs
│   ├── ParameterNames.cs
│   └── ErrorMessages.cs
│
├── Infrastructure/
│   ├── App.cs
│   └── RibbonBuilder.cs
│
└── Resources/
└── Icons/

Основные принципы архитектуры
1. Separation of Concerns (Разделение ответственности)

Commands - только обработка команд Revit, валидация контекста
Services - бизнес-логика обработки элементов
Handlers - специфичная логика для каждого типа элементов
Helpers - утилиты общего назначения

2. Single Responsibility Principle

Каждый класс отвечает за одну задачу
PipeHandler - только трубы
DuctHandler - только воздуховоды
ParameterHelper - только работа с параметрами

3. Open/Closed Principle (Открыт для расширения)
   IElementHandler (interface)
   ├── PipeHandler
   ├── DuctHandler
   ├── InsulationHandler
   └── [Новый тип элемента] ← легко добавить
4. Dependency Injection готовность

Сервисы принимают зависимости через конструктор
Легко тестировать
Легко менять реализацию

🎯 Ключевые компоненты
Commands Layer

Минимальная логика
Получение контекста Revit (Document, UIDocument)
Валидация предусловий
Вызов соответствующего сервиса
Обработка результата и показ UI

Services Layer

Координация обработки
Управление транзакциями
Вызов нужных handlers
Агрегация результатов

Handlers Layer

Конкретная логика для типа элемента
Формирование наименований
Расчет количества
Копирование параметров

Helpers Layer

Работа с Revit API (получение элементов, параметров)
Конвертация единиц измерения
Поиск корневых семейств
Валидация данных

Constants

Все магические числа и строки в одном месте
Легко поддерживать
Избегаем дублирования

🔄 Паттерны для применения

Strategy Pattern - для разных типов элементов (Handler'ы)
Factory Pattern - для создания handler'ов по категории
Template Method - общий алгоритм копирования с переопределением деталей
Chain of Responsibility - обработка разных параметров последовательно

📦 Преимущества структуры
✅ Расширяемость - новый тип элемента = новый Handler
✅ Тестируемость - каждый компонент изолирован
✅ Читаемость - понятно где что искать
✅ Поддерживаемость - изменения локализованы
✅ Переиспользование - Helpers используются везде
✅ DRY - нет дублирования кода
Хочешь, чтобы я показал примеры интерфейсов и структуру классов для какого-то конкретного слоя?