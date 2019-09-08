namespace RssReader
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using RssReader.Library.FeedParsers;
    using RssReader.Library.Storage;
    using Feed = Library.Feed;
    using FeedItem = RssReader.Library.FeedItem;

    public class Program
    {
        private const FeedParserType feedParserType = FeedParserType.CodeHollow;

        public static async Task Main(string[] args)
        {
            IFeedStorage storage = new LocalFilesystemStorage();
            List<Feed> feeds;
            try
            {
                feeds = await storage.ReadFeedListFromCsvAsync("rss.csv");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                Console.Read();
                return;
            }

            await storage.LoadFeedItemsAsync(feeds);
            if (args.Length > 0)
            {
                DisplayRecentItems(args[0], feeds);
            }
            else
            {
                await WatchAsync(storage, feeds);
            }
        }

        private static void DisplayRecentItems(string duration, List<Feed> feeds)
        {
            if (duration == "hour")
            {
                DisplayFeeds(feeds, TimeSpan.FromHours(1));
            }
            else if (duration == "day")
            {
                DisplayFeeds(feeds, TimeSpan.FromDays(1));
            }
            else if (duration == "week")
            {
                DisplayFeeds(feeds, TimeSpan.FromDays(7));
            }
            else
            {
                DisplayHelp();
            }
        }

        private static void DisplayHelp()
        {
            Console.WriteLine($"Usage: {System.AppDomain.CurrentDomain.FriendlyName} [day|hour|week]");
            Console.WriteLine("  to use in watch mode do not input any argument");
            Console.WriteLine("  to display recent items input the wanted duration");
        }

        private static void DisplayFeeds(List<Feed> feeds, TimeSpan timeSpan)
        {
            List<FeedItem> feedItems = new List<FeedItem>();
            foreach (var item in feeds)
            {
                feedItems.AddRange(item.Items);
            }
            DateTimeOffset limitDate = DateTimeOffset.UtcNow - timeSpan;
            foreach (var item in feedItems.OrderBy(item => item.Date).Where(item => item.Date >= limitDate))
            {
                Console.WriteLine($"{item.Date:s}: {item.FeedName} - {item.Title} - {item.Link}");
            }
        }

        private static async Task WatchAsync(IFeedStorage storage, List<Feed> feeds)
        {
            Console.SetError(TextWriter.Null);
            IFeedParser parser = FeedParserCreator.Create(feedParserType);
            while (true)
            {
                await RefreshAsync(parser, storage, feeds);
                await Task.Delay(TimeSpan.FromMinutes(5));
            }
        }

        private static async Task RefreshAsync(IFeedParser parser, IFeedStorage storage, List<Feed> feeds)
        {
            var result = feeds.Select(async feed =>
            {
                IEnumerable<FeedItem> items = await feed.ReadItemsAsync(parser);
                feed.Add(items, item => Console.WriteLine($"{item.Date:s}: {item.FeedName} - {item.Title} - {item.Link}"));
                await feed.SaveAsync(storage);
            });
            await Task.WhenAll(result);
            Console.WriteLine($"Refreshed at {DateTimeOffset.Now:s}.");
        }
    }
}
