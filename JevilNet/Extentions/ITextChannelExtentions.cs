using Discord;

namespace JevilNet.Extentions;

public static class ITextChannelExtentions
{
    public static async Task SendBlockAsync(this ITextChannel channel, string message)
    {
        List<string> messages = message.SplitInParts(1980).ToList();
        messages[0] = "```\n" + messages[0];
        messages[messages.Count - 1] += "\n```";
        
        foreach (var s in messages)
        {
            await channel.SendMessageAsync(s, allowedMentions: AllowedMentions.None);
        }
    }
}