using System.Text.Json.Serialization;

namespace Contracts.Types;

public class Part
{
  [JsonPropertyName("text")]
  public string? Text { get; set; }
}

public class Content
{
  [JsonPropertyName("parts")]
  public List<Part>? Parts { get; set; }
}

public class ResponseSchemaProperty
{
  [JsonPropertyName("type")]
  public string? Type { get; set; }

  [JsonPropertyName("properties")]
  public Dictionary<string, ResponseSchemaProperty>? Properties { get; set; }

  [JsonPropertyName("items")]
  public ResponseSchemaProperty? Items { get; set; }

  [JsonPropertyName("propertyOrdering")]
  public List<string>? PropertyOrdering { get; set; }

  [JsonPropertyName("enum")]
  public List<string>? Enum { get; set; } // For string enums if needed
}

public class GenerationConfig
{
  [JsonPropertyName("responseMimeType")]
  public string? ResponseMimeType { get; set; }

  [JsonPropertyName("responseSchema")]
  public ResponseSchemaProperty? ResponseSchema { get; set; }
}

public class GeminiRequest
{
  [JsonPropertyName("contents")]
  public List<Content>? Contents { get; set; }

  [JsonPropertyName("generationConfig")]
  public GenerationConfig? GenerationConfig { get; set; }
}

public class GeminiResponseCandidate
{
  [JsonPropertyName("content")]
  public Content? Content { get; set; }
}

public class GeminiResponse
{
  [JsonPropertyName("candidates")]
  public List<GeminiResponseCandidate>? Candidates { get; set; }
}
