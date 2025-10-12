using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using GOSTSpec.Core.Services;
using System;

namespace GOSTSpec.Core.Commands
{
    /// <summary>
    /// Команда для копирования параметров элементов
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class CopyParametersCommand : IExternalCommand
    {
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
                var service = new ParameterCopyService();

                using (var transaction = new Transaction(document, "Копирование параметров"))
                {
                    transaction.Start();

                    try
                    {
                        service.Execute(document);
                        transaction.Commit();

                        TaskDialog.Show("Успех", 
                            "Копирование параметров завершено успешно!");

                        return Result.Succeeded;
                    }
                    catch (Exception ex)
                    {
                        transaction.RollBack();
                        message = $"Ошибка при копировании параметров: {ex.Message}";
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
    }
}