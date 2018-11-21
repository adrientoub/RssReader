namespace RssReader.Library
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using CsvHelper;

    public class FeedList
    {
        public List<Feed> Feeds { get; set; } = new List<Feed>();

        public static FeedList ReadFeeds(string path)
        {
            using (var fileReader = File.OpenText(path))
            {
                using (var csvReader = new CsvReader(fileReader))
                {
                    var records = csvReader.GetRecords<FeedInfo>();
                    return new FeedList
                    {
                        Feeds = records.Select(info => new Feed(info)).ToList(),
                    };
                }
            }
        }

        public async Task LoadAsync()
        {
            // TODO: handle blobs...
            List<(int year, int month)> monthsToLoad = FindMonthsToLoad();

            IEnumerable<Task> tasks = Feeds.Select(feed => Task.Run(() =>
                monthsToLoad.ForEach(m => feed.LoadMonth(m.year, m.month))
            ));
            await Task.WhenAll(tasks);
            Feeds.ForEach(feed => feed.RebuildDictionary());
        }

        // TODO: should be abstracted in a storage class to be able to do it with any storage
        private static List<(int year, int month)> FindMonthsToLoad()
        {
            List<(int year, int month)> monthsToLoad = new List<(int year, int month)>();
            foreach (int year in Enumerable.Range(2010, 10)) // Be less specific
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

        public static FeedList ReadFeedsOpml(string path)
        {
            var fileContent = File.ReadAllText(path);
            List<FeedInfo> records = OpmlParser.ParseFeed(fileContent);
            return new FeedList
            {
                Feeds = records.Select(info => new Feed(info)).ToList(),
            };
        }

        public void SaveFeeds(string path)
        {
            using (var fileWriter = File.CreateText(path))
            {
                using (var csvWriter = new CsvWriter(fileWriter))
                {
                    csvWriter.WriteRecords(Feeds.Select(feed => feed.Info));
                }
            }
        }
    }
}
