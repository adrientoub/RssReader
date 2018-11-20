namespace RssReader.Library
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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

        public void Load()
        {
            // TODO: handle blobs...
            foreach (int year in Enumerable.Range(2010, 10)) // Be less specific
            {
                if (Directory.Exists(year.ToString()))
                {
                    foreach (string enumerateDirectory in Directory.EnumerateDirectories(year.ToString()))
                    {
                        string dirName = Path.GetFileName(enumerateDirectory);
                        if (int.TryParse(dirName, out int month))
                        {
                            // TODO: do it async in parallel
                            Feeds.ForEach(feed => feed.LoadMonth(year, month));
                        }
                    }
                }
            }
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
