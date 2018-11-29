namespace RssReader.Library
{
    using System;
    using CsvHelper.Configuration.Attributes;

    public class FeedItem
    {
        private Feed _feed;

        public string FeedName { get; set; }

        [Format("o")]
        public DateTimeOffset Date { get; set; }

        public string Title { get; set; }

        [TypeConverter(typeof(Base64Gzip))]
        public string Description { get; set; }

        public string Link { get; set; }
        
        public string Guid { get; set; }

        public bool Read { get; set; } = false;

        public void SetFeed(Feed feed)
        {
            _feed = feed;
        }

        public void MarkAsRead()
        {
            if (!Read)
            {
                Read = true;
                _feed?.SetSave(Date.Year, Date.Month);
            }
        }

        public static bool operator==(FeedItem self, FeedItem other)
        {
            if (ReferenceEquals(null, self) && ReferenceEquals(null, other))
            {
                return true;
            }

            if (ReferenceEquals(null, self) || ReferenceEquals(null, other))
            {
                return false;
            }
            return self.Date == other.Date &&
                   self.Description == other.Description &&
                   self.Guid == other.Guid &&
                   self.Link == other.Link &&
                   self.Title == other.Title;
        }

        public static bool operator !=(FeedItem self, FeedItem other)
        {
            return !(self == other);
        }
    }
}
