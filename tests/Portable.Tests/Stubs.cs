// Test-only in-memory WinRT/MVVM surfaces; no network, UI or external data.
using System;using System.Collections.Generic;using System.Threading.Tasks;
namespace Windows.ApplicationModel.Resources.Core {} namespace Windows.Foundation {} namespace Windows.Foundation.Collections {} namespace Windows.UI.Xaml.Data {} namespace Windows.UI.ApplicationSettings {} namespace Windows.UI.Xaml.Navigation {} namespace Windows.System {}
namespace Windows.Foundation.Metadata { public class WebHostHiddenAttribute:Attribute{} }
namespace Windows.UI.Xaml.Media {public class ImageSource{}}
namespace Windows.UI.Xaml.Media.Imaging {public class BitmapImage:Windows.UI.Xaml.Media.ImageSource {public BitmapImage(Uri uri){} }}
namespace M0rg0tRss.DataModel {}
namespace M0rg0tRss.Common {public class BindableBase {protected void SetProperty<T>(ref T field,T value){field=value;}protected void OnPropertyChanged(string name){}}}
namespace GalaSoft.MvvmLight {public class ViewModelBase {protected void RaisePropertyChanged(string name){}}}
namespace Windows.Data.Html {public static class HtmlUtilities {public static string ConvertToText(string text){return text;}}}
namespace Windows.Web.Syndication {
 public class SyndicationText {public string Text;public SyndicationText(string text){Text=text;}}
 public class SyndicationItem {public string Id;public SyndicationText Title,Summary,Content;}
 public class SyndicationFeed {public SyndicationText Title,Subtitle;public Uri ImageUri;public List<SyndicationItem> Items=new List<SyndicationItem>();}
 public class SyndicationClient {public static Queue<Task<SyndicationFeed>> Responses=new Queue<Task<SyndicationFeed>>();public Task<SyndicationFeed> RetrieveFeedAsync(Uri uri){if(uri.Host!="example.invalid")throw new Exception("Only synthetic feeds permitted");return Responses.Dequeue();}}
}
