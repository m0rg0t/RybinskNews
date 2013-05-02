using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Windows.Web.Syndication;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml.Navigation;
using Windows.System;
using System.Text.RegularExpressions;
using System.IO;
using M0rg0tRss.DataModel;
using GalaSoft.MvvmLight;

// Модель данных, определяемая этим файлом, служит типичным примером строго типизированной
// модели, которая поддерживает уведомление при добавлении, удалении или изменении членов. Выбранные
// имена свойств совпадают с привязками данных из стандартных шаблонов элементов.
//
// Приложения могут использовать эту модель в качестве начальной точки и добавлять к ней дополнительные элементы или полностью удалить и
// заменить ее другой моделью, соответствующей их потребностям.

namespace M0rg0tRss.Data
{
    /// <summary>
    /// Базовый класс объектов <see cref="RssDataItem"/> и <see cref="RssDataGroup"/>, который
    /// определяет свойства, общие для обоих объектов.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class RssDataCommon : M0rg0tRss.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public RssDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        public String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(RssDataCommon._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Универсальная модель данных элементов.
    /// </summary>
    public class RssDataItem : RssDataCommon
    {
        public RssDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, RssDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        public string OneLineContent
        {
            get { return this._content.Replace(System.Environment.NewLine, " "); }
            private set
            {
            }
        }

        private RssDataGroup _group;
        public RssDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// Универсальная модель данных групп.
    /// </summary>
    public class RssDataGroup : RssDataCommon
    {
        public RssDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            if (uniqueId.Contains("http://rybinsk.ru/news-2013?format=feed&type=atom"))
            {
                itemsCount = 12;
            };
            Items.CollectionChanged += ItemsCollectionChanged;
        }
        public int itemsCount = 6;

