using System;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RssReader.UWP
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.ApplicationModel.Core;
    using Windows.Foundation;
    using Windows.UI.Core;
    using RssReader.Library;
    using RssReader.Library.FeedParsers;
    using RssReader.Library.Storage;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<FeedItem> Items { get; }
            = new ObservableCollection<FeedItem>();
        private List<Feed> Feeds;

        public MainPage()
        {
            InitializeComponent();
            var storage = new LocalFilesystemStorage("D:\\projects\\RssLocal");
            try
            {
                Feeds = storage.ReadFeedListFromCsvAsync("rss.csv").Result;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }

            IAsyncAction wi = Windows.System.Threading.ThreadPool.RunAsync(async workItem =>
            {
                await storage.LoadFeedItemsAsync(Feeds);
                RefreshUi();
                IFeedParser parser = new MicrosoftFeedParser();
                await RefreshAsync(parser, storage, Feeds);
                RefreshUi();
            });
            Windows.System.Threading.ThreadPoolTimer.CreatePeriodicTimer(async action =>
            {
                IFeedParser parser = new MicrosoftFeedParser();
                await RefreshAsync(parser, storage, Feeds);
                RefreshUi();
            }, TimeSpan.FromMinutes(5));
        }

        private void RefreshUi()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.High,
                new DispatchedHandler(() =>
                {
                    Items.Clear();
                    foreach (var feed in Feeds)
                    {
                        foreach (var feedItem in feed.Items)
                        {
                            Items.Add(feedItem);
                        }
                    }
                }));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private static async Task RefreshAsync(IFeedParser parser, IFeedStorage storage, List<Feed> feeds)
        {
            var result = feeds.Select(async feed =>
            {
                IEnumerable<FeedItem> items = await feed.ReadItemsAsync(parser);
                feed.Add(items, item => Console.WriteLine($"{item.Date:s}: {item.FeedName} - {item.Title} - {item.Link}"));
                await feed.SaveAsync(storage);
            });
            await Task.WhenAll(result);
        }
    }
}
