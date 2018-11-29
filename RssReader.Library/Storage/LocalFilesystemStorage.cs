namespace RssReader.Library.Storage
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using CsvHelper;

    public class LocalFilesystemStorage : IFeedStorage
    {
        private string _basePath;

        public LocalFilesystemStorage(string basePath = "")
        {
            _basePath = basePath;
        }

        #region FeedList

        public Task<List<Feed>> ReadFeedListFromOpmlAsync(string path)
        {
            var fileContent = File.ReadAllText(Path.Combine(_basePath, path));
            List<FeedInfo> records = OpmlParser.ParseFeed(fileContent);
            return Task.FromResult(records.Select(info => new Feed(info)).ToList());
        }

        public Task<List<Feed>> ReadFeedListFromCsvAsync(string path)
        {
            using (var fileReader = File.OpenText(Path.Combine(_basePath, path)))
            {
                using (var csvReader = new CsvReader(fileReader))
                {
                    var records = csvReader.GetRecords<FeedInfo>();
                    return Task.FromResult(records.Select(info => new Feed(info)).ToList());
                }
            }
        }

        public Task SaveFeedListToCsvAsync(string path, List<Feed> feeds)
        {
            using (StreamWriter fileWriter = File.CreateText(Path.Combine(_basePath, path)))
            {
                using (var csvWriter = new CsvWriter(fileWriter))
                {
                    csvWriter.WriteRecords(feeds.Select(feed => feed.Info));
                }
            }

            return Task.CompletedTask;
        }

        #endregion

        #region FeedItems

        public Task SaveFeedItemsAsync(IGrouping<(int year, int month), FeedItem> feedItems, FeedInfo info)
        {
            string feedPath = FeedPath(feedItems.Key.year, feedItems.Key.month, info.CleanName);
            CreateDirectories(feedPath);
            using (var fileWriter = File.CreateText(feedPath))
            {
                using (var csvWriter = new CsvWriter(fileWriter))
                {
                    csvWriter.WriteRecords(feedItems);
                }
            }
            return Task.CompletedTask;
        }

        private void CreateDirectories(string feedPath)
        {
            string directoryName = Path.GetDirectoryName(Path.Combine(_basePath, feedPath));
            if (directoryName != null)
            {
                Directory.CreateDirectory(directoryName);
            }
        }

        public async Task LoadFeedItemsAsync(List<Feed> feeds)
        {
            List<(int year, int month)> monthsToLoad = FindMonthsToLoad();

            IEnumerable<Task> tasks = feeds.Select(feed => Task.Run(() =>
                Task.WhenAll(monthsToLoad.Select(m => LoadMonthAsync(m.year, m.month, feed))).Wait()
            ));
            await Task.WhenAll(tasks);
            feeds.ForEach(feed => feed.RebuildDictionary());
        }

        public Task LoadMonthAsync(int year, int month, Feed feed)
        {
            feed.LoadedMonths.Add(Feed.MonthKeyName(year, month));
            string feedPath = FeedPath(year, month, feed.Info.CleanName);
            if (!File.Exists(feedPath))
            {
                return Task.CompletedTask;
            }

            using (var fileReader = File.OpenText(feedPath))
            {
                using (var csvReader = new CsvReader(fileReader))
                {
                    IEnumerable<FeedItem> records = csvReader.GetRecords<FeedItem>();
                    feed.Items.AddRange(records.Select(item =>
                    {
                        item.SetFeed(feed);
                        return item;
                    }));
                }
            }

            return Task.CompletedTask;
        }

        private List<(int year, int month)> FindMonthsToLoad()
        {
            List<(int year, int month)> monthsToLoad = new List<(int year, int month)>();
            foreach (int year in Enumerable.Range(2010, 10)) // Be less specific
            {
                var directoryPath = Path.Combine(_basePath, year.ToString());
                if (Directory.Exists(directoryPath))
                {
                    foreach (string enumerateDirectory in Directory.EnumerateDirectories(directoryPath))
                    {
                        string dirName = Path.GetFileName(enumerateDirectory);
                        if (int.TryParse(dirName, out int month))
                        {
                            monthsToLoad.Add((year, month));
                        }
                    }
                }
            }
            return monthsToLoad;
        }

        private string FeedPath(int year, int month, string cleanName)
        {
            return Path.Combine(_basePath, $"{year:D4}/{month:D2}/{cleanName}.csv");
        }

        #endregion
    }
}
