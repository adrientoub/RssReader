namespace RssReader.Library
{
    using System;

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
