namespace RssReader.UWP
{
    using Windows.UI.Text;

    public static class HelperFunctions
    {
        public static FontWeight ConvertReadToFontWeight(bool isRead)
        {
            return isRead ? FontWeights.Normal : FontWeights.Bold;
        }
    }
}
