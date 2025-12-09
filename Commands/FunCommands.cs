/*
 * Yuno Gasai 2 (C# Edition) - Fun Commands
 * Copyright (C) 2025 blubskye
 * SPDX-License-Identifier: AGPL-3.0-or-later
 */

using Discord.WebSocket;

namespace Yuno.Commands;

public class FunCommands
{
    private static readonly string[] EightBallResponses =
    {
        // Positive
        "It is certain~ 💕",
        "It is decidedly so~ 💗",
        "Without a doubt~ 💖",
        "Yes, definitely~ 💕",
        "You may rely on it~ 💗",
        "As I see it, yes~ ✨",
        "Most likely~ 💕",
        "Outlook good~ 💖",
        "Yes~ 💗",
        "Signs point to yes~ ✨",

        // Neutral
        "Reply hazy, try again~ 🤔",
        "Ask again later~ 💭",
        "Better not tell you now~ 😏",
        "Cannot predict now~ 🔮",
        "Concentrate and ask again~ 💫",

        // Negative
        "Don't count on it~ 💔",
        "My reply is no~ 😤",
        "My sources say no~ 💢",
        "Outlook not so good~ 😞",
        "Very doubtful~ 💔"
    };

    private readonly Random _random = new();

    public async Task EightBallAsync(SocketSlashCommand command)
    {
        var question = command.Data.Options.FirstOrDefault(o => o.Name == "question")?.Value?.ToString() ?? "...";
        var response = EightBallResponses[_random.Next(EightBallResponses.Length)];

        await command.RespondAsync(
            $"🎱 **Magic 8-Ball**\n\n" +
            $"**Question:** {question}\n\n" +
            $"**Answer:** {response}\n\n" +
            "*shakes the 8-ball mysteriously*");
    }

    public async Task EightBallPrefixAsync(SocketUserMessage message, string args)
    {
        if (string.IsNullOrEmpty(args))
        {
            await message.Channel.SendMessageAsync("💔 You need to ask a question~ 🎱");
            return;
        }

        var response = EightBallResponses[_random.Next(EightBallResponses.Length)];

        await message.Channel.SendMessageAsync(
            $"🎱 **Magic 8-Ball**\n\n" +
            $"**Question:** {args}\n\n" +
            $"**Answer:** {response}\n\n" +
            "*shakes the 8-ball mysteriously*");
    }
}
