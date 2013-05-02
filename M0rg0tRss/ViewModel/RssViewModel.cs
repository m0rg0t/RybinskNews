using GalaSoft.MvvmLight;
using M0rg0tRss.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Web.Syndication;

namespace M0rg0tRss.ViewModel
{
    public class RssViewModel : ViewModelBase
    {
        private ObservableCollection<RssDataGroup> _allGroups = new ObservableCollection<RssDataGroup>();
        public ObservableCollection<RssDataGroup> AllGroups
        {
            get
            {
                ObservableCollection<RssDataGroup> tempGroups = new ObservableCollection<RssDataGroup>();
                var sorted = (from groupitem in _allGroups
                              orderby groupitem.Order descending
                              select groupitem).ToList();
                foreach(var item in sorted) {
                    tempGroups.Add(item);
                };
                return tempGroups;
                //return _allGroups;
            }
            set
            {
                    _allGroups = value;
                    RaisePropertyChanged("AllGroups");
            }
        }

        public async Task<bool> AddGroupForFeedAsync(string feedUrl)
        {
            string clearedContent = String.Empty;

            if (GetGroup(feedUrl) != null) return false;

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

                        _allGroups.Add(group1);
                        RaisePropertyChanged("AllGroups");
                        //AllGroups = SortItems();
                    }
                    catch { };
                    break;
            };

            _allGroups.Add(feedGroup);
            RaisePropertyChanged("AllGroups");
            //AllGroups = SortItems();
            return true;
        }

        private ObservableCollection<RssDataGroup> SortItems()
        {
            ObservableCollection<RssDataGroup> tempGroups = new ObservableCollection<RssDataGroup>();
            var sorted = (from groupitem in _allGroups
                          orderby groupitem.Order descending
                          select groupitem).ToList();
            foreach (var item in sorted)
            {
                tempGroups.Add(item);
            };
            return tempGroups;
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

        public IEnumerable<RssDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");
            return AllGroups;
        }

        public RssDataGroup GetGroup(string uniqueId)
        {
            // Для небольших наборов данных можно использовать простой линейный поиск
            var matches = AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public RssDataItem GetItem(string uniqueId)
        {
            // Для небольших наборов данных можно использовать простой линейный поиск
            var matches = AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() > 0) return matches.First();
            return null;
        }

        public RssViewModel()
        {            
        }
    }
}
