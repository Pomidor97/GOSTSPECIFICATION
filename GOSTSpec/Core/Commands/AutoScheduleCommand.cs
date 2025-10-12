using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using GOSTSpec.Core.Services;
using System;
using System.Linq;

namespace GOSTSpec.Core.Commands
{
    /// <summary>
    /// Команда для автоматического создания спецификаций
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class AutoScheduleCommand : IExternalCommand
    {
        private const string TemplateScheduleName = "# Спецификация для оформления";
        private const string PositionScheduleName = "В_Спецификация по ГОСТ_Позиция";

        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            var uiApp = commandData.Application;
            var uiDoc = uiApp.ActiveUIDocument;
            var document = uiDoc.Document;

            try
            {
                // Поиск требуемых спецификаций
                var schedules = FindRequiredSchedules(document);

                if (schedules.templateSchedule == null || schedules.positionSchedule == null)
                {
                    ShowRequirementsWarning(
                        schedules.templateSchedule == null,
                        schedules.positionSchedule == null);
                    return Result.Cancelled;
                }

                var service = new ScheduleGenerationService();

                using (var transaction = new Transaction(document, "Создание спецификаций по системам"))
                {
                    transaction.Start();

                    try
                    {
                        var createdCount = service.Execute(
                            schedules.templateSchedule,
                            schedules.positionSchedule);

                        transaction.Commit();

                        TaskDialog.Show("Успех",
                            $"Создано спецификаций: {createdCount}");

                        return Result.Succeeded;
                    }
                    catch (Exception ex)
                    {
                        transaction.RollBack();
                        message = $"Ошибка при создании спецификаций: {ex.Message}";
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

        /// <summary>
        /// Найти требуемые спецификации в документе
        /// </summary>
        private (ViewSchedule templateSchedule, ViewSchedule positionSchedule) FindRequiredSchedules(Document document)
        {
            var collector = new FilteredElementCollector(document);
            var schedules = collector
                .OfClass(typeof(ViewSchedule))
                .Cast<ViewSchedule>()
                .ToList();

            var templateSchedule = schedules.FirstOrDefault(s => s.Name == TemplateScheduleName);
            var positionSchedule = schedules.FirstOrDefault(s => s.Name == PositionScheduleName);

            return (templateSchedule, positionSchedule);
        }

        /// <summary>
        /// Показать предупреждение о требованиях
        /// </summary>
        private void ShowRequirementsWarning(bool missingTemplate, bool missingPosition)
        {
            var warningText = "Для создания спецификаций необходимо:\n";

            if (missingPosition)
            {
                warningText += $"1. Открыть спецификацию '{PositionScheduleName}'\n";
            }

            if (missingTemplate)
            {
                warningText += $"2. Наличие шаблонной спецификации '{TemplateScheduleName}' в проекте";
            }

            TaskDialog.Show("Предупреждение", warningText);
        }
    }
}