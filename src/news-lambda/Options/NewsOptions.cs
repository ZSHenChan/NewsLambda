// using trade_api.Contracts.Types;

namespace Options;

public sealed class NewsReportOptions
{
  public required string YahooFinanceUrl { get; set; }
  public required string NewsMinimalistUrl { get; set; }
  public required int MaxFeedCount { get; set; }
  public required int PmReportHour { get; set; }
  public required int PmReportMinute { get; set; }
  public required int AmReportHour { get; set; }
  public required int AmReportMinute { get; set; }
}
