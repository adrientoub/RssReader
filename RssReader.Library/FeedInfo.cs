namespace RssReader.Library
{
    using System.Text.RegularExpressions;
    using CsvHelper.Configuration.Attributes;

    public class FeedInfo
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Url { get; set; }

        private static readonly Regex NameCleaner = new Regex(@"[^A-Za-z0-9-]+");

        [Ignore]
        public string CleanName => NameCleaner.Replace(Name, "");

        public FeedInfo()
        {
        }

        public FeedInfo(string name, string displayName, string url)
        {
            Name = name;
            DisplayName = displayName;
            Url = url;
        }

        public FeedInfo(string name, string url)
        {
            DisplayName = Name = name;
            Url = url;
        }
    }
}
