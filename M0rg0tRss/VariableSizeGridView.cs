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
    class VariableSizeGridView : GridView
    {
        private int rowVal;
        private int colVal;

        protected override void PrepareContainerForItemOverride(Windows.UI.Xaml.DependencyObject element, object item)
        {
            RssDataItem dataItem = item as RssDataItem;

            int group = -1;
            if (dataItem.Group.UniqueId.Contains("stas"))
            {
                group = 1;
            };

            int index = -1;

            if (dataItem != null)
            {
                index = dataItem.Group.Items.IndexOf(dataItem);

            }
            if (index == 1)
            {
                colVal = 2;
                rowVal = 4;
            }
            else
            {
                colVal = 2;
                rowVal = 2;
            }
            if (index == 2)
            {
                colVal = 2;
                rowVal = 4;
            }
            if (index == 5)
            {
                colVal = 4;
                rowVal = 4;
            };

            if (group > 0)
            {
                if (index == 2)
                {
                    colVal = 2;
                    rowVal = 4;
                }

                if (index == 5)
                {
                    colVal = 4;
                    rowVal = 4;
                }
            };

            VariableSizedWrapGrid.SetRowSpan(element as UIElement, rowVal);
            VariableSizedWrapGrid.SetColumnSpan(element as UIElement, colVal);

            base.PrepareContainerForItemOverride(element, item);
        }
    }
}
