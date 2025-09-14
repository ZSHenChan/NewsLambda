// using trade_api.Contracts.Types;

namespace Options;

public sealed class DailyNewsOptions
{
  public required List<int> ReportHours { get; set; }
  public required string[] WorldRssList { get; set; }
  public required string[] MarketRssList { get; set; }
  public required int MinimumReportCount { get; set; }
  public required string TimeZone { get; set; }
}
