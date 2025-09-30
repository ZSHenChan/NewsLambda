namespace Options;

public sealed class GeminiOptions
{
    public required string ApiKey { get; set; }
    public required string ApiKeyHeader { get; set; }
    public required string TextBaseUrl { get; set; }
    public required string Model { get; set; }
    public required string BaseAddress { get; set; }
}
