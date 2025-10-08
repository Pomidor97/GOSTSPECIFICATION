using Autodesk.Revit.DB;

namespace GOSTSpec.Constants
{
    /// <summary>
    /// Константы категории элементов Кумше
    /// </summary>
    public static class CategoryConstants
    {
        // Трубопроводы
        public static readonly BuiltInCategory PipeFittings = BuiltInCategory.OST_PipeFitting;
        public static readonly BuiltInCategory FlexPipes = BuiltInCategory.OST_FlexPipeCurves;
        public static readonly BuiltInCategory PipeInsulation = BuiltInCategory.OST_PipeInsulations;
        public static readonly BuiltInCategory MechanicalEquipment = BuiltInCategory.OST_MechanicalEquipment;
        public static readonly BuiltInCategory PlumbingFixtures = BuiltInCategory.OST_PlumbingFixtures;
        public static readonly BuiltInCategory PipeAccessories = BuiltInCategory.OST_PipeAccessory;
        public static readonly BuiltInCategory Sprinklers = BuiltInCategory.OST_Sprinklers;
        public static readonly BuiltInCategory Pipe = BuiltInCategory.OST_PipeCurves;
        
        
        //Вентиляция
        public static readonly BuiltInCategory DuctFittings = BuiltInCategory.OST_DuctFitting;
        public static readonly BuiltInCategory Ducts = BuiltInCategory.OST_DuctCurves;
        public static readonly BuiltInCategory AirTerminals = BuiltInCategory.OST_DuctTerminal; // Воздухораспределители
        public static readonly BuiltInCategory DuctAccessories  = BuiltInCategory.OST_DuctAccessory;
        public static readonly BuiltInCategory FlexDucts = BuiltInCategory.OST_FlexDuctCurves;
        public static readonly BuiltInCategory DuctInsulation = BuiltInCategory.OST_DuctInsulations;

        /// <summary>
        /// Получить все категории
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
                Pipe,
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