using System;

namespace RssReader.Library.FeedParsers
{
    public static class FeedParserCreator
    {
        public static IFeedParser Create(FeedParserType type)
        {
            switch (type)
            {
                case FeedParserType.CodeHollow:
                    return new CodeHollowFeedParser();
                case FeedParserType.Custom:
                    return new CustomFeedParser();
                case FeedParserType.Microsoft:
                    return new MicrosoftFeedParser();
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }
}
