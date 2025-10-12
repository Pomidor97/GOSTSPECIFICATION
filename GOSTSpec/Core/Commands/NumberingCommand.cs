using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using GOSTSpec.Core.Services;
using System;

namespace GOSTSpec.Core.Commands
{
    /// <summary>
    /// Команда для автоматической нумерации позиций
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class NumberingCommand : IExternalCommand
    {
        private const string RequiredScheduleName = "В_Спецификация по ГОСТ_Позиция";

        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            var uiApp = commandData.Application;
            var uiDoc = uiApp.ActiveUIDocument;
            var document = uiDoc.Document;
            var activeView = document.ActiveView;

            // Проверка типа активного вида
            if (activeView.ViewType != ViewType.Schedule)
            {
                ShowWarning("Для автонумерации требуется открыть спецификацию " +
                           $"'{RequiredScheduleName}'");
                return Result.Cancelled;
            }

            // Проверка имени спецификации
            if (activeView.Name != RequiredScheduleName)
            {
                ShowWarning("Для автонумерации требуется открыть спецификацию " +
                           $"'{RequiredScheduleName}'");
                return Result.Cancelled;
            }

            var schedule = activeView as ViewSchedule;

            try
            {
                var service = new NumberingService();

                using (var transaction = new Transaction(document, "Автонумерация позиций"))
                {
                    transaction.Start();

                    try
                    {
                        var success = service.Execute(schedule);

                        if (success)
                        {
                            transaction.Commit();
                            TaskDialog.Show("Успех", 
                                "Автонумерация позиций завершена успешно!");
                            return Result.Succeeded;
                        }
                        else
                        {
                            transaction.RollBack();
                            ShowWarning("Не удалось выполнить нумерацию. " +
                                       "Проверьте содержимое спецификации.");
                            return Result.Failed;
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.RollBack();
                        message = $"Ошибка при нумерации: {ex.Message}";
                        return Result.Failed;
                    }
                }
            }
            catch (Exception ex)
            {
                message = $"Критическая ошибка: {ex.Message}";
                return Result.Failed;
            }
        }

        private void ShowWarning(string warningMessage)
        {
            TaskDialog.Show("Предупреждение", warningMessage);
        }
    }
}