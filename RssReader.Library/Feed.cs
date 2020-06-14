namespace RssReader.Library
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
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

        public async Task<string?> ReadFeedAsync()
        {
            return await ReadFeedAsync(Info.Url);
        }

        private async Task<string?> ReadFeedAsync(string? url)
        {
            if (string.IsNullOrEmpty(url))
            {
                Console.Error.WriteLine($"No feed URL for feed {Info.Name}.");
                return null;
            }
            HttpResponseMessage result;
            try
            {
                result = await Client.GetAsync(url);
            }
            catch (HttpRequestException e)
            {
                Console.Error.WriteLine($"Impossible to read from {Info.Name}: {url} - {e.Message}.");
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

            if (result.StatusCode == HttpStatusCode.Moved || result.StatusCode == HttpStatusCode.MovedPermanently)
            {
                string? newUrl = result.Headers?.Location?.AbsoluteUri;
                if (newUrl != null)
                {
                    Console.Error.WriteLine($"Redirecting feed '{Info.Name}' to {newUrl} because of status code {result.StatusCode} ({(int) result.StatusCode}).");
                    return await ReadFeedAsync(newUrl);
                }
            }
            Console.Error.WriteLine(
                $"Feed {Info.Name} at {Info.Url} failed with status {result.StatusCode} ({(int)result.StatusCode}).");
            return null;
        }

        public async Task<IEnumerable<FeedItem>> ReadItemsAsync(IFeedParser feedParser)
        {
            string? feed;
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
            var newDictionary = new Dictionary<string, FeedItem>();
            HashSet<FeedItem> toRemove = new HashSet<FeedItem>();
            foreach (var item in Items)
            {
                item.Guid ??= item.GenerateNotNullableGuid();
                if (newDictionary.TryGetValue(item.Guid, out FeedItem alreadySaved))
                {
                    // If the Guid is already present we will remove the oldest occurence
                    if (alreadySaved.Date > item.Date)
                    {
                        toRemove.Add(item);
                        _toSave.Add((alreadySaved.Date.Year, alreadySaved.Date.Month));
                        continue;
                    }
                    _toSave.Add((item.Date.Year, item.Date.Month));
                    toRemove.Add(alreadySaved);
                }
                newDictionary[item.Guid] = item;
            }

            if (toRemove.Any())
            {
                foreach (var itemToRemove in toRemove)
                {
                    _toSave.Add((itemToRemove.Date.Year, itemToRemove.Date.Month));
                }
                int removed = Items.RemoveAll(i => toRemove.Contains(i));
                Console.WriteLine($"Removed {removed} duplicate items in feed '{Info.Name}'.");
            }

            _uniqueItems = newDictionary;
        }

        public void Add(IEnumerable<FeedItem> feedItems, Action<FeedItem> toDoForNew)
        {
            // TODO: load dynamically months where data is to be added.
            foreach (var item in feedItems)
            {
                item.Guid ??= item.GenerateNotNullableGuid();
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
            ILookup<(int month, int year), FeedItem> grouped =
                Items.ToLookup(item => (item.Date.Year, item.Date.Month));
            foreach (var keyToSave in _toSave)
            {
                IEnumerable<FeedItem> feedItems = grouped[keyToSave];
                if (!LoadedMonths.Contains(MonthKeyName(keyToSave.year, keyToSave.month)))
                {
                    // Load
                }
                await storage.SaveFeedItemsAsync(keyToSave.year, keyToSave.month, feedItems, Info);
            }
            _toSave.Clear();
        }

        public static string MonthKeyName(int year, int month)
        {
            return $"{year:D4}-{month:D2}";
        }
    }
}