using Amazon.Lambda.Core;
using Contracts.Interfaces;
using Contracts.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Options;
using Services;
using Utilities;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(
  typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer)
)]

namespace news_lambda;

public class Function
{
  private readonly IServiceProvider _serviceProvider;

  public Function()
  {
    var services = new ServiceCollection();

    services.AddLogging(builder => { });
    var configuration = new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
      .Build();

    services.Configure<NewsReportOptions>(configuration.GetSection("NewsReportOptions"));
    services.Configure<DailyNewsOptions>(configuration.GetSection("DailyNewsOptions"));
    services.Configure<TelegramOptions>(configuration.GetSection("TelegramOptions"));
    services.Configure<GeminiOptions>(configuration.GetSection("GeminiOptions"));

    services.AddSingleton<ITelegramService, TelegramService>();
    services.AddSingleton<GeminiService>();
    services.AddHttpClient<NewsService>();
    // Finally, build the service provider.
    _serviceProvider = services.BuildServiceProvider();
  }

  /// <summary>
  /// A simple function that takes a string and does a ToUpper
  /// </summary>
  /// <param name="input">The event for the Lambda function handler to process.</param>
  /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
  /// <returns></returns>
  public async Task FunctionHandler(Stream stream, ILambdaContext context)
  {
    NewsService newsService = _serviceProvider.GetRequiredService<NewsService>();
    await newsService.SendNews();
  }
}
