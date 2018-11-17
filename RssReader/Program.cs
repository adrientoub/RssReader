using System;
using System.Collections.Generic;
using System.IO;
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

            foreach (var feed in feeds.Feeds)
            {
                var items = await feed.Info.ReadItems();
                feed.Add(items);
            }
        }
    }
}
