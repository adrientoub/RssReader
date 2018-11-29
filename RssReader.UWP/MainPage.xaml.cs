using System;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RssReader.UWP
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;
    using Windows.ApplicationModel.Core;
    using Windows.Foundation;
    using Windows.Storage;
    using Windows.System;
    using Windows.System.Threading;
    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Input;
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

        private IFeedStorage _storage;

        public MainPage()
        {
            InitializeComponent();
            var folder = ApplicationData.Current.LocalFolder;
            var storage = new LocalFilesystemStorage();
            try
            {
                Feeds = storage.ReadFeedListFromCsvAsync("rss.csv").Result;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return;
            }

            _storage = new LocalFilesystemStorage(folder.Path);
            IAsyncAction wi = ThreadPool.RunAsync(async workItem =>
            {
                await _storage.LoadFeedItemsAsync(Feeds);
                RefreshUi();
                IFeedParser parser = new MicrosoftFeedParser();
                await RefreshAsync(parser, _storage, Feeds);
                RefreshUi();
            });
            ThreadPoolTimer.CreatePeriodicTimer(async action =>
            {
                IFeedParser parser = new MicrosoftFeedParser();
                await RefreshAsync(parser, _storage, Feeds);
                RefreshUi();
            }, TimeSpan.FromMinutes(5));
        }

        private void RefreshUi()
        {
            var items = new List<FeedItem>();
            foreach (var feed in Feeds)
            {
                foreach (var feedItem in feed.Items)
                {
                    items.Add(feedItem);
                }
            }

            var sorted = items.OrderBy(item => item.Date).ToList();
            var launched = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.High,
                new DispatchedHandler(() =>
                {
                    Items.Clear();
                    foreach (var item in sorted)
                    {
                        Items.Add(item);
                    }
                }));
        }

        private static async Task RefreshAsync(IFeedParser parser, IFeedStorage storage, List<Feed> feeds)
        {
            var result = feeds.Select(async feed =>
            {
                IEnumerable<FeedItem> items = await feed.ReadItemsAsync(parser);
                feed.Add(items,
                    item => Console.WriteLine($"{item.Date:s}: {item.FeedName} - {item.Title} - {item.Link}"));
                await feed.SaveAsync(storage);
            });
            await Task.WhenAll(result);
        }

        private void ListViewBase_OnItemClick(object sender, ItemClickEventArgs e)
        {
            FeedItem item = (FeedItem) e.ClickedItem;
            item.MarkAsRead();
        }

        private void UIElement_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var frameworkElement = (FrameworkElement) e.OriginalSource;
            var item = (FeedItem) frameworkElement.DataContext;

            var launched = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Low,
                new DispatchedHandler(async () =>
                {
                    await Windows.System.Launcher.LaunchUriAsync(new Uri(item.Link));
                }));
        }

        private void UIElement_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.G)
            {
                NextUnread(sender, true);
            }
            else if (e.Key == VirtualKey.T)
            {
                NextUnread(sender, false);
            }
            var ctrl = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
            if ((ctrl & CoreVirtualKeyStates.Down) != 0 && e.Key == VirtualKey.S)
            {
                Feeds.ForEach(feed => feed.SaveAsync(_storage));
            }
            if ((ctrl & CoreVirtualKeyStates.Down) != 0 && e.Key == VirtualKey.R)
            {
                RefreshUi();
            }
        }

        private void NextUnread(object sender, bool down)
        {
            var listView = (ListView)sender;
            int index = listView.SelectedIndex;
            if (down)
            {
                if (index < 0)
                {
                    index = 0;
                }
                for (var i = index; i < Items.Count; i++)
                {
                    if (!Items[i].Read)
                    {
                        listView.SelectedIndex = i;
                        listView.ScrollIntoView(Items[i]);
                        Items[i].MarkAsRead();
                        return;
                    }
                }
                for (var i = 0; i < index; i++)
                {
                    if (!Items[i].Read)
                    {
                        listView.SelectedIndex = i;
                        listView.ScrollIntoView(Items[i]);
                        Items[i].MarkAsRead();
                        return;
                    }
                }
            }
            else
            {
                if (index < 0)
                {
                    index = Items.Count - 1;
                }
                for (var i = index; i >= 0; i--)
                {
                    if (!Items[i].Read)
                    {
                        listView.SelectedIndex = i;
                        listView.ScrollIntoView(Items[i]);
                        Items[i].MarkAsRead();
                        return;
                    }
                }
                for (var i = Items.Count - 1; i >= index; i--)
                {
                    if (!Items[i].Read)
                    {
                        listView.SelectedIndex = i;
                        listView.ScrollIntoView(Items[i]);
                        Items[i].MarkAsRead();
                        return;
                    }
                }
            }
        }
    }
}