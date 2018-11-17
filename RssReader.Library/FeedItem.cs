namespace RssReader.Library
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class FeedItem
    {
        public string FeedName { get; set; }

        public DateTimeOffset Date { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Link { get; set; }
        
        public string Guid { get; set; }
    }
}
