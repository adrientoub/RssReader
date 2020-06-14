namespace RssReader.Library
{
    using System;
    using CsvHelper.Configuration.Attributes;

    public class FeedItem
    {
        public string? FeedName { get; set; }

        [Format("o")]
        public DateTimeOffset Date { get; set; }

        public string? Title { get; set; }

        [TypeConverter(typeof(Base64Gzip))]
        public string? Description { get; set; }

        public string? Link { get; set; }
        
        public string? Guid { get; set; }

        public bool Read { get; set; } = false;

        public string GenerateNotNullableGuid()
        {
            return Link ?? System.Guid.NewGuid().ToString();
        }
    }
}
