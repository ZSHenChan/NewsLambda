using System.ServiceModel.Syndication;
using System.Xml;
using Contracts.Interfaces;
using Contracts.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Options;
using Utilities;

namespace Services;

public class NewsService
{
  private readonly HttpClient _httpClient;
  private readonly DailyNewsOptions _dailyNewsOptions;
  private readonly ILogger<NewsService> _logger;
  private readonly ITelegramService _telegramService;
  private readonly GeminiService _geminiService;

  public NewsService(
    HttpClient client,
    ILogger<NewsService> logger,
    IOptions<DailyNewsOptions> dailyNewsOptions,
    ITelegramService telegramService,
    GeminiService geminiService
  )
  {
    _httpClient = client;
    _httpClient.DefaultRequestHeaders.Add("User-Agent", "YourApp/1.0");
    _logger = logger;
    _dailyNewsOptions = dailyNewsOptions.Value;
    _telegramService = telegramService;
    _geminiService = geminiService;
  }

  public async Task FetchNews()
  {
    _logger.LogInformation($"{nameof(NewsService)} started running");

    DateTimeOffset currentTime = DateTimeOffset.Now;
    TimeZoneInfo targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(_dailyNewsOptions.TimeZone);
    DateTimeOffset targetTime = TimeZoneInfo.ConvertTime(currentTime, targetTimeZone);

    var reportHours = _dailyNewsOptions.ReportHours;
    var today = targetTime.Date;
    var yesterday = today.AddDays(-1);

    List<DateTimeOffset> potentialThresholds =
    [
      .. reportHours.Select(h => new DateTimeOffset(
        yesterday.Year,
        yesterday.Month,
        yesterday.Day,
        h,
        0,
        0,
        targetTime.Offset
      )),
      .. reportHours.Select(h => new DateTimeOffset(
        today.Year,
        today.Month,
        today.Day,
        h,
        0,
        0,
        targetTime.Offset
      )),
    ];

    DateTimeOffset thresholdHour = potentialThresholds.Last(t => t < targetTime);
    thresholdHour = potentialThresholds[potentialThresholds.IndexOf(thresholdHour) - 1];
    _logger.LogInformation($"threshold hour: {thresholdHour}");

    IEnumerable<SyndicationItem> allNewsRssFeeds = await FetchRssFeeds(
      _dailyNewsOptions.WorldRssList,
      thresholdHour
    );

    if (!allNewsRssFeeds.Any())
    {
      _logger.LogWarning($"{nameof(FetchNews)} - No News found from RSS source");
      return;
    }

    List<NewsItem>? newsItem = await _geminiService.CleanupNews(allNewsRssFeeds);

    if (newsItem == null)
    {
      _logger.LogWarning($"{nameof(SendNews)} - Gemini returned null");
      return;
    }

    List<NewsItem> eligibleNewsItems =
    [
      .. newsItem.Where(item => item.Links.Count >= _dailyNewsOptions.MinimumReportCount),
    ];

    if (eligibleNewsItems.Count == 0)
    {
      _logger.LogWarning("No News items found");
      return;
    }

    List<NewsItem> individualNewsItems = [.. newsItem.Where(item => item.Links.Count == 1)];
    if (individualNewsItems.Count != 0)
    {
      List<NewsItem>? significantIndiNewsItems = await _geminiService.FilterSignificantNews(
        individualNewsItems
      );

      if (significantIndiNewsItems != null)
        eligibleNewsItems = [.. eligibleNewsItems, .. significantIndiNewsItems];
    }

    await SendNews("Daily News", eligibleNewsItems, targetTime);
    // await SendNews("BloomBerg Financials", marketItems, targetTime);
  }

  private static async Task<IEnumerable<SyndicationItem>> FetchRssFeeds(
    string[] rssSourceList,
    DateTimeOffset thresholdHour
  )
  {
    IEnumerable<SyndicationItem> allRssFeeds = [];
    foreach (string rssLink in rssSourceList)
    {
      IEnumerable<SyndicationItem> rssFeedList = await HttpHelper.ConsumeRssFeed(rssLink);

      IEnumerable<SyndicationItem> filteredFeed = rssFeedList.Where(feed =>
        feed.PublishDate > thresholdHour
      );

      allRssFeeds = allRssFeeds.Concat(filteredFeed) ?? [];
    }
    return allRssFeeds ?? [];
  }

  private async Task SendNews(string headline, List<NewsItem> newsItem, DateTimeOffset targetTime)
  {
    string header =
      $"*{StringFormatter.EscapeMarkdownV2($"{headline} - {targetTime:yyyy-MM-dd} ({GetTimeOfDayCategory(targetTime)})")}*";

    string body =
      $@"{$"{header}"}

{string.Join("\n\n", newsItem.Select(item => FormatItem(item)))}";

    await _telegramService.SendTextMessageAsync(message: body, forReal: true);
  }

  public async Task<IEnumerable<SyndicationItem>> ConsumeRssFeed(
    string feedUrl,
    int maxFeedCount = 10
  )
  {
    try
    {
      var xmlString = await _httpClient.GetStringAsync(feedUrl);

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
      return [];
    }
  }

  private static string FormatItem(NewsItem item)
  {
    return @$"*{StringFormatter.EscapeMarkdownV2($"__{item.Title}__")}*
{StringFormatter.EscapeMarkdownV2(item.Summary)}
{string.Join("\n", item.Links.Select(linkItem => $"[{StringFormatter.EscapeMarkdownV2(linkItem.Publisher)}]({linkItem.Link})"))}
";
  }

  private static string GetTimeOfDayCategory(DateTimeOffset dateTime)
  {
    int hour = dateTime.Hour;

    if (hour >= 5 && hour < 12)
    {
      return "Morning";
    }
    else if (hour == 12)
    {
      return "Noon";
    }
    else if (hour > 12 && hour < 17) // 1 PM to 4:59 PM
    {
      return "Afternoon";
    }
    else if (hour >= 17 && hour < 21) // 5 PM to 8:59 PM
    {
      return "Evening";
    }
    else
    {
      return "Night"; // 9 PM to 4:59 AM
    }
  }
}
