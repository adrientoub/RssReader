namespace RssReader.Library
{
    using System;
    using CsvHelper.Configuration.Attributes;

    public class FeedItem
    {
        public string FeedName { get; set; }

        [Format("o")]
        public DateTimeOffset Date { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Link { get; set; }
        
        public string Guid { get; set; }

        public bool Read { get; set; } = false;
    }
}
