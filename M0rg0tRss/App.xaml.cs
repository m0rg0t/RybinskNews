using M0rg0tRss.Common;
using M0rg0tRss.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону "Приложение таблицы" см. по адресу http://go.microsoft.com/fwlink/?LinkId=234226

namespace M0rg0tRss
{
    /// <summary>
    /// Обеспечивает зависящее от конкретного приложения поведение, дополняющее класс Application по умолчанию.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Инициализирует одноэлементный объект приложения. Это первая строка разрабатываемого кода
        /// кода; поэтому она является логическим эквивалентом main() или WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        public static async Task<bool> CopyConfigToLocalFolder()
        {
            //получаем папку с именем Data в локальной папке приложения
            var localFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Data", CreationCollisionOption.OpenIfExists);

            //получаем список файлов в папке Data
            var files = await localFolder.GetFilesAsync();

            //получаем список всех файлов, имя которых config.xml
            var config = from file in files
                         where file.Name.Equals("config.xml")
                         select file;


            //нам возращается IEnumrable - а он гарантирует тольок один проход
            //копируем в массив - если не беспокоитесь об этом - просто уберите эту строчку
            //а в условии проверяйте config.Count()
            // if (config.Count() == 0) { }
            var configEntries = config as StorageFile[] ?? config.ToArray();

            //то же самое, что config.Count() == 0, но гарантиует от странных ошибок
            //т.е. в целом мы проверили, что файла config.xml нет в подпапке Data
            //папки локальных данных приложения
            if (!configEntries.Any())
            {
                //получаем папку Data из установленого приложения
                var dataFolder = await Package.Current.InstalledLocation.GetFolderAsync("Data");
                //получаем файл сonfig.xml
                var configFile = await dataFolder.GetFileAsync("config.xml");
                //копируем его в локальную папку данных
                await configFile.CopyAsync(localFolder);
                return true;
            }
            return false;
        }


        public static async Task<IEnumerable<Feed>> ReadSettings()
        {
            //получаем папку в которой находится наш файл конфигурации
            var localFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync
              ("Data", CreationCollisionOption.OpenIfExists);

            //получаем список файлов в папке Data
            var files = await localFolder.GetFilesAsync();

            //получаем список всех файлов, имя которых config.xml
            var config = from file in files
                         where file.Name.Equals("config.xml")
                         select file;


            //нам возращается IEnumrable - а он гарантирует тольок один проход
            //копируем в массив - если не беспокоитесь об этом - просто уберите эту строчку
            //а в условии проверяйте config.Count()
            // if (config.Count() == 0) { }
            var configEntries = config as StorageFile[] ?? config.ToArray();

            //то же самое, что config.Count() == 0, но гарантиует от странных ошибок
            //т.е. в целом мы проверили, что файла config.xml нет в подпапке Data
            //папки локальных данных приложения
            if (!configEntries.Any())
                await CopyConfigToLocalFolder();

            //получаем конфигурационный файл
            var configFile = await localFolder.GetFileAsync("config.xml");
            //считываем его как текст
            var configText = await FileIO.ReadTextAsync(configFile);
            //загружаем его как XML
            XElement configXML = XElement.Parse(configText);

            //разбираем XML инициализируя данным массив
            var feeds =
                from feed in configXML.Descendants("feed")
                select new Feed
                {
                    id = feed.Element("id").Value,
                    title = feed.Element("title").Value,
                    url = feed.Element("url").Value,
                    description = feed.Element("description").Value,
                    type = feed.Element("type").Value,
                    view = feed.Element("view").Value,
                    policy = feed.Element("policy").Value
                };

            //отдаем наружу массив с конфигурацией RSS потоков
            return feeds;
        }


        /// <summary>
        /// Вызывается при обычном запуске приложения пользователем.  Будут использоваться другие точки входа,
        /// если приложение запускается для открытия конкретного файла, отображения
        /// результатов поиска и т. д.
        /// </summary>
        /// <param name="args">Сведения о запросе и обработке запуска.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Не повторяйте инициализацию приложения, если в окне уже имеется содержимое,
            // только обеспечьте активность окна
            
            if (rootFrame == null)
            {
                // Создание фрейма, который станет контекстом навигации, и переход к первой странице
                rootFrame = new Frame();
                //Связывание фрейма с ключом SuspensionManager                                
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Восстановление сохраненного состояния сеанса только при необходимости
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        //Возникли ошибки при восстановлении состояния.
                        //Предполагаем, что состояние отсутствует, и продолжаем
                    }
                }

                // Размещение фрейма в текущем окне
                Window.Current.Content = rootFrame;
            }
            if (rootFrame.Content == null)
            {
                // Если стек навигации не восстанавливается для перехода к первой странице,
                // настройка новой страницы путем передачи необходимой информации в качестве параметра
                // навигации
                if (!rootFrame.Navigate(typeof(GroupedItemsPage), "AllGroups"))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // Обеспечение активности текущего окна
            Window.Current.Activate();
        }

        /// <summary>
        /// Вызывается при приостановке выполнения приложения. Состояние приложения сохраняется
        /// без учета информации о том, будет ли оно завершено или возобновлено с неизменным
        /// содержимым памяти.
        /// </summary>
        /// <param name="sender">Источник запроса приостановки.</param>
        /// <param name="e">Сведения о запросе приостановки.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }

        /// <summary>
        /// Вызывается при активации приложения для отображения результатов поиска.
        /// </summary>
        /// <param name="args">Сведения о запросе на активацию.</param>
        protected async override void OnSearchActivated(Windows.ApplicationModel.Activation.SearchActivatedEventArgs args)
        {
            // TODO: Регистрация события Windows.ApplicationModel.Search.SearchPane.GetForCurrentView().QuerySubmitted
            // в OnWindowCreated для ускорения поиска во время выполнения приложения

            // Если в окне еще не используется навигация по фреймам, вставьте собственный фрейм
            var previousContent = Window.Current.Content;
            var frame = previousContent as Frame;

            // Если приложение не содержит фрейм верхнего уровня, то, возможно, это
            // первый запуск приложения. Обычно этот метод и метод OnLaunched 
            // из файла App.xaml.cs могут вызывать общий метод.
            if (frame == null)
            {
                // Создание фрейма, играющего роль контекста навигации, и его связывание с
                // ключом SuspensionManager
                frame = new Frame();
                M0rg0tRss.Common.SuspensionManager.RegisterFrame(frame, "AppFrame");

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Восстановление сохраненного состояния сеанса только при необходимости
                    try
                    {
                        await M0rg0tRss.Common.SuspensionManager.RestoreAsync();
                    }
                    catch (M0rg0tRss.Common.SuspensionManagerException)
                    {
                        //Возникли ошибки при восстановлении состояния.
                        //Предполагаем, что состояние отсутствует, и продолжаем
                    }
                }
            }

            frame.Navigate(typeof(SearchResultsPage), args.QueryText);
            Window.Current.Content = frame;

            // Обеспечение активности текущего окна
            Window.Current.Activate();
        }
    }
}
