/*
 * Yuno Gasai 2 (C# Edition) - Utility Commands
 * Copyright (C) 2025 blubskye
 * SPDX-License-Identifier: AGPL-3.0-or-later
 */

using Discord;
using Discord.WebSocket;

namespace Yuno.Commands;

public class UtilityCommands
{
    private readonly YunoConfig _config;
    private readonly YunoDatabase _database;

    public UtilityCommands(YunoConfig config, YunoDatabase database)
    {
        _config = config;
        _database = database;
    }

    #region Ping

    public async Task PingAsync(SocketSlashCommand command)
    {
        await command.RespondAsync("💓 **Pong!**\nI'm always here for you~ 💕");
    }

    public async Task PingPrefixAsync(SocketUserMessage message)
    {
        await message.Channel.SendMessageAsync("💓 **Pong!**\nI'm always here for you~ 💕");
    }

    #endregion

    #region Help

    public async Task HelpAsync(SocketSlashCommand command)
    {
        var response =
            "💕 **Yuno's Commands** 💕\n" +
            "*\"Let me show you everything I can do for you~\"* 💗\n\n" +
            "**🔪 Moderation**\n" +
            "`/ban` - Ban a user\n" +
            "`/kick` - Kick a user\n" +
            "`/unban` - Unban a user\n" +
            "`/timeout` - Timeout a user\n" +
            "`/clean` - Delete messages\n" +
            "`/mod-stats` - View moderation stats\n\n" +
            "**⚙️ Utility**\n" +
            "`/ping` - Check latency\n" +
            "`/prefix` - Set server prefix\n" +
            "`/source` - View source code\n" +
            "`/help` - This menu\n\n" +
            "**✨ Leveling**\n" +
            "`/xp` - Check XP and level\n" +
            "`/leaderboard` - Server rankings\n\n" +
            "**🎱 Fun**\n" +
            "`/8ball` - Ask the magic 8-ball\n\n" +
            "💕 *Yuno is always watching over you~* 💕";

        await command.RespondAsync(response);
    }

    public async Task HelpPrefixAsync(SocketUserMessage message)
    {
        var guild = (message.Channel as SocketGuildChannel)?.Guild;
        var prefix = guild != null ? _database.GetPrefix(guild.Id, _config.DefaultPrefix) : _config.DefaultPrefix;

        var response =
            "💕 **Yuno's Commands** 💕\n" +
            "*\"Let me show you everything I can do for you~\"* 💗\n" +
            $"Prefix: `{prefix}`\n\n" +
            "**🔪 Moderation**\n" +
            "`ban` - Ban a user\n" +
            "`kick` - Kick a user\n" +
            "`unban` - Unban a user\n" +
            "`timeout` - Timeout a user\n" +
            "`clean` - Delete messages\n" +
            "`mod-stats` - View moderation stats\n\n" +
            "**⚙️ Utility**\n" +
            "`ping` - Check latency\n" +
            "`prefix` - Set server prefix\n" +
            "`source` - View source code\n" +
            "`help` - This menu\n\n" +
            "**✨ Leveling**\n" +
            "`xp` - Check XP and level\n" +
            "`leaderboard` - Server rankings\n\n" +
            "**🎱 Fun**\n" +
            "`8ball` - Ask the magic 8-ball\n\n" +
            "💕 *Yuno is always watching over you~* 💕";

        await message.Channel.SendMessageAsync(response);
    }

    #endregion

    #region Source

    public async Task SourceAsync(SocketSlashCommand command)
    {
        var response =
            "📜 **Source Code**\n" +
            "*\"I have nothing to hide from you~\"* 💕\n\n" +
            "**C# Version**: https://github.com/blubskye/yuno_csharp\n" +
            "**C Version**: https://github.com/blubskye/yuno_c\n" +
            "**PHP Version**: https://github.com/blubskye/yuno_php\n" +
            "**Go Version**: https://github.com/blubskye/yuno-go\n" +
            "**Rust Version**: https://github.com/blubskye/yuno_rust\n" +
            "**Original JS**: https://github.com/japaneseenrichmentorganization/Yuno-Gasai-2\n\n" +
            "Licensed under **AGPL-3.0** 💗";

        await command.RespondAsync(response);
    }

    public async Task SourcePrefixAsync(SocketUserMessage message)
    {
        var response =
            "📜 **Source Code**\n" +
            "*\"I have nothing to hide from you~\"* 💕\n\n" +
            "**C# Version**: https://github.com/blubskye/yuno_csharp\n" +
            "**C Version**: https://github.com/blubskye/yuno_c\n" +
            "**PHP Version**: https://github.com/blubskye/yuno_php\n" +
            "**Go Version**: https://github.com/blubskye/yuno-go\n" +
            "**Rust Version**: https://github.com/blubskye/yuno_rust\n" +
            "**Original JS**: https://github.com/japaneseenrichmentorganization/Yuno-Gasai-2\n\n" +
            "Licensed under **AGPL-3.0** 💗";

        await message.Channel.SendMessageAsync(response);
    }

    #endregion

    #region Prefix

    public async Task PrefixAsync(SocketSlashCommand command)
    {
        var newPrefix = command.Data.Options.FirstOrDefault(o => o.Name == "prefix")?.Value?.ToString();

        if (string.IsNullOrEmpty(newPrefix) || newPrefix.Length > 5)
        {
            await command.RespondAsync("💔 Prefix too long! Max 5 characters~", ephemeral: true);
            return;
        }

        var guild = (command.Channel as SocketGuildChannel)?.Guild;
        if (guild == null)
        {
            await command.RespondAsync("💔 This command can only be used in a server~", ephemeral: true);
            return;
        }

        _database.SetPrefix(guild.Id, newPrefix);
        await command.RespondAsync($"🔧 **Prefix Updated!**\nNew prefix is now: `{newPrefix}` 💕");
    }

