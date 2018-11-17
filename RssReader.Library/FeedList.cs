namespace RssReader.Library
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using CsvHelper;

    public class FeedList
    {
        public List<Feed> Feeds { get; set; }

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
