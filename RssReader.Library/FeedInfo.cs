namespace RssReader.Library
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class FeedInfo
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Url { get; set; }

        public FeedInfo()
        {
        }

        public FeedInfo(string name, string displayName, string url)
        {
            Name = name;
            DisplayName = displayName;
            Url = url;
        }

        public FeedInfo(string name, string url)
        {
            DisplayName = Name = name;
            Url = url;
        }

        public async Task<string> ReadFeedAsync()
        {
            HttpClient client = new HttpClient();
            var result = await client.GetAsync(Url);
            if (result.IsSuccessStatusCode)
            {
                return await result.Content.ReadAsStringAsync();
            }

            Console.WriteLine($"Feed {Name} at {Url} failed with status {result.StatusCode} ({(int)result.StatusCode}).");
            return null;
        }

        public async Task<IEnumerable<FeedItem>> ReadItems()
        {
            string feed;
            try
            {
                feed = await ReadFeedAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to read {Name} at {Url}.");
                Console.WriteLine(e);
                return Enumerable.Empty<FeedItem>();
            }

            if (feed == null)
            {
                return Enumerable.Empty<FeedItem>();
            }
            return FeedParser.ParseFeed(feed, Name);
        }
    }
}
