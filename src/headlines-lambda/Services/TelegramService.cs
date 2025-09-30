using Contracts.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Utilities;

namespace Services;

public class TelegramService : ITelegramService
{
  private readonly TelegramBotClient _botClient;
  private readonly ILogger<TelegramService> _logger;
  private readonly int _maxCharPerMessage;
  private readonly IOptions<TelegramOptions> _telegramOptions;

  public TelegramService(IOptions<TelegramOptions> telegramOptions, ILogger<TelegramService> logger)
  {
    _logger = logger;
    using var cts = new CancellationTokenSource();
    _botClient = new TelegramBotClient(telegramOptions.Value.ApiKey, cancellationToken: cts.Token);
    _maxCharPerMessage = telegramOptions.Value.MaxCharPerMessage;
    _telegramOptions = telegramOptions;
  }

  public async Task SendTextMessageAsync(
    string message,
    string? chatId = null,
    ReplyMarkup? replyMarkup = null,
    bool forReal = false
  )
  {
    chatId ??= _telegramOptions.Value.ChatIdTest;
    if (forReal)
      chatId = _telegramOptions.Value.ChatIdNews;
    try
    {
      if (message.Length < _maxCharPerMessage)
      {
        await _botClient.SendMessage(
          chatId: chatId,
          text: message,
          parseMode: ParseMode.MarkdownV2,
          replyMarkup: replyMarkup
        );
      }
      else
      {
        List<string> messageList = StringFormatter.SplitMessageByNewlines(
          longMessage: message,
          maxMessageLength: _maxCharPerMessage
        );
        for (int i = 0; i < messageList.Count - 1; i++)
        {
          await _botClient.SendMessage(
            chatId: chatId,
            text: messageList[i],
            parseMode: ParseMode.MarkdownV2,
            replyMarkup: null
          );
        }
        await _botClient.SendMessage(
          chatId: chatId,
          text: messageList[^1],
          parseMode: ParseMode.MarkdownV2,
          replyMarkup: replyMarkup
        );
      }
      _logger.LogInformation($"Telegram message sent to {chatId}.");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, $"An error occurred while sending Telegram message to {chatId}.");
      await _botClient.SendMessage(
        chatId: chatId,
        text: "Something went wrong while sending message"
      );
    }
  }
}
