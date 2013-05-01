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

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            RssDataItem dataItem = item as RssDataItem;

            if (dataItem.Group.UniqueId.Contains("Новости Рыбинска за 2013 год"))
            {
                return Template1;
            }
            else
                return Template2;
        }
    }
}
