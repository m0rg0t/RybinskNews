using System;using System.Linq;using System.Threading.Tasks;using Windows.Web.Syndication;using M0rg0tRss.Data;
class Program {
 static void Main(){Run().GetAwaiter().GetResult();}
 static SyndicationFeed Feed(){return new SyndicationFeed{Title=new SyndicationText("Synthetic")};}
 static async Task Run(){
 var feed=Feed();feed.Items.Add(new SyndicationItem{Id="first",Title=new SyndicationText("First"),Summary=new SyndicationText("First summary")});feed.Items.Add(new SyndicationItem{Id="empty",Title=new SyndicationText("Empty")});feed.Items.Add(new SyndicationItem{Id="content",Title=new SyndicationText("Content"),Content=new SyndicationText("Third content")});feed.Items.Add(new SyndicationItem{Id="empty2",Title=new SyndicationText("Empty again")});
 SyndicationClient.Responses.Enqueue(Task.FromResult(feed));await RssDataSource.AddGroupForFeedAsync("https://example.invalid/content");var group=RssDataSource.GetGroup("https://example.invalid/content");
 if(group.Items.Count!=4||group.Items[0].Content!="First summary"||group.Items[1].Content!=""||group.Items[2].Content!="Third content"||group.Items[3].Content!="")throw new Exception("Article content leaked between feed items");
 var a=new TaskCompletionSource<SyndicationFeed>();var b=new TaskCompletionSource<SyndicationFeed>();SyndicationClient.Responses.Enqueue(a.Task);SyndicationClient.Responses.Enqueue(b.Task);
 var first=RssDataSource.AddGroupForFeedAsync("https://example.invalid/race");var second=RssDataSource.AddGroupForFeedAsync("https://example.invalid/race");b.SetResult(Feed());if(!await second)throw new Exception("First completion failed");a.SetResult(Feed());if(await first)throw new Exception("Duplicate completion was accepted");if(RssDataSource.GetGroups("AllGroups").Count(g=>g.UniqueId=="https://example.invalid/race")!=1)throw new Exception("Duplicate groups");
 if(await RssDataSource.AddGroupForFeedAsync("https://example.invalid/race"))throw new Exception("Existing group accepted");
 Console.WriteLine("PASS: missing summary/content isolation, out-of-order duplicate feeds and existing group guard");
 }
}
