namespace RssReader.Library
{
    using System.Collections.Generic;
    using System.Linq;

    public class Feed
    {
        public FeedInfo Info { get; set; }

        public List<FeedItem> Items { get; set; } = new List<FeedItem>();

        public Feed(FeedInfo feedInfo)
        {
            Info = feedInfo;
        }

        public void Add(IEnumerable<FeedItem> feedItems)
        {
            Dictionary<string, FeedItem> dict = new Dictionary<string, FeedItem>();
            foreach (var item in Items)
            {
                dict.Add(item.Guid, item);
            }
            foreach (var item in feedItems)
            {
                if (dict.ContainsKey(item.Guid))
                {
                    dict[item.Guid] = item;
                }
                else
                {
                    dict.Add(item.Guid, item);
                }
            }
            Items = dict.Values.OrderBy(item => item.Date).ToList();
        }
    }
}
