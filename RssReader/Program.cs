namespace RssReader
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using RssReader.Library;
    using RssReader.Library.FeedParsers;
    using RssReader.Library.Storage;

    public class Program
    {
        static async Task Main(string[] args)
        {
            // IFeedParser parser = new CustomFeedParser();
            IFeedParser parser = new MicrosoftFeedParser();
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
            Console.SetError(TextWriter.Null);
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
