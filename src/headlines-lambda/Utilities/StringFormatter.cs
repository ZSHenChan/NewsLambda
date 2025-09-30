using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Utilities;

public static class StringFormatter
{
    public static string EscapeMarkdownV2(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        // List of special characters that need escaping in MarkdownV2
        // From Telegram Bot API documentation: https://core.telegram.org/bots/api#markdownv2-style
        char[] specialChars = new char[]
        {
            // '_',
            // '*',
            '[',
            ']',
            '(',
            ')',
            '~',
            '`',
            '<',
            '>',
            '#',
            '+',
            '-',
            '=',
            '|',
            '{',
            '}',
            '.',
            '!',
        };

        foreach (char c in specialChars)
        {
            text = text.Replace(c.ToString(), $"\\{c}");
        }

        return text;
    }

    public static List<string> SplitMessageByNewlines(string longMessage, int maxMessageLength)
    {
        List<string> messages = new List<string>();
        if (string.IsNullOrWhiteSpace(longMessage))
        {
            messages.Add("Message is empty.");
            return messages;
        }

        // Split the message into individual lines
        // StringSplitOptions.None keeps empty lines, which is often desired for formatting.
        string[] lines = longMessage.Split(new[] { '\n' }, StringSplitOptions.None);

        var currentMessageBuilder = new StringBuilder();

        foreach (string line in lines)
        {
            // Add the line itself, plus a newline character for all but the very last line of the original message
            // or if it's the start of a new message block.
            string lineToAdd = line;
            if (!line.Equals(lines.Last())) // Add newline unless it's the absolute last line
            {
                lineToAdd += "\n";
            }
            // For the last line, we don't add '\n' if the original message didn't end with it.
            // If the original message *did* end with '\n', the last line in `lines` would be an empty string,
            // which will correctly add a final newline.


            // Handle cases where a single line might exceed the limit
            if (lineToAdd.Length > maxMessageLength)
            {
                // If currentMessageBuilder has content, add it before splitting the long line
                if (currentMessageBuilder.Length > 0)
                {
                    messages.Add(currentMessageBuilder.ToString().TrimEnd('\n')); // Trim trailing newline before adding
                    currentMessageBuilder.Clear();
                }

                // Split the single long line into chunks
                for (int i = 0; i < lineToAdd.Length; i += maxMessageLength)
                {
                    string chunk = lineToAdd.Substring(
                        i,
                        Math.Min(maxMessageLength, lineToAdd.Length - i)
                    );
                    messages.Add(chunk);
                }
                continue; // Move to the next line from original input
            }

            // Check if adding the current line would exceed the limit of the current message
            if (
                currentMessageBuilder.Length > 0
                && (currentMessageBuilder.Length + lineToAdd.Length) > maxMessageLength
            )
            {
                // If it exceeds, add the current accumulated message to the list
                messages.Add(currentMessageBuilder.ToString().TrimEnd('\n')); // Trim trailing newline before adding
                // Start a new message with the current line
                currentMessageBuilder.Clear();
                currentMessageBuilder.Append(lineToAdd);
            }
            else
            {
                // Otherwise, append the line to the current message
                currentMessageBuilder.Append(lineToAdd);
            }
        }

        // Add the last accumulated message if it's not empty
        if (currentMessageBuilder.Length > 0)
        {
            messages.Add(currentMessageBuilder.ToString().TrimEnd('\n'));
        }

        return messages;
    }

    public static string[]? SplitCommand(string command)
    {
        var parts = command.Split(' ');
        if (parts.Length > 1)
        {
            return parts;
        }
        else
        {
            return null;
        }
    }

    public static string CleanHtmlText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        string decodedText = WebUtility.HtmlDecode(text);
        string cleanedText = decodedText.Replace('\u00A0', ' ');
        cleanedText = Regex.Replace(cleanedText, @"\s+", " ").Trim();
        return cleanedText;
    }
}
