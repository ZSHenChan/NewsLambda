using System.ServiceModel.Syndication;
using Contracts.Types;

namespace Utilities;

public static class NewsItemMapper
{
  public static NewsItem MapSynItemToNewsItem(
    SyndicationItem item,
    string publisher = "Unknown Publisher"
  )
  {
    return new NewsItem()
    {
      Title = item?.Title?.Text ?? "No Title",
      Summary = item?.Summary?.Text ?? "No Summary",
      Links =
      [
        new NewsLinkItem()
        {
          Publisher = publisher,
          Link = item?.Links.FirstOrDefault()?.Uri?.ToString() ?? "No Url",
        },
      ],
    };
  }
}
