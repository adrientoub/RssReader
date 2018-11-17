namespace RssReader.Library
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;

    public static class FeedParser
    {
        public static List<FeedItem> ParseFeed(string content, string feedName)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(content);
            XmlNodeList xmlNodeList = xmlDocument.GetElementsByTagName("item");
            List<FeedItem> feedItems = new List<FeedItem>();
            foreach (XmlNode item in xmlNodeList)
            {
                var title = item.SelectSingleNode("title");
                var description = item.SelectSingleNode("description");
                var date = item.SelectSingleNode("pubDate");
                var guid = item.SelectSingleNode("guid");
                var link = item.SelectSingleNode("link");
                feedItems.Add(new FeedItem
                {
                    FeedName = feedName,
                    Title = title.InnerText.Trim(),
                    Description = description.InnerText.Trim(),
                    Guid = guid.InnerText.Trim(),
                    Link = link.InnerText.Trim(),
                    Date = DateTimeOffset.Parse(date.InnerText),
                });
            }  

            return feedItems;
        }
    }
}
