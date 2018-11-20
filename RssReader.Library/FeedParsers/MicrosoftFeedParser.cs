namespace RssReader.Library.FeedParsers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml;
    using Microsoft.SyndicationFeed;
    using Microsoft.SyndicationFeed.Atom;
    using Microsoft.SyndicationFeed.Rss;

    public class MicrosoftFeedParser : IFeedParser
    {
        /// <inheritdoc />
        public async Task<IEnumerable<FeedItem>> ParseFeedAsync(string content, string feedName)
        {
            using (var xmlReader = XmlReader.Create(new StringReader(content), new XmlReaderSettings() { Async = true }))
            {
                try
                {
                    RssFeedReader feedReader = new RssFeedReader(xmlReader);
                    return await ReadFeedAsync(feedReader, feedName);
                }
                catch (Exception)
                {
                    // ignored
                }
                try
                {
                    AtomFeedReader feedReader = new AtomFeedReader(xmlReader);
                    return await ReadFeedAsync(feedReader, feedName);
                }
                catch (Exception)
                {
                    return Enumerable.Empty<FeedItem>();
                }
            }
        }

        private static async Task<IEnumerable<FeedItem>> ReadFeedAsync(ISyndicationFeedReader feedReader, string feedName)
        {
            List<FeedItem> items = new List<FeedItem>();
            while (await feedReader.Read())
            {
                switch (feedReader.ElementType)
                {
                    case SyndicationElementType.Image:
                        ISyndicationImage image = await feedReader.ReadImage();
                        // We might want to save this for the UI
                        break;

                    case SyndicationElementType.Item:
                        ISyndicationItem item = await feedReader.ReadItem();
                        items.Add(new FeedItem()
                        {
                            Date = item.LastUpdated,
                            Description = item.Description.Trim(),
                            FeedName = feedName,
                            Guid = item.Id,
                            Title = item.Title.Trim(),
                            Link = item.Links.FirstOrDefault()?.Uri.ToString(),
                        });
                        break;
                }
            }

            return items;
        }
    }
}
