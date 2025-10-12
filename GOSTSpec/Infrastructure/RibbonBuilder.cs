using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace GOSTSpec.Infrastructure
{
    /// <summary>
    /// Класс для создания пользовательской ленты в Revit
    /// </summary>
    public class RibbonBuilder
    {
        private const string TabName = "KAZGOR";
        private const string PanelName = "Оформление";

        private readonly string _assemblyPath;

        public RibbonBuilder()
        {
            _assemblyPath = Assembly.GetExecutingAssembly().Location;
        }

        /// <summary>
        /// Создать ленту с кнопками
        /// </summary>
        public void BuildRibbon(UIControlledApplication application)
        {
            // Создать вкладку
            CreateTab(application);

            // Создать панель
            var panel = CreatePanel(application);

            // Добавить кнопки
            AddButtons(panel);
        }

        /// <summary>
        /// Создать вкладку
        /// </summary>
        private void CreateTab(UIControlledApplication application)
        {
            try
            {
                application.CreateRibbonTab(TabName);
            }
            catch (Exception)
            {
                // Вкладка уже существует
            }
        }

        /// <summary>
        /// Создать или получить панель
        /// </summary>
        private RibbonPanel CreatePanel(UIControlledApplication application)
        {
            var panels = application.GetRibbonPanels(TabName);
            var panel = panels.Find(p => p.Name == PanelName);

            if (panel == null)
            {
                panel = application.CreateRibbonPanel(TabName, PanelName);
            }

            return panel;
        }

        /// <summary>
        /// Добавить кнопки на панель
        /// </summary>
        private void AddButtons(RibbonPanel panel)
        {
            // Кнопка копирования параметров
            var copyButton = CreatePushButton(
                panel,
                "copyParameterValues_btn",
                "Копирование\nзначение параметров",
                "GOSTSpec.Core.Commands.CopyParametersCommand",
                "Копирование значение параметров",
                "icons8_copy_32.png");

            // Кнопка автонумерации
            var numberingButton = CreatePushButton(
                panel,
                "autoNumbering_btn",
                "Автонумерация\nпозиций",
                "GOSTSpec.Core.Commands.NumberingCommand",
                "Автонумерация позиций",
                "icons8_counter_32.png");

            // Кнопка автоспецификации
            var scheduleButton = CreatePushButton(
                panel,
                "autoScheduling_btn",
                "Авто\nспецификация",
                "GOSTSpec.Core.Commands.AutoScheduleCommand",
                "Автоматическое создание спецификации",
                "icons8_schedule_32.png");
        }

        /// <summary>
        /// Создать кнопку
        /// </summary>
        private PushButton CreatePushButton(
            RibbonPanel panel,
            string buttonName,
            string buttonText,
            string className,
            string tooltip,
            string iconName)
        {
            var buttonData = new PushButtonData(
                buttonName,
                buttonText,
                _assemblyPath,
                className);

            var button = panel.AddItem(buttonData) as PushButton;

            if (button != null)
            {
                button.ToolTip = tooltip;
                button.LargeImage = LoadIcon(iconName);
            }

            return button;
        }

        /// <summary>
        /// Загрузить иконку из ресурсов
        /// </summary>
        private BitmapSource LoadIcon(string iconName)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = $"GOSTSpec.Resources.{iconName}";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        var decoder = new PngBitmapDecoder(
                            stream,
                            BitmapCreateOptions.PreservePixelFormat,
                            BitmapCacheOption.Default);

                        return decoder.Frames[0];
                    }
                }
            }
            catch (Exception)
            {
                // Возвращаем null, если не удалось загрузить иконку
            }

            return null;
        }
    }
}