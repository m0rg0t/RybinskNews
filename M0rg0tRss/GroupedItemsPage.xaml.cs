using Callisto.Controls;
using M0rg0tRss.Controls;
using M0rg0tRss.Data;
using M0rg0tRss.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WhereIsPolicemanWin8.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Шаблон элемента страницы сгруппированных элементов задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234231

namespace M0rg0tRss
{
    /// <summary>
    /// Страница, на которой отображается сгруппированная коллекция элементов.
    /// </summary>
    public sealed partial class GroupedItemsPage : M0rg0tRss.Common.LayoutAwarePage
    {
        public GroupedItemsPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Заполняет страницу содержимым, передаваемым в процессе навигации. Также предоставляется любое сохраненное состояние
        /// при повторном создании страницы из предыдущего сеанса.
        /// </summary>
        /// <param name="navigationParameter">Значение параметра, передаваемое
        /// <see cref="Frame.Navigate(Type, Object)"/> при первоначальном запросе этой страницы.
        /// </param>
        /// <param name="pageState">Словарь состояния, сохраненного данной страницей в ходе предыдущего
        /// сеанса. Это значение будет равно NULL при первом посещении страницы.</param>
        protected async override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            //получаем папку с именем Data в локальной папке приложения
            var localFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync
               ("Data", CreationCollisionOption.OpenIfExists);

            //получаем список файлов в папке Data
            var cachedFeeds = await localFolder.GetFilesAsync();

            //получаем список всех файлов, имя которых config.xml
            var feedsToLoad = from feeds in cachedFeeds
                              where feeds.Name.EndsWith(".rss")
                              select feeds;

            //нам возращается IEnumrable - а он гарантирует тольок один проход
            //копируем в массив                
            var feedsEntries = feedsToLoad as StorageFile[] ?? feedsToLoad.ToArray();
            if (feedsEntries.Any())
            {
                /*foreach (var feed in feedsEntries)
                {
                    await ViewModelLocator.MainStatic.AddGroupForFeedAsync(feed);
                };*/
                await ViewModelLocator.MainStatic.LoadCacheRss(feedsEntries);
            };

            if (NetworkInformation.GetInternetConnectionProfile().GetNetworkConnectivityLevel() != 
              NetworkConnectivityLevel.InternetAccess)
            {
                if (feedsEntries.Any())
                {                   
                    /*foreach (var feed in feedsEntries)
                    {
                        await ViewModelLocator.MainStatic.AddGroupForFeedAsync(feed);
                    }*/
                    OfflineMode.Visibility = Visibility.Visible;
                }
                else
                {
                    var msg = new MessageDialog("Для работы приложения необходимо к интернет подключение.");
                    await msg.ShowAsync();
                }
            }
            else
            { 
                OfflineMode.Visibility = Visibility.Collapsed;
                //if (ViewModelLocator.MainStatic.AllGroups.Count() == 0)
                //{
                    ViewModelLocator.MainStatic.LoadRss();
                //};
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SettingsPane.GetForCurrentView().CommandsRequested -= Settings_CommandsRequested;
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SettingsPane.GetForCurrentView().CommandsRequested += Settings_CommandsRequested;
            base.OnNavigatedTo(e);
        }

        /*private void Settings_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            var viewPrivacyPage = new SettingsCommand("", "Privacy Statement", cmd =>
            {
                Launcher.LaunchUriAsync(new Uri("http://m0rg0t.com/?p=61", UriKind.Absolute));
            });
            args.Request.ApplicationCommands.Add(viewPrivacyPage);
        }*/

        /// <summary>
        /// Вызывается при нажатии заголовка группы.
        /// </summary>
        /// <param name="sender">Объект Button, используемый в качестве заголовка выбранной группы.</param>
        /// <param name="e">Данные о событии, описывающие, каким образом было инициировано нажатие.</param>
        void Header_Click(object sender, RoutedEventArgs e)
        {
            // Определение группы, представляемой экземпляром Button
            var group = (sender as FrameworkElement).DataContext;

            // Переход к соответствующей странице назначения и настройка новой страницы
            // путем передачи необходимой информации в виде параметра навигации
            if (((RssDataGroup)group).UniqueId == "MainNews")
            {
                try
                {
                    var itemId = ((RssDataGroup)group).Items.First().UniqueId;
                    this.Frame.Navigate(typeof(ItemDetailPage), itemId);
                }
                catch { };
            }
            else
            {
                this.Frame.Navigate(typeof(GroupDetailPage), ((RssDataGroup)group).UniqueId);
            };            
        }

        /// <summary>
        /// Вызывается при нажатии элемента внутри группы.
        /// </summary>
        /// <param name="sender">Объект GridView (или ListView, если приложение прикреплено),
        /// в котором отображается нажатый элемент.</param>
        /// <param name="e">Данные о событии, описывающие нажатый элемент.</param>
        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Переход к соответствующей странице назначения и настройка новой страницы
            // путем передачи необходимой информации в виде параметра навигации
            var itemId = ((RssDataItem)e.ClickedItem).UniqueId;
            this.Frame.Navigate(typeof(ItemDetailPage), itemId);
        }

        private async void MapButton_Click(object sender, RoutedEventArgs e)
        {
            //await WindowsMapsHelper.MapsHelper.SearchAsync("tourist", "Rybinsk, Yaroslavl', Russia", null);
            try
            {
                this.Frame.Navigate(typeof(MapPage));
            }
            catch { };
        }

        private async void WriteProblem1AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Popup anon = new Popup();
            anon.Child = new AnonimusWriteControl();
            anon.IsLightDismissEnabled = true;
            anon.IsOpen = true;
        }

        void Settings_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            try
            {
                var viewAboutPage = new SettingsCommand("", "Об авторе", cmd =>
                {
                    //(Window.Current.Content as Frame).Navigate(typeof(AboutPage));
                    var settingsFlyout = new SettingsFlyout();
                    settingsFlyout.Content = new About();
                    settingsFlyout.HeaderText = "Об авторе";

                    settingsFlyout.IsOpen = true;
                });
                args.Request.ApplicationCommands.Add(viewAboutPage);

                var viewAboutMalukahPage = new SettingsCommand("", "Политика конфиденциальности", cmd =>
                {
                    var settingsFlyout = new SettingsFlyout();
                    settingsFlyout.Content = new Privacy();
                    settingsFlyout.HeaderText = "Политика конфиденциальности";

                    settingsFlyout.IsOpen = true;
                });
                args.Request.ApplicationCommands.Add(viewAboutMalukahPage);
            }
            catch { };
        }



        public SettingsCommand viewStreetAndTownPage;

    }
}
