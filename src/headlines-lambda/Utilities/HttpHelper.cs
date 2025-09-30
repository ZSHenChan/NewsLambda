using System.ServiceModel.Syndication;
using System.Xml;

namespace Utilities;

public static class HttpHelper
{
  public static async Task<IEnumerable<SyndicationItem>> ConsumeRssFeed(
    string feedUrl,
    int maxFeedCount = 10,
    int retryCount = 3
  )
  {
    for (int i = 0; i < retryCount - 1; i++)
    {
      try
      {
        // 1. Fetch the XML content from the URL
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "YourApp/1.0"); // Good practice to identify your app
        var xmlString = await httpClient.GetStringAsync(feedUrl);

        // 2. Load the XML into an XmlReader
        using var stringReader = new StringReader(xmlString);
        using var xmlReader = XmlReader.Create(stringReader);

        // 3. Parse the feed using SyndicationFeed
        var feed = SyndicationFeed.Load(xmlReader);

        return feed.Items.Take(maxFeedCount);
      }
      catch (HttpRequestException e)
      {
        Console.WriteLine($"Error fetching feed: {e.Message}");
        return [];
      }
      catch (XmlException e)
      {
        Console.WriteLine($"Error parsing XML feed: {e.Message}");
        return [];
      }
      catch (Exception e)
      {
        Console.WriteLine($"An unexpected error occurred: {e.Message}");
        continue;
      }
    }
    Console.WriteLine($"Failed to fetch RSS feed after {retryCount} trials.");
    return [];
  }
}
