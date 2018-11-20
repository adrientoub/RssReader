namespace RssReader.Library
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using CsvHelper;
    using RssReader.Library.FeedParsers;

    public class Feed
    {
        public FeedInfo Info { get; set; }

        public List<FeedItem> Items { get; set; } = new List<FeedItem>();

        private readonly HashSet<string> _loadedMonths = new HashSet<string>();

        public Feed(FeedInfo feedInfo)
        {
            Info = feedInfo;
        }

        public async Task<string> ReadFeedAsync()
        {
            HttpClient client = new HttpClient();
            var result = await client.GetAsync(Info.Url);
            if (result.IsSuccessStatusCode)
            {
                return await result.Content.ReadAsStringAsync();
            }

            Console.WriteLine($"Feed {Info.Name} at {Info.Url} failed with status {result.StatusCode} ({(int)result.StatusCode}).");
            return null;
        }

        public async Task<IEnumerable<FeedItem>> ReadItems(IFeedParser feedParser)
        {
            string feed;
            try
            {
                feed = await ReadFeedAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to read {Info.Name} at {Info.Url}.");
                Console.WriteLine(e);
                return Enumerable.Empty<FeedItem>();
            }

            if (feed == null)
            {
                return Enumerable.Empty<FeedItem>();
            }
            return await feedParser.ParseFeedAsync(feed, Info.Name);
        }

        public void Add(IEnumerable<FeedItem> feedItems)
        {
            Dictionary<string, FeedItem> dict = new Dictionary<string, FeedItem>();
            // TODO: load dynamically months where data is to be added.
            foreach (var item in Items)
            {
                dict.Add(item.Guid, item);
            }
            foreach (var item in feedItems)
            {
                if (dict.ContainsKey(item.Guid))
                {
                    dict[item.Guid] = item;
                }
                else
                {
                    dict.Add(item.Guid, item);
                }
            }
            Items = dict.Values.OrderBy(item => item.Date).ToList();
        }

        public void Save()
        {
            // TODO: save to blob
            // TODO: use a base path
            // TODO: gzip when all read and older than 6 months

            IEnumerable<IGrouping<(int year, int month), FeedItem>> grouped = Items.GroupBy(item => (item.Date.Year, item.Date.Month));
            foreach (IGrouping<(int year, int month), FeedItem> feedItems in grouped)
            {
                if (!_loadedMonths.Contains(MonthKeyName(feedItems.Key.year, feedItems.Key.month)))
                {
                    // Load
                }

                string feedPath = FeedPath(feedItems.Key.year, feedItems.Key.month);
                CreateDirectories(feedPath);
                using (var fileWriter = File.CreateText(feedPath))
                {
                    using (var csvWriter = new CsvWriter(fileWriter))
                    {
                        csvWriter.WriteRecords(feedItems);
                    }
                }
            }
        }

        private void CreateDirectories(string feedPath)
        {
            // TODO: abstract storage
            string directoryName = Path.GetDirectoryName(feedPath);
            Directory.CreateDirectory(directoryName);
        }

        private void LoadMonth(DateTimeOffset itemDate)
        {
            _loadedMonths.Add(MonthKeyName(itemDate));
            string feedPath = FeedPath(itemDate);
            // TODO: add ability to read from external filesystem (blob)
            // TODO: add ability to read gziped content
            if (!File.Exists(feedPath))
            {
                return;
            }
            using (var fileReader = File.OpenText(feedPath))
            {
                using (var csvReader = new CsvReader(fileReader))
                {
                    var records = csvReader.GetRecords<FeedItem>();
                    Items.AddRange(records);
                }
            }
        }

        private static string MonthKeyName(int year, int month)
        {
            return $"{year:D4}-{month:D2}";
        }

        private static string MonthKeyName(DateTimeOffset dateTimeOffset)
        {
            return MonthKeyName(dateTimeOffset.Year, dateTimeOffset.Month);
        }

        private string FeedPath(int year, int month)
        {
            return $"{year:D4}/{month:D2}/{Info.CleanName}.csv";
        }

        private string FeedPath(DateTimeOffset itemDate)
        {
            return FeedPath(itemDate.Year, itemDate.Month);
        }
    }
}
