using Bing.Maps;
using M0rg0tRss.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M0rg0tRss.DataModel
{
    public class MapItem : RssDataItem
    {
        public MapItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, RssDataGroup group, double lat = 0, double lon = 0)
            : base(uniqueId, title, subtitle, imagePath, description, content, group)
        {
            this.Lat = lat;
            this.Lon = lon;
        }

        private double _lat;
        public double Lat
        {
            get
            {
                return _lat;
            }
            set
            {
                if (_lat!=value)
                {
                    _lat = value;
                };
            }
        }

        private double _lon;
        public double Lon
        {
            get
            {
                return _lon;
            }
            set
            {
                if (_lon != value)
                {
                    _lon = value;
                };
            }
        }

        public Bing.Maps.Location Location {
            private set { 

            }
            get {
                return new Location(this.Lat, this.Lon); ;
            }
        }
    }
}
