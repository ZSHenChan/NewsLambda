using Amazon.Lambda.Core;
using Contracts.Interfaces;
using Contracts.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Options;
using Serilog;
using Services;
using Utilities;

[assembly: LambdaSerializer(
  typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer)
)]

namespace headlines_lambda;

public class Function
{
  private readonly IServiceProvider _serviceProvider;

  public Function()
  {
    var services = new ServiceCollection();

    var configuration = new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
      .Build();
    services.AddSerilog(
      (services, lc) =>
        lc.ReadFrom.Configuration(configuration).ReadFrom.Services(services).Enrich.FromLogContext()
    );
    services.AddLogging(builder => { });

    services.Configure<NewsReportOptions>(configuration.GetSection("NewsReportOptions"));
    services.Configure<DailyNewsOptions>(configuration.GetSection("DailyNewsOptions"));
    services.Configure<TelegramOptions>(configuration.GetSection("TelegramOptions"));
    services.Configure<GeminiOptions>(configuration.GetSection("GeminiOptions"));

    services.AddSingleton<ITelegramService, TelegramService>();
    services.AddSingleton<GeminiService>();
    services.AddHttpClient<NewsService>();
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
    await newsService.FetchNews();
  }
}
