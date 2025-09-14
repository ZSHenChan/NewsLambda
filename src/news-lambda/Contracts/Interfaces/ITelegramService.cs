using Telegram.Bot.Types.ReplyMarkups;

namespace Contracts.Interfaces;

public interface ITelegramService
{
    Task SendTextMessageAsync(
        string message,
        string? chatId = null,
        ReplyMarkup? replyMarkup = null,
        bool forReal = false
    );
}
