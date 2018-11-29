namespace RssReader.Library
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using RssReader.Library.FeedParsers;
    using RssReader.Library.Storage;

    public class Feed
    {
        public FeedInfo Info { get; set; }

        public List<FeedItem> Items { get; set; } = new List<FeedItem>();

        internal readonly HashSet<string> LoadedMonths = new HashSet<string>();

        private static readonly HttpClient Client = new HttpClient();

        private Dictionary<string, FeedItem> _uniqueItems = new Dictionary<string, FeedItem>();

        private readonly HashSet<(int year, int month)> _toSave = new HashSet<(int year, int month)>();

        public Feed(FeedInfo feedInfo)
        {
            Info = feedInfo;
        }

        public async Task<string> ReadFeedAsync()
        {
            HttpResponseMessage result;
            try
            {
                result = await Client.GetAsync(Info.Url);
            }
            catch (HttpRequestException e)
            {
                Console.Error.WriteLine($"Impossible to read from {Info.Name}: {Info.Url} - {e.Message}.");
                return null;
            }

            if (result.IsSuccessStatusCode)
            {
                try
                {
                    return await result.Content.ReadAsStringAsync();
                }
                catch (InvalidOperationException)
                {
                    return Encoding.UTF8.GetString(await result.Content.ReadAsByteArrayAsync());
                }
            }

            Console.Error.WriteLine(
                $"Feed {Info.Name} at {Info.Url} failed with status {result.StatusCode} ({(int)result.StatusCode}).");
            return null;
        }

        public async Task<IEnumerable<FeedItem>> ReadItemsAsync(IFeedParser feedParser)
        {
            string feed;
            try
            {
                feed = await ReadFeedAsync();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Failed to read {Info.Name} at {Info.Url}: {e}.");
                return Enumerable.Empty<FeedItem>();
            }

            if (feed == null)
            {
                return Enumerable.Empty<FeedItem>();
            }

            return await feedParser.ParseFeedAsync(feed, Info.Name);
        }

        internal void RebuildDictionary()
        {
            _uniqueItems = Items.ToDictionary(item => item.Guid);
        }

        public void Add(IEnumerable<FeedItem> feedItems, Action<FeedItem> toDoForNew)
        {
            // TODO: load dynamically months where data is to be added.
            foreach (var item in feedItems)
            {
                if (_uniqueItems.ContainsKey(item.Guid))
                {
                    _uniqueItems[item.Guid] = item;
                }
                else
                {
                    _toSave.Add((item.Date.Year, item.Date.Month));
                    _uniqueItems.Add(item.Guid, item);
                    toDoForNew(item);
                }
            }

            if (_toSave.Any())
            {
                Items = _uniqueItems.Values.OrderBy(item => item.Date).ToList();
            }
        }

        public async Task SaveAsync(IFeedStorage storage)
        {
            IEnumerable<IGrouping<(int year, int month), FeedItem>> grouped =
                Items.GroupBy(item => (item.Date.Year, item.Date.Month));
            foreach (IGrouping<(int year, int month), FeedItem> feedItems in
                     grouped.Where(group => _toSave.Contains((group.Key.year, group.Key.month))))
            {
                if (!LoadedMonths.Contains(MonthKeyName(feedItems.Key.year, feedItems.Key.month)))
                {
                    // Load
                }
                await storage.SaveFeedItemsAsync(feedItems, Info);
            }
            _toSave.Clear();
        }

        public static string MonthKeyName(int year, int month)
        {
            return $"{year:D4}-{month:D2}";
        }

        public void MarkAllAsRead()
        {
            Items.ForEach(item => item.Read = true);
        }
    }
}