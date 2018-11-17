namespace RssReader.Library
{
    using System;
    using System.Collections.Generic;
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
            return await result.Content.ReadAsStringAsync();
        }

        public async Task<List<FeedItem>> ReadItems()
        {
            var feed = await ReadFeedAsync();
            return FeedParser.ParseFeed(feed, Name);
        }
    }
}
