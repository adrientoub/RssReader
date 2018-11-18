using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RssReader.Library;

namespace RssReader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var feeds = FeedList.ReadFeeds("rss.csv");

            var result = feeds.Feeds.Select(async feed =>
            {
                IEnumerable<FeedItem> items = await feed.ReadItems();
                feed.Add(items);
                feed.Save();
            });
            await Task.WhenAll(result);
        }
    }
}
