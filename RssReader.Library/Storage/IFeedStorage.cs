namespace RssReader.Library.Storage
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public interface IFeedStorage
    {
        /// <summary>Load all the feed items of all the feeds from the storage.</summary>
        /// <param name="feeds">The feeds to refresh.</param>
        Task LoadFeedItemsAsync(List<Feed> feeds);

        /// <summary>Load a month of feed items for a specific feed.</summary>
        /// <param name="year">The year to load.</param>
        /// <param name="month">The month to load.</param>
        /// <param name="feed">The feed.</param>
        Task LoadMonthAsync(int year, int month, Feed feed);

        /// <summary>Read a feed list from a CSV file.</summary>
        /// <param name="path">The location of the file.</param>
        /// <returns>A list of feeds that is loaded from the CSV. This will not refresh the content of each feed but only fill the FeedInfo.</returns>
        Task<List<Feed>> ReadFeedListFromCsvAsync(string path);

        /// <summary>
        /// Read a feed list from an OPML (Outline Processor Markup Language) file.
        /// OPML is a standard file format used for list of web feeds.
        /// </summary>
        /// <param name="path">The location of the OPML file.</param>
        /// <returns>The list of feeds read from the OPML file.</returns>
        Task<List<Feed>> ReadFeedListFromOpmlAsync(string path);

        /// <summary>Save back a collection of feed items to the storage.</summary>
        /// <param name="year">The year of all the items.</param>
        /// <param name="month">The month of all the items.</param>
        /// <param name="feedItems">The items to save.</param>
        /// <param name="info">The feed they are related to.</param>
        Task SaveFeedItemsAsync(int year, int month, IEnumerable<FeedItem> feedItems, FeedInfo info);

        /// <summary>Save a list of feeds in a CSV that can be read by the <see cref="ReadFeedListFromCsvAsync"/> method.</summary>
        /// <param name="path">The path.</param>
        /// <param name="feeds">The feeds to save.</param>
        Task SaveFeedListToCsvAsync(string path, List<Feed> feeds);
    }
}