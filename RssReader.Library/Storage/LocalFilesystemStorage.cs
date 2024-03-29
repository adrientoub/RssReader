﻿namespace RssReader.Library.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using CsvHelper;
    using CsvHelper.Configuration;

    public class LocalFilesystemStorage : IFeedStorage
    {
        private readonly string _basePath;

        /// <summary>
        /// CsvConfiguration is not thread-safe. This can cause crash when reading lots of feeds in parallel.
        /// Do not share configurations until this is fixed.
        /// cf. https://github.com/JoshClose/CsvHelper/issues/1320
        /// </summary>
        private CsvConfiguration _csvConfiguration => new CsvConfiguration(CultureInfo.InvariantCulture);

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
            using var fileReader = File.OpenText(Path.Combine(_basePath, path));
            using var csvReader = new CsvReader(fileReader, _csvConfiguration);
            var records = csvReader.GetRecords<FeedInfo>();
            return Task.FromResult(records.Select(info => new Feed(info)).ToList());
        }

        public Task SaveFeedListToCsvAsync(string path, List<Feed> feeds)
        {
            string feedListPath = Path.Combine(_basePath, path);
            try
            {
                using StreamWriter fileWriter = File.CreateText(feedListPath);
                using var csvWriter = new CsvWriter(fileWriter, _csvConfiguration);
                csvWriter.WriteRecords(feeds.Select(feed => feed.Info));
            }
            catch (UnauthorizedAccessException)
            {
                Console.Error.WriteLine($"Could not write to {feedListPath}.");
            }

            return Task.CompletedTask;
        }

        #endregion

        #region FeedItems

        public Task SaveFeedItemsAsync(int year, int month, IEnumerable<FeedItem> feedItems, FeedInfo info)
        {
            string feedPath = FeedPath(year, month, info.CleanName);
            CreateDirectories(feedPath);
            try
            {
                using var fileWriter = File.CreateText(feedPath);
                try
                {
                    using var csvWriter = new CsvWriter(fileWriter, _csvConfiguration);
                    csvWriter.WriteRecords(feedItems);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Could not write CSV to disk: {e}");
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.Error.WriteLine($"Could not write to {feedPath}.");
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

            using StreamReader fileReader = File.OpenText(feedPath);
            using var csvReader = new CsvReader(fileReader, _csvConfiguration);
            var records = csvReader.GetRecords<FeedItem>();
            feed.Items.AddRange(records);
            return Task.CompletedTask;
        }

        private List<(int year, int month)> FindMonthsToLoad()
        {
            List<(int year, int month)> monthsToLoad = new List<(int year, int month)>();
            foreach (int year in Enumerable.Range(2010, 15)) // Be less specific
            {
                if (Directory.Exists(year.ToString()))
                {
                    foreach (string enumerateDirectory in Directory.EnumerateDirectories(year.ToString()))
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
