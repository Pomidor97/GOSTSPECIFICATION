using Autodesk.Revit.DB;

namespace GOSTSpec.Constants
{
    /// <summary>
    /// Константы категорий элементов Revit
    /// </summary>
    public static class CategoryConstants
    {
        // Трубопроводы
        public static readonly BuiltInCategory PipeFittings = (BuiltInCategory)(-2008055);
        public static readonly BuiltInCategory FlexPipes = (BuiltInCategory)(-2008050);
        public static readonly BuiltInCategory PipeInsulation = (BuiltInCategory)(-2008122);
        public static readonly BuiltInCategory MechanicalEquipment = (BuiltInCategory)(-2001140);
        public static readonly BuiltInCategory PlumbingFixtures = (BuiltInCategory)(-2001160);
        public static readonly BuiltInCategory PipeAccessories = (BuiltInCategory)(-2008049);
        public static readonly BuiltInCategory Sprinklers = (BuiltInCategory)(-2008099);
        public static readonly BuiltInCategory Pipes = (BuiltInCategory)(-2008044);

        // Вентиляция
        public static readonly BuiltInCategory DuctFittings = (BuiltInCategory)(-2008016);
        public static readonly BuiltInCategory Ducts = (BuiltInCategory)(-2008000);
        public static readonly BuiltInCategory AirTerminals = (BuiltInCategory)(-2008013);
        public static readonly BuiltInCategory DuctAccessories = (BuiltInCategory)(-2008010);
        public static readonly BuiltInCategory FlexDucts = (BuiltInCategory)(-2008020);
        public static readonly BuiltInCategory DuctInsulation = (BuiltInCategory)(-2008123);

        /// <summary>
        /// Получить все категории для обработки
        /// </summary>
        public static BuiltInCategory[] GetAllCategories()
        {
            return new[]
            {
                PipeFittings,
                FlexPipes,
                PipeInsulation,
                MechanicalEquipment,
                PlumbingFixtures,
                PipeAccessories,
                Sprinklers,
                Pipes,
                DuctFittings,
                Ducts,
                AirTerminals,
                DuctAccessories,
                FlexDucts,
                DuctInsulation
            };
        }
    }
}