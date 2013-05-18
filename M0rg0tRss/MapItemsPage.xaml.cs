using Bing.Maps;
using Callisto.Controls;
using M0rg0tRss.Controls;
using M0rg0tRss.Data;
using M0rg0tRss.DataModel;
using M0rg0tRss.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using WhereIsPolicemanWin8.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Шаблон элемента страницы сведений о группе задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234229

namespace M0rg0tRss
{
    /// <summary>
    /// Страница, на которой показываются общие сведения об отдельной группе, включая предварительный просмотр элементов
    /// внутри группы.
    /// </summary>
    public sealed partial class MapItemsPage : M0rg0tRss.Common.LayoutAwarePage
    {
        public MapItemsPage()
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
        //protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        //{
            // TODO: Присвоить this.DefaultViewModel["Group"] связываемую группу
            // TODO: Присвоить this.DefaultViewModel["Items"] коллекцию связываемых элементов
        //}

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


        /// <summary>
        /// Заполняет страницу содержимым, передаваемым в процессе навигации. Также предоставляется любое сохраненное состояние
        /// при повторном создании страницы из предыдущего сеанса.
        /// </summary>
        /// <param name="navigationParameter">Значение параметра, передаваемое
        /// <see cref="Frame.Navigate(Type, Object)"/> при первоначальном запросе этой страницы.
        /// </param>
        /// <param name="pageState">Словарь состояния, сохраненного данной страницей в ходе предыдущего
        /// сеанса. Это значение будет равно NULL при первом посещении страницы.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // TODO: Создание соответствующей модели данных для области проблемы, чтобы заменить пример данных
            var group = ViewModelLocator.MainStatic.GetGroup((String)navigationParameter);
            this.DefaultViewModel["Group"] = group;
            this.DefaultViewModel["Items"] = group.Items;

            ObservableCollection<RssDataItem> mapsdata = new ObservableCollection<RssDataItem>();
            mapsdata = ViewModelLocator.MainStatic.GetGroup("Tourist").Items;
            foreach (MapItem item in mapsdata)
            {
                Pushpin pushpin = new Pushpin();
                MapLayer.SetPosition(pushpin, item.Location);
                pushpin.Name = item.UniqueId;
                pushpin.Tapped += pushpinTapped;
                map.Children.Add(pushpin);
            };
        }

        Flyout box = new Flyout();

        private async void pushpinTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Pushpin tappedpin = sender as Pushpin;  // gets the pin that was tapped
            if (null == tappedpin) return;  // null check to prevent bad stuff if it wasn't a pin.
            ViewModelLocator.MainStatic.CurrentTouristItem = (MapItem)ViewModelLocator.MainStatic.GetGroup("Tourist").Items.FirstOrDefault(c => c.UniqueId.ToString() == tappedpin.Name.ToString());

            var x = MapLayer.GetPosition(tappedpin);

            box = new Flyout();
            box.Placement = PlacementMode.Top;
            box.Content = new TouristControl();
            box.PlacementTarget = sender as UIElement;
            box.IsOpen = true;
            //MessageDialog dialog = new MessageDialog("You are here " + x.Latitude + " " + x.Longitude);
            //await dialog.ShowAsync();
        }

        private TappedEventHandler ShowFlyoutData()
        {
            throw new NotImplementedException();
        }

        private void pushpin_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        /// <summary>
        /// Сохраняет состояние, связанное с данной страницей, в случае приостановки приложения или
        /// удаления страницы из кэша навигации. Значения должны соответствовать требованиям сериализации
        /// <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">Пустой словарь, заполняемый сериализуемым состоянием.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            box.IsOpen = false;
        }

        /// <summary>
        /// Вызывается при щелчке элемента.
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

        private void MapButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Frame.Navigate(typeof(MapPage));
            }
            catch { };
        }

        private void itemGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var itemId = ((RssDataItem)e.ClickedItem).UniqueId;
            this.Frame.Navigate(typeof(ItemDetailPage), itemId);
        }

    }
}
