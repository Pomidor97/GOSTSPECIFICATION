using System;
using Autodesk.Revit.UI;


namespace GOSTSpec.Infrastructure
{
    /// <summary>
    /// Главный класс приложения Revit
    /// </summary>
    public class App:IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                var ribbonBuilder = new RibbonBuilder();
                ribbonBuilder.BuildRibbon(application);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                TaskDialog.Show("Ошибка запуска", 
                    $"Не удалось инициализировать плагин GOSTSpec:\n{e.Message}");
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}