using M0rg0tRss.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace M0rg0tRss
{
    class MyDataTemplateSelector : DataTemplateSelector
    {

        public DataTemplate Template1 { get; set; }
        public DataTemplate Template2 { get; set; }
        //NewsItemTemplate

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            RssDataItem dataItem = item as RssDataItem;

            if (dataItem.Group.UniqueId.Contains("http://rybinsk.ru/news-2013?format=feed&type=atom"))
            {
                return Template1;
            }
            else
            {
                return Template2;
            };
        }
    }
}
