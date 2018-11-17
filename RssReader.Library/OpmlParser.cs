namespace RssReader.Library
{
    using System.Collections.Generic;
    using System.Xml;

    public class OpmlParser
    {
        public static List<FeedInfo> ParseFeed(string content)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(content);
            XmlNodeList xmlNodeList = xmlDocument.GetElementsByTagName("outline");
            List<FeedInfo> feedList = new List<FeedInfo>();
            foreach (XmlNode item in xmlNodeList)
            {
                var attributes = item.Attributes;
                if (attributes == null)
                {
                    continue;
                }
                feedList.Add(new FeedInfo(attributes["text"].InnerText, attributes["title"].InnerText, attributes["xmlUrl"].InnerText));
            }

            return feedList;
        }
    }
}
