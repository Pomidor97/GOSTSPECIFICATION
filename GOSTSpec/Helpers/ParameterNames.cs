namespace GOSTSpec.Constants
{
    /// <summary>
    /// Имена параметров для чтения и записи
    /// </summary>
    public static class ParameterNames
    {
        // Системные параметры
        public const string System = "С_Система";
        public const string RevitSystemName = "Имя системы";
        public const string Order = "С_Сортировка";
        public const string Position = "С_Позиция";

        // Исходные параметры (источник)
        public const string SourceName = "KAZGOR_Наименование";
        public const string SourceMark = "KAZGOR_Марка";
        public const string SourceMass = "KAZGOR_Масса";
        public const string SourceCode = "KAZGOR_Код изделия";
        public const string SourceInsulationType = "Тип изоляции";
        public const string SourcePipeType = "Тип трубопровода";
        public const string SourceManufacturer = "KAZGOR_Завод-изготовитель";
        public const string SourceUnit = "KAZGOR_Единица измерения";
        public const string SourceNote = "KAZGOR_Примечание";
        public const string SourceCircSize = "KAZGOR_Размер_Диаметр";
        public const string SourceHeight = "KAZGOR_Размер_Высота";
        public const string SourceWidth = "KAZGOR_Размер_Ширина";
        public const string SourceInsulationSize = "KAZGOR_Диаметр изоляции";
        public const string SourceLength = "KAZGOR_Размер_Длина";

        // Целевые параметры (назначение)
        public const string TargetName = "С_Наименование";
        public const string TargetMark = "С_Марка";
        public const string TargetMass = "С_Масса";
        public const string TargetCode = "С_Код изделия";
        public const string TargetManufacturer = "С_Завод-изготовитель";
        public const string TargetUnit = "С_Единица измерения";
        public const string TargetNote = "С_Примечание";
        public const string TargetCount = "С_Количество";

        // Встроенные параметры Revit
        public const string Length = "Длина";
        public const string Area = "Площадь";
        public const string Size = "Размер";
        public const string PipeSize = "Размер трубы";
        public const string DuctSize = "Размер воздуховода";
        public const string Diameter = "Диаметр";
        public const string Height = "Высота";
        public const string Width = "Ширина";
        public const string Thickness = "Толщина изоляции";
        public const string OuterDiameter = "Внешний диаметр";
        public const string InnerDiameter = "Внутренний диаметр";
        public const string TypeName = "Имя типа";
        public const string CountingType = "Тип_подсчета";

        // Глобальные параметры
        public const string GlobalReserve = "Запас";

        // Специальные значения
        public const string ExcludeValue = "Исключить";
        public const string ErrorValue = "-999999";
    }
}