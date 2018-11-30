namespace RssReader.UWP
{
    using System;
    using Windows.UI.Text;
    using RssReader.Library;

    public static class HelperFunctions
    {
        public static FontWeight ConvertReadToFontWeight(bool isRead)
        {
            return isRead ? FontWeights.Normal : FontWeights.Bold;
        }

        public static Uri FeedItemToUri(object feedItem)
        {
            var item = feedItem as FeedItem;
            if (item == null)
            {
                return new Uri("https://www.bing.com/");
            }
            return new Uri(item.Link);
        }
    }
}
