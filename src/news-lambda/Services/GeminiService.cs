using System.Net.Http.Json;
using System.ServiceModel.Syndication;
using System.Text.Json;
using Contracts.Types;
using DotnetGeminiSDK.Client;
using DotnetGeminiSDK.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Options;
using DotnetSdkRes = DotnetGeminiSDK.Model.Response;

namespace Services;

public class GeminiService
{
  private readonly GeminiClient _geminiClient;
  private readonly HttpClient _httpClient;
  private readonly string _apiKey;
  private readonly ILogger<GeminiService> _logger;

  public GeminiService(IOptions<GeminiOptions> geminiOptions, ILogger<GeminiService> logger)
  {
    _logger = logger;
    _apiKey = geminiOptions.Value.ApiKey;
    _geminiClient = new GeminiClient(
      new GoogleGeminiConfig()
      {
        ApiKey = _apiKey,
        TextBaseUrl = $"{geminiOptions.Value.TextBaseUrl}{geminiOptions.Value.Model}",
      }
    );
    _httpClient = new() { BaseAddress = new Uri(geminiOptions.Value.BaseAddress) };
    _httpClient.DefaultRequestHeaders.Add(geminiOptions.Value.ApiKeyHeader, _apiKey);
  }

  public async Task<string?> GenerateResult(string prompt)
  {
    DotnetSdkRes.GeminiMessageResponse? response = await _geminiClient.TextPrompt(prompt);

    return response?.Candidates[0].Content.Parts[0].Text;
  }

  public async Task<List<NewsItem>?> CleanupNews(IEnumerable<SyndicationItem> rssFeedList)
  {
    string promptText =
      @$"Identify the same news talking about same topic and collect their links and publishers together under a single headline (use the one with more context without misleading wordings). Keep those not duplicate as standalone news.

Here are the news:
{string.Join("\n", rssFeedList.Select(item => @$"Headline: {item.Title?.Text}
Description:{item.Summary.Text}
Link:{item.Links.FirstOrDefault()?.Uri?.ToString()}"))}
";

    var request = new GeminiRequest
    {
      Contents = new List<Content>
      {
        new Content { Parts = new List<Part> { new Part { Text = promptText } } },
      },
      GenerationConfig = new GenerationConfig
      {
        ResponseMimeType = "application/json",
        ResponseSchema = new ResponseSchemaProperty
        {
          Type = "ARRAY", // We want an array of news items
          Items = new ResponseSchemaProperty // Each item in the array is an object
          {
            Type = "OBJECT",
            Properties = new Dictionary<string, ResponseSchemaProperty>
            {
              {
                "title",
                new ResponseSchemaProperty { Type = "STRING" }
              },
              {
                "summary",
                new ResponseSchemaProperty { Type = "STRING" }
              },
              {
                "links",
                new ResponseSchemaProperty
                {
                  Type = "ARRAY",
                  Items = new ResponseSchemaProperty
                  {
                    Type = "OBJECT",
                    Properties = new Dictionary<string, ResponseSchemaProperty>
                    {
                      {
                        "publisher",
                        new ResponseSchemaProperty { Type = "STRING" }
                      },
                      {
                        "link",
                        new ResponseSchemaProperty { Type = "STRING" }
                      },
                    },
                  },
                }
              },
            },
          },
        },
      },
    };

    try
    {
      var response = await _httpClient.PostAsJsonAsync(
        "v1beta/models/gemini-2.5-flash:generateContent",
        request
      );
      response.EnsureSuccessStatusCode(); // Throws an exception if the HTTP response status is an error code.

      var geminiResponse = await response.Content.ReadFromJsonAsync<GeminiResponse>();

      if (
        geminiResponse?.Candidates?.Count > 0
        && geminiResponse.Candidates[0]?.Content?.Parts?.Count > 0
      )
      {
        // The actual JSON output from Gemini will be within the 'text' property of the first part.
        // We need to parse that string as a JSON array of CookieRecipe.
        var jsonContentString = geminiResponse?.Candidates[0]?.Content?.Parts?[0].Text;
        if (jsonContentString == null)
          return null;
        return JsonSerializer.Deserialize<List<NewsItem>>(jsonContentString);
      }
      else
      {
        _logger.LogInformation("No candidates or content found in the Gemini response.");
        return new List<NewsItem>();
      }
    }
    catch (HttpRequestException e)
    {
      _logger.LogError($"Request error: {e.Message}");
      if (e.StatusCode.HasValue)
      {
        _logger.LogError($"Status Code: {e.StatusCode.Value}");
      }
      _logger.LogError($"Error Content: {e.Message}");
      return null;
    }
    catch (JsonException e)
    {
      Console.WriteLine($"JSON deserialization error: {e.Message}");
      Console.WriteLine($"Original JSON: {e.Path}"); // Path in JSON where error occurred
      return null;
    }
    catch (Exception e)
    {
      Console.WriteLine($"An unexpected error occurred: {e.Message}");
      return null;
    }
  }
}
