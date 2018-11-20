namespace RssReader
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using RssReader.Library;
    using RssReader.Library.FeedParsers;

    public class Program
    {
        static async Task Main(string[] args)
        {
            // IFeedParser parser = new CustomFeedParser();
            IFeedParser parser = new MicrosoftFeedParser();
            FeedList feeds;
            try
            {
                feeds = FeedList.ReadFeeds("rss.csv");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.Read();
                return;
            }

            feeds.Load();
            var result = feeds.Feeds.Select(async feed =>
            {
                IEnumerable<FeedItem> items = await feed.ReadItems(parser);
                feed.Add(items);
                feed.Save();
            });
            await Task.WhenAll(result);
        }
    }
}