        private int _order = 0;
        public int Order
        {
            get
            {
                return _order;
            }
            set
            {
                if (_order!=value)
                {
                    _order = value;
                };
            }
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Предоставляет подмножество полной коллекции элементов, привязываемой из объекта GroupedItemsPage
            // по двум причинам: GridView не виртуализирует большие коллекции элементов и оно
            // улучшает работу пользователей при просмотре групп с большим количеством
            // элементов.
            //
            // Отображается максимальное число столбцов (12), поскольку это приводит к заполнению столбцов сетки
            // сколько строк отображается: 1, 2, 3, 4 или 6

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < itemsCount)
                    {
                        TopItems.Insert(e.NewStartingIndex,Items[e.NewStartingIndex]);
                        if (TopItems.Count > itemsCount)
                        {
                            TopItems.RemoveAt(itemsCount);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < itemsCount && e.NewStartingIndex < itemsCount)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < itemsCount)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[itemsCount-1]);
                    }
                    else if (e.NewStartingIndex < itemsCount)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(itemsCount);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < itemsCount)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= itemsCount)
                        {
                            TopItems.Add(Items[itemsCount-1]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < itemsCount)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < itemsCount)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<RssDataItem> _items = new ObservableCollection<RssDataItem>();
        public ObservableCollection<RssDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<RssDataItem> _topItem = new ObservableCollection<RssDataItem>();
        public ObservableCollection<RssDataItem> TopItems
        {
            get {return this._topItem; }
        }
    }

    /// <summary>
    /// Создает коллекцию групп и элементов с жестко заданным содержимым.
    /// 
    /// SampleDataSource инициализируется подстановочными данными, а не реальными рабочими
    /// данными, чтобы пример данных был доступен как во время разработки, так и во время выполнения.
    /// </summary>
    public sealed class RssDataSource: ViewModelBase
    {
        private static RssDataSource _sampleDataSource = new RssDataSource();

        private ObservableCollection<RssDataGroup> _allGroups = new ObservableCollection<RssDataGroup>();
        public ObservableCollection<RssDataGroup> AllGroups
        {
            get {
                /*ObservableCollection<RssDataGroup> tempGroups = new ObservableCollection<RssDataGroup>();
                var sorted = (from groupitem in _allGroups
                              orderby groupitem.Order descending
                              select groupitem).ToList();
                foreach(var item in sorted) {
                    tempGroups.Add(item);
                };
                return tempGroups;*/
                return _allGroups;
            }
            set
            {
                if (_allGroups != value)
                {
                    _allGroups = value;
                    RaisePropertyChanged("AllGroups");
                };
            }
        }

        public static async Task<bool> AddGroupForFeedAsync(string feedUrl)
        {
            string clearedContent = String.Empty;

            if (RssDataSource.GetGroup(feedUrl) != null) return false;

            var feed = await new SyndicationClient().RetrieveFeedAsync(new Uri(feedUrl));

            var feedGroup = new RssDataGroup(
                uniqueId: feedUrl,
                title: feed.Title != null ? feed.Title.Text : null,
                subtitle: feed.Subtitle != null ? feed.Subtitle.Text : null,
                imagePath: feed.ImageUri != null ? feed.ImageUri.ToString() : null,
                description: null);

            foreach (var i in feed.Items)
            {
                string imagePath = null;
                try
                {
                    imagePath = GetImageFromPostContents(i); ;
                }
                catch { };

                if (i.Summary != null)
                    clearedContent = Windows.Data.Html.HtmlUtilities.ConvertToText(i.Summary.Text);
                else
                    if (i.Content != null)
                        clearedContent = Windows.Data.Html.HtmlUtilities.ConvertToText(i.Content.Text);

                if (imagePath != null && feedGroup.Image == null)
                    feedGroup.SetImage(imagePath);

                if (imagePath == null) imagePath = "ms-appx:///Assets/DarkGray.png";

                try
                {
                    feedGroup.Items.Add(new RssDataItem(
                        uniqueId: i.Id, title: i.Title.Text, subtitle: null, imagePath: imagePath,
                        description: null, content: clearedContent, @group: feedGroup));
                }
                catch { };
            }

            switch (feedGroup.UniqueId)
            {
                case "http://rybinsk.ru/news-2013?format=feed&type=atom":
                    feedGroup.Order = 20;

                    try
                    {
                        var group1 = new RssDataGroup("MainNews", "Главная новость", "", "", "");
                        group1.Order = 30;
                        var tempitem = new RssDataItem(feedGroup.Items.First().UniqueId + "main",
                        feedGroup.Items.First().Title, null,
                        feedGroup.Items.First()._imagePath,
                        "",
                        feedGroup.Items.First().Content,
                        group1);

                        group1.Items.Add(tempitem);
                        group1.Items.Add(tempitem);
                        group1.Items.Add(tempitem);
                        group1.Items.Add(tempitem);
                        group1.Items.Add(tempitem);
                        group1.Items.Add(tempitem);

                        _sampleDataSource.AllGroups.Add(group1);
                        _sampleDataSource.RaisePropertyChanged("AllGroups");
                    }
                    catch { };
                    break;
            };

            _sampleDataSource.AllGroups.Add(feedGroup);
            _sampleDataSource.RaisePropertyChanged("AllGroups");
            return true;
        }

        private static string GetImageFromPostContents(SyndicationItem item)
        {
            string text2search = "";

            if (item.Content != null) text2search += item.Content.Text;
            if (item.Summary != null) text2search += item.Summary.Text;

            return Regex.Matches(text2search,
                    @"(?<=<img\s+[^>]*?src=(?<q>['""]))(?<url>.+?)(?=\k<q>)",
                    RegexOptions.IgnoreCase)
                .Cast<Match>()
                .Where(m =>
                {
                    Uri url;
                    if (Uri.TryCreate(m.Groups[0].Value, UriKind.Absolute, out url))
                    {
                        string ext = Path.GetExtension(url.AbsolutePath).ToLower();
                        if (ext == ".png" || ext == ".jpg" || ext == ".bmp") return true;
                    }
                    return false;
                })
                .Select(m => m.Groups[0].Value)
                .FirstOrDefault();
        }

        public static IEnumerable<RssDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");
            
            return _sampleDataSource.AllGroups;
        }

        public static RssDataGroup GetGroup(string uniqueId)
        {
            // Для небольших наборов данных можно использовать простой линейный поиск
            var matches = _sampleDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static RssDataItem GetItem(string uniqueId)
        {
            // Для небольших наборов данных можно использовать простой линейный поиск
            var matches = _sampleDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() > 0) return matches.First();
            return null;
        }

        public RssDataSource()
        {
            String ITEM_CONTENT = String.Format("Item Content: {0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}",
                        "Curabitur class aliquam vestibulum nam curae maecenas sed integer cras phasellus suspendisse quisque donec dis praesent accumsan bibendum pellentesque condimentum adipiscing etiam consequat vivamus dictumst aliquam duis convallis scelerisque est parturient ullamcorper aliquet fusce suspendisse nunc hac eleifend amet blandit facilisi condimentum commodo scelerisque faucibus aenean ullamcorper ante mauris dignissim consectetuer nullam lorem vestibulum habitant conubia elementum pellentesque morbi facilisis arcu sollicitudin diam cubilia aptent vestibulum auctor eget dapibus pellentesque inceptos leo egestas interdum nulla consectetuer suspendisse adipiscing pellentesque proin lobortis sollicitudin augue elit mus congue fermentum parturient fringilla euismod feugiat");

            /*var group1 = new RssDataGroup("Tourist",
        "Достопримечательности Рыбинска", "",
        "Assets/DarkGray.png",
        "");
            group1.Items.Add(new MapItem("s1",
                    "Соборная площадь и Спасо-Преображенский собор",
                    "",
                    "Assets/MediumGray.png",
                    "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                    ITEM_CONTENT,
                    group1));
            this.AllGroups.Add(group1);*/

            /*var group1 = new RssDataGroup("Group-1",
                    "Group Title: 1",
                    "Group Subtitle: 1",
                    "Assets/DarkGray.png",
                    "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            group1.Items.Add(new RssDataItem("Group-1-Item-1",
                    "Item Title: 1",
                    "Item Subtitle: 1",
                    "Assets/LightGray.png",
                    "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new RssDataItem("Group-1-Item-5",
                    "Item Title: 5",
                    "Item Subtitle: 5",
                    "Assets/MediumGray.png",
                    "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
                    ITEM_CONTENT,
                    group1));
            this.AllGroups.Add(group1);*/
        }
    }
}
