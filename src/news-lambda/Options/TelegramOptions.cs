namespace Options;

public sealed class TelegramOptions
{
  public required string ApiKey { get; set; }
  public required int MaxCharPerMessage { get; set; }
  public required int WaitTimeLimitMs { get; set; }
  public required string ChatIdTest { get; set; }
  public required string ChatIdNews { get; set; }
}
