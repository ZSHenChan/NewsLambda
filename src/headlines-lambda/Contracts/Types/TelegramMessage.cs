using Telegram.Bot.Types.ReplyMarkups;

namespace Contracts.Types;

public class TelegramMessage
{
    public required string ChatId { get; set; }
    public required string Body { get; set; }
    public ReplyMarkup? ReplyMarkup { get; set; } = null;
}
