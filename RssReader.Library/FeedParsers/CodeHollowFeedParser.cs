namespace RssReader.Library.FeedParsers
{
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using CodeHollow.FeedReader;
    using FeedItem = RssReader.Library.FeedItem;

    public class CodeHollowFeedParser: IFeedParser
    {
        public Task<IEnumerable<FeedItem>> ParseFeedAsync(string content, string feedName)
        {
            List<FeedItem> items = new List<FeedItem>();
            Feed feed;
            try
            {
                feed = FeedReader.ReadFromString(content);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error while parsing {feedName}: {e.Message}.");
                return Task.FromResult((IEnumerable<FeedItem>)items);
            }

            foreach (CodeHollow.FeedReader.FeedItem item in feed.Items)
            {
                try
                {
                    items.Add(new FeedItem
                    {
                        Date = item.PublishingDate ?? DateTimeOffset.Now,
                        Description = (item.Description ?? item.Content)?.Trim(),
                        FeedName = feedName,
                        Guid = item.Id,
                        Title = item.Title.Trim(),
                        Link = item.Link,
                    });
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Error while reading feed {feedName}: {e.Message}. {item.Id}: {item.Title}.");
                }
            }

            return Task.FromResult((IEnumerable<FeedItem>) items);
        }
    }
}