    public async Task PrefixPrefixAsync(SocketUserMessage message, string args)
    {
        var guild = (message.Channel as SocketGuildChannel)?.Guild;
        if (guild == null) return;

        if (string.IsNullOrEmpty(args))
        {
            var currentPrefix = _database.GetPrefix(guild.Id, _config.DefaultPrefix);
            await message.Channel.SendMessageAsync($"💕 Current prefix: `{currentPrefix}`");
            return;
        }

        if (args.Length > 5)
        {
            await message.Channel.SendMessageAsync("💔 Prefix too long! Max 5 characters~");
            return;
        }

        _database.SetPrefix(guild.Id, args);
        await message.Channel.SendMessageAsync($"🔧 **Prefix Updated!**\nNew prefix is now: `{args}` 💕");
    }

    #endregion

    #region XP

    public async Task XpAsync(SocketSlashCommand command)
    {
        var guild = (command.Channel as SocketGuildChannel)?.Guild;
        if (guild == null)
        {
            await command.RespondAsync("💔 This command can only be used in a server~", ephemeral: true);
            return;
        }

        var targetUser = (SocketGuildUser?)command.Data.Options.FirstOrDefault(o => o.Name == "user")?.Value;
        var userId = targetUser?.Id ?? command.User.Id;

        var userXp = _database.GetUserXp(userId, guild.Id);
        var nextLevel = userXp.Level + 1;
        var xpForNext = nextLevel * nextLevel * 100;
        var progress = xpForNext > 0 ? (int)(userXp.Xp * 100 / xpForNext) : 0;

        await command.RespondAsync(
            $"✨ **XP Stats**\n" +
            $"<@{userId}>'s progress~ 💕\n\n" +
            $"**Level:** {userXp.Level}\n" +
            $"**XP:** {userXp.Xp}\n" +
            $"**Progress to Next:** {progress}%");
    }

    public async Task XpPrefixAsync(SocketUserMessage message, string args)
    {
        var guild = (message.Channel as SocketGuildChannel)?.Guild;
        if (guild == null) return;

        var userId = message.Author.Id;
        if (!string.IsNullOrEmpty(args))
        {
            var mentionedId = ParseUserMention(args);
            if (mentionedId != 0) userId = mentionedId;
        }

        var userXp = _database.GetUserXp(userId, guild.Id);
        var nextLevel = userXp.Level + 1;
        var xpForNext = nextLevel * nextLevel * 100;
        var progress = xpForNext > 0 ? (int)(userXp.Xp * 100 / xpForNext) : 0;

        await message.Channel.SendMessageAsync(
            $"✨ **XP Stats**\n" +
            $"<@{userId}>'s progress~ 💕\n\n" +
            $"**Level:** {userXp.Level}\n" +
            $"**XP:** {userXp.Xp}\n" +
            $"**Progress to Next:** {progress}%");
    }

    #endregion

    #region Leaderboard

    public async Task LeaderboardAsync(SocketSlashCommand command)
    {
        var guild = (command.Channel as SocketGuildChannel)?.Guild;
        if (guild == null)
        {
            await command.RespondAsync("💔 This command can only be used in a server~", ephemeral: true);
            return;
        }

        var topUsers = _database.GetLeaderboard(guild.Id, 10);

        var response = "🏆 **Server Leaderboard**\n*\"Look who's been the most active~\"* 💕\n\n";

        for (var i = 0; i < topUsers.Count; i++)
        {
            var medal = i switch
            {
                0 => "🥇",
                1 => "🥈",
                2 => "🥉",
                _ => ""
            };

            response += $"{medal} {i + 1}. <@{topUsers[i].UserId}> - Level {topUsers[i].Level} ({topUsers[i].Xp} XP)\n";
        }

        if (topUsers.Count == 0)
        {
            response += "No one has earned XP yet~";
        }

        await command.RespondAsync(response);
    }

    public async Task LeaderboardPrefixAsync(SocketUserMessage message)
    {
        var guild = (message.Channel as SocketGuildChannel)?.Guild;
        if (guild == null) return;

        var topUsers = _database.GetLeaderboard(guild.Id, 10);

        var response = "🏆 **Server Leaderboard**\n*\"Look who's been the most active~\"* 💕\n\n";

        for (var i = 0; i < topUsers.Count; i++)
        {
            var medal = i switch
            {
                0 => "🥇",
                1 => "🥈",
                2 => "🥉",
                _ => ""
            };

            response += $"{medal} {i + 1}. <@{topUsers[i].UserId}> - Level {topUsers[i].Level} ({topUsers[i].Xp} XP)\n";
        }

        if (topUsers.Count == 0)
        {
            response += "No one has earned XP yet~";
        }

        await message.Channel.SendMessageAsync(response);
    }

    #endregion

    private static ulong ParseUserMention(string mention)
    {
        if (mention.StartsWith("<@") && mention.EndsWith(">"))
        {
            var idStr = mention[2..^1];
            if (idStr.StartsWith("!"))
                idStr = idStr[1..];
            if (ulong.TryParse(idStr, out var id))
                return id;
        }

        if (ulong.TryParse(mention, out var rawId))
            return rawId;

        return 0;
    }
}
