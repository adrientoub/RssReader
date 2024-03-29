namespace RssReader.Library.FeedParsers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml;

    public class CustomFeedParser: IFeedParser
    {
        private const string NamespaceFilter = @"xmlns=""([^""]+)""|xsi(:\w+)?=""([^""]+)""";
        private static readonly Regex NamespaceRegex = new Regex(NamespaceFilter);

        public async Task<IEnumerable<FeedItem>> ParseFeedAsync(string content, string? feedName)
        {
            // Should always be true
            if (!string.IsNullOrEmpty(feedName))
            {
                Directory.CreateDirectory("output");
                File.WriteAllText($"output/{Regex.Replace(feedName, @"[^A-Za-z0-9-]+", "")}.xml", content);
            }
            await Task.Yield();
            var filtered = NamespaceRegex.Replace(content, "");
            var xmlDocument = new XmlDocument();
            try
            {
                xmlDocument.LoadXml(filtered);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error while parsing {feedName}: {e}.");
                return Enumerable.Empty<FeedItem>();
            }
            // if RSS
            var rssNode = xmlDocument.SelectSingleNode("rss");
            if (rssNode != null)
            {
                return ParseRss(xmlDocument, feedName);
            }
            var atomNode = xmlDocument.SelectSingleNode("feed");
            // if Atom
            if (atomNode != null)
            {
                return ParseAtom(xmlDocument, feedName);
            }
            return Enumerable.Empty<FeedItem>();
        }

        private static List<FeedItem> ParseAtom(XmlDocument xmlDocument, string? feedName)
        {
            XmlNodeList xmlNodeList = xmlDocument.GetElementsByTagName("entry");
            List<FeedItem> feedItems = new List<FeedItem>();
            foreach (XmlNode entry in xmlNodeList)
            {
                var title = entry.SelectSingleNode("title");
                var description = entry.SelectSingleNode("summary") ?? entry.SelectSingleNode("content");
                var date = entry.SelectSingleNode("published") ?? entry.SelectSingleNode("updated");
                var guid = entry.SelectSingleNode("id");
                var link = entry.SelectSingleNode("link");
                string? linkText = null;
                if (!DateTimeOffset.TryParse(date.InnerText, out DateTimeOffset parsedDate))
                {
                    Console.Error.WriteLine($"Impossible to parse date {date.InnerText} in feed {feedName}, defaulting to now.");
                    parsedDate = DateTimeOffset.Now;
                }
                if (link?.Attributes != null)
                {
                    linkText = link.Attributes["href"].InnerText;
                }
                feedItems.Add(new FeedItem
                {
                    FeedName = feedName,
                    Title = title.InnerText.Trim(),
                    Description = description.InnerText.Trim(),
                    Guid = guid.InnerText.Trim(),
                    Link = linkText,
                    Date = parsedDate,
                });
            }

            return feedItems;
        }

        private static List<FeedItem> ParseRss(XmlDocument xmlDocument, string? feedName)
        {
            XmlNodeList xmlNodeList = xmlDocument.GetElementsByTagName("item");
            List<FeedItem> feedItems = new List<FeedItem>();
            foreach (XmlNode item in xmlNodeList)
            {
                var title = item.SelectSingleNode("title");
                var description = item.SelectSingleNode("description") ??
                                  item.SelectSingleNode("encoded");
                var date = item.SelectSingleNode("pubDate");
                var guid = item.SelectSingleNode("guid");
                var link = item.SelectSingleNode("link");
                feedItems.Add(new FeedItem
                {
                    FeedName = feedName,
                    Title = title.InnerText.Trim(),
                    Description = description?.InnerText.Trim(),
                    Guid = guid.InnerText.Trim(),
                    Link = link.InnerText.Trim(),
                    Date = DateTimeOffset.Parse(date.InnerText),
                });
            }

            return feedItems;
        }
    }
    }
