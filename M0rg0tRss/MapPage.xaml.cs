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
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Основная страница" см. по адресу http://go.microsoft.com/fwlink/?LinkId=234237

namespace M0rg0tRss
{
    /// <summary>
    /// Основная страница, которая обеспечивает характеристики, являющимися общими для большинства приложений.
    /// </summary>
    public sealed partial class MapPage : M0rg0tRss.Common.LayoutAwarePage
    {
        public MapPage()
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
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
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

        private void pageTitle_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }
    }
}
