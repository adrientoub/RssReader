namespace RssReader.Library.FeedParsers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IFeedParser
    {
        Task<IEnumerable<FeedItem>> ParseFeedAsync(string content, string feedName);
    }
}
