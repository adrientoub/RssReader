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
            var feeds = new FeedList
            {
                Feeds = new List<Feed>()
            };
            feeds.Feeds.Add(new Feed(new FeedInfo("Engadget", "https://www.engadget.com/rss.xml")));
            feeds.Feeds.Add(new Feed(new FeedInfo("JeuxVideo.com", "http://www.jeuxvideo.com/rss/rss.xml")));
            feeds.SaveFeeds("feeds.csv");

            foreach (var feed in feeds.Feeds)
            {
                var items = await feed.Info.ReadItems();
                feed.Add(items);
            }
        }
    }
}
