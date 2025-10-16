using Autodesk.Revit.DB;

namespace GOSTSpec.Constants
{
    /// <summary>
    /// Константы категорий элементов Revit
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
        public static readonly BuiltInCategory Pipes = BuiltInCategory.OST_PipeCurves;

        // Вентиляция
        public static readonly BuiltInCategory DuctFittings = BuiltInCategory.OST_DuctFitting;
        public static readonly BuiltInCategory Ducts = BuiltInCategory.OST_DuctCurves;
        public static readonly BuiltInCategory AirTerminals = BuiltInCategory.OST_DuctTerminal;
        public static readonly BuiltInCategory DuctAccessories = BuiltInCategory.OST_DuctAccessory;
        public static readonly BuiltInCategory FlexDucts = BuiltInCategory.OST_FlexDuctCurves;
        public static readonly BuiltInCategory DuctInsulation = BuiltInCategory.OST_DuctInsulations;

        // ЭЛЕКТРИКА (Силовая)
        public static readonly BuiltInCategory ElectricalFixtures = BuiltInCategory.OST_ElectricalFixtures;
        public static readonly BuiltInCategory ElectricalEquipment = BuiltInCategory.OST_ElectricalEquipment;
        public static readonly BuiltInCategory LightingFixtures = BuiltInCategory.OST_LightingFixtures;
        
        // ✅ ПРАВИЛЬНЫЕ ID ДЛЯ КАБЕЛЬНЫХ СИСТЕМ
        public static readonly BuiltInCategory CableTray = BuiltInCategory.OST_CableTray;              // -2008132
        public static readonly BuiltInCategory CableTrayFitting = BuiltInCategory.OST_CableTrayFitting; // -2008133
        public static readonly BuiltInCategory Conduits = BuiltInCategory.OST_Conduit;                  // -2008100
        public static readonly BuiltInCategory ConduitFittings = BuiltInCategory.OST_ConduitFitting;    // -2008120

        // СЛАБОТОЧКА
        public static readonly BuiltInCategory DataDevices = BuiltInCategory.OST_DataDevices;
        public static readonly BuiltInCategory TelephoneDevices = BuiltInCategory.OST_TelephoneDevices;
        public static readonly BuiltInCategory SecurityDevices = BuiltInCategory.OST_SecurityDevices;
        public static readonly BuiltInCategory FireAlarmDevices = BuiltInCategory.OST_FireAlarmDevices;
        public static readonly BuiltInCategory NurseCallDevices = BuiltInCategory.OST_NurseCallDevices;
        public static readonly BuiltInCategory CommunicationDevices = BuiltInCategory.OST_CommunicationDevices;

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
                DuctInsulation,
                ElectricalFixtures, 
                ElectricalEquipment, 
                LightingFixtures,
                CableTray, 
                CableTrayFitting, 
                Conduits,
                ConduitFittings,
                DataDevices, 
                TelephoneDevices, 
                SecurityDevices, 
                FireAlarmDevices,
                NurseCallDevices, 
                CommunicationDevices
            };
        }

        public static BuiltInCategory[] GetElectricalCategories()
        {
            return new[]
            {
                ElectricalFixtures, 
                ElectricalEquipment, 
                LightingFixtures,
                CableTray, 
                CableTrayFitting, 
                Conduits, 
                ConduitFittings
            };
        }

        public static BuiltInCategory[] GetLowVoltageCategories()
        {
            return new[]
            {
                DataDevices, 
                TelephoneDevices, 
                SecurityDevices,
                FireAlarmDevices, 
                NurseCallDevices, 
                CommunicationDevices
            };
        }
    }
}