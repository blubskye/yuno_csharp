/*
 * Yuno Gasai 2 (C# Edition) - Moderation Commands
 * Copyright (C) 2025 blubskye
 * SPDX-License-Identifier: AGPL-3.0-or-later
 */

using Discord;
using Discord.WebSocket;

namespace Yuno.Commands;

public class ModerationCommands
{
    private readonly YunoConfig _config;
    private readonly YunoDatabase _database;

    public ModerationCommands(YunoConfig config, YunoDatabase database)
    {
        _config = config;
        _database = database;
    }

    #region Ban

    public async Task BanAsync(SocketSlashCommand command)
    {
        var user = (SocketGuildUser?)command.Data.Options.FirstOrDefault(o => o.Name == "user")?.Value;
        var reason = command.Data.Options.FirstOrDefault(o => o.Name == "reason")?.Value?.ToString() ?? "No reason provided";

        if (user == null)
        {
            await command.RespondAsync("💔 Please specify a user to ban~", ephemeral: true);
            return;
        }

        var guild = (command.Channel as SocketGuildChannel)?.Guild;
        if (guild == null)
        {
            await command.RespondAsync("💔 This command can only be used in a server~", ephemeral: true);
            return;
        }

        try
        {
            await guild.AddBanAsync(user, reason: reason);

            _database.LogModAction(new ModAction
            {
                GuildId = guild.Id,
                ModeratorId = command.User.Id,
                TargetId = user.Id,
                ActionType = "ban",
                Reason = reason,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });

            await command.RespondAsync(
                $"🔪 **Banned!**\nThey won't bother you anymore~ 💕\n\n" +
                $"**User:** <@{user.Id}>\n" +
                $"**Moderator:** <@{command.User.Id}>\n" +
                $"**Reason:** {reason}");
        }
        catch (Exception ex)
        {
            await command.RespondAsync($"💔 Failed to ban: {ex.Message}", ephemeral: true);
        }
    }

    public async Task BanPrefixAsync(SocketUserMessage message, string args)
    {
        var guild = (message.Channel as SocketGuildChannel)?.Guild;
        if (guild == null) return;

        var parts = args.Split(' ', 2);
        if (parts.Length == 0 || string.IsNullOrEmpty(parts[0]))
        {
            await message.Channel.SendMessageAsync("💔 Please specify a user to ban~");
            return;
        }

        var userId = ParseUserMention(parts[0]);
        if (userId == 0)
        {
            await message.Channel.SendMessageAsync("💔 I couldn't find that user~");
            return;
        }

        var reason = parts.Length > 1 ? parts[1] : "No reason provided";

        try
        {
            await guild.AddBanAsync(userId, reason: reason);

            _database.LogModAction(new ModAction
            {
                GuildId = guild.Id,
                ModeratorId = message.Author.Id,
                TargetId = userId,
                ActionType = "ban",
                Reason = reason,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });

            await message.Channel.SendMessageAsync(
                $"🔪 **Banned!**\nThey won't bother you anymore~ 💕\n\n" +
                $"**User:** <@{userId}>\n" +
                $"**Moderator:** <@{message.Author.Id}>\n" +
                $"**Reason:** {reason}");
        }
        catch (Exception ex)
        {
            await message.Channel.SendMessageAsync($"💔 Failed to ban: {ex.Message}");
        }
    }

    #endregion

    #region Kick

    public async Task KickAsync(SocketSlashCommand command)
    {
        var user = (SocketGuildUser?)command.Data.Options.FirstOrDefault(o => o.Name == "user")?.Value;
        var reason = command.Data.Options.FirstOrDefault(o => o.Name == "reason")?.Value?.ToString() ?? "No reason provided";

        if (user == null)
        {
            await command.RespondAsync("💔 Please specify a user to kick~", ephemeral: true);
            return;
        }

        var guild = (command.Channel as SocketGuildChannel)?.Guild;
        if (guild == null)
        {
            await command.RespondAsync("💔 This command can only be used in a server~", ephemeral: true);
            return;
        }

        try
        {
            await user.KickAsync(reason);

            _database.LogModAction(new ModAction
            {
                GuildId = guild.Id,
                ModeratorId = command.User.Id,
                TargetId = user.Id,
                ActionType = "kick",
                Reason = reason,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });

            await command.RespondAsync(
                $"👢 **Kicked!**\nGet out! 💢\n\n" +
                $"**User:** <@{user.Id}>\n" +
                $"**Moderator:** <@{command.User.Id}>\n" +
                $"**Reason:** {reason}");
        }
        catch (Exception ex)
        {
            await command.RespondAsync($"💔 Failed to kick: {ex.Message}", ephemeral: true);
        }
    }

    public async Task KickPrefixAsync(SocketUserMessage message, string args)
    {
        var guild = (message.Channel as SocketGuildChannel)?.Guild;
        if (guild == null) return;

        var parts = args.Split(' ', 2);
        if (parts.Length == 0 || string.IsNullOrEmpty(parts[0]))
        {
            await message.Channel.SendMessageAsync("💔 Please specify a user to kick~");
            return;
        }

        var userId = ParseUserMention(parts[0]);
        var user = guild.GetUser(userId);
        if (user == null)
        {
            await message.Channel.SendMessageAsync("💔 I couldn't find that user~");
            return;
        }

        var reason = parts.Length > 1 ? parts[1] : "No reason provided";

        try
        {
            await user.KickAsync(reason);

            _database.LogModAction(new ModAction
            {
                GuildId = guild.Id,
                ModeratorId = message.Author.Id,
                TargetId = userId,
                ActionType = "kick",
                Reason = reason,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });

            await message.Channel.SendMessageAsync(
                $"👢 **Kicked!**\nGet out! 💢\n\n" +
                $"**User:** <@{userId}>\n" +
                $"**Moderator:** <@{message.Author.Id}>\n" +
                $"**Reason:** {reason}");
        }
        catch (Exception ex)
        {
            await message.Channel.SendMessageAsync($"💔 Failed to kick: {ex.Message}");
        }
    }

    #endregion

    #region Unban

    public async Task UnbanAsync(SocketSlashCommand command)
    {
        var userIdStr = command.Data.Options.FirstOrDefault(o => o.Name == "user_id")?.Value?.ToString();
        var reason = command.Data.Options.FirstOrDefault(o => o.Name == "reason")?.Value?.ToString() ?? "No reason provided";

        if (string.IsNullOrEmpty(userIdStr) || !ulong.TryParse(userIdStr, out var userId))
        {
            await command.RespondAsync("💔 Please specify a valid user ID to unban~", ephemeral: true);
            return;
        }

        var guild = (command.Channel as SocketGuildChannel)?.Guild;
        if (guild == null)
        {
            await command.RespondAsync("💔 This command can only be used in a server~", ephemeral: true);
            return;
        }

        try
        {
            await guild.RemoveBanAsync(userId);

            _database.LogModAction(new ModAction
            {
                GuildId = guild.Id,
                ModeratorId = command.User.Id,
                TargetId = userId,
                ActionType = "unban",
                Reason = reason,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });

            await command.RespondAsync(
                $"💕 **Unbanned!**\nI'm giving them another chance~ Be good this time!\n\n" +
                $"**User:** <@{userId}>\n" +
                $"**Moderator:** <@{command.User.Id}>\n" +
                $"**Reason:** {reason}");
        }
        catch (Exception ex)
        {
            await command.RespondAsync($"💔 Failed to unban: {ex.Message}", ephemeral: true);
        }
    }

    public async Task UnbanPrefixAsync(SocketUserMessage message, string args)
    {
        var guild = (message.Channel as SocketGuildChannel)?.Guild;
        if (guild == null) return;

        var parts = args.Split(' ', 2);
        if (parts.Length == 0 || !ulong.TryParse(parts[0], out var userId))
        {
            await message.Channel.SendMessageAsync("💔 Please specify a valid user ID to unban~");
            return;
        }

        var reason = parts.Length > 1 ? parts[1] : "No reason provided";

        try
        {
            await guild.RemoveBanAsync(userId);

            _database.LogModAction(new ModAction
            {
                GuildId = guild.Id,
                ModeratorId = message.Author.Id,
                TargetId = userId,
                ActionType = "unban",
                Reason = reason,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });

            await message.Channel.SendMessageAsync(
                $"💕 **Unbanned!**\nI'm giving them another chance~ Be good this time!\n\n" +
                $"**User:** <@{userId}>\n" +
                $"**Moderator:** <@{message.Author.Id}>\n" +
                $"**Reason:** {reason}");
        }
        catch (Exception ex)
        {
            await message.Channel.SendMessageAsync($"💔 Failed to unban: {ex.Message}");
        }
    }

    #endregion

    #region Timeout

    public async Task TimeoutAsync(SocketSlashCommand command)
    {
        var user = (SocketGuildUser?)command.Data.Options.FirstOrDefault(o => o.Name == "user")?.Value;
        var minutes = Convert.ToInt64(command.Data.Options.FirstOrDefault(o => o.Name == "minutes")?.Value ?? 5);
        var reason = command.Data.Options.FirstOrDefault(o => o.Name == "reason")?.Value?.ToString() ?? "No reason provided";

        if (user == null)
        {
            await command.RespondAsync("💔 Please specify a user to timeout~", ephemeral: true);
            return;
        }

        var guild = (command.Channel as SocketGuildChannel)?.Guild;
        if (guild == null)
        {
            await command.RespondAsync("💔 This command can only be used in a server~", ephemeral: true);
            return;
        }

        try
        {
            await user.SetTimeOutAsync(TimeSpan.FromMinutes(minutes));

            _database.LogModAction(new ModAction
            {
                GuildId = guild.Id,
                ModeratorId = command.User.Id,
                TargetId = user.Id,
                ActionType = "timeout",
                Reason = $"{reason} ({minutes} minutes)",
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });

            await command.RespondAsync(
                $"⏰ **Timed Out!**\nThink about what you did~ 😤\n\n" +
                $"**User:** <@{user.Id}>\n" +
                $"**Duration:** {minutes} minutes\n" +
                $"**Moderator:** <@{command.User.Id}>\n" +
                $"**Reason:** {reason}");
        }
        catch (Exception ex)
        {
            await command.RespondAsync($"💔 Failed to timeout: {ex.Message}", ephemeral: true);
        }
    }

    public async Task TimeoutPrefixAsync(SocketUserMessage message, string args)
    {
        var guild = (message.Channel as SocketGuildChannel)?.Guild;
        if (guild == null) return;

        var parts = args.Split(' ', 3);
        if (parts.Length < 2)
        {
            await message.Channel.SendMessageAsync("💔 Usage: timeout <user> <minutes> [reason]~");
            return;
        }

        var userId = ParseUserMention(parts[0]);
        var user = guild.GetUser(userId);
        if (user == null)
        {
            await message.Channel.SendMessageAsync("💔 I couldn't find that user~");
            return;
        }

        if (!int.TryParse(parts[1], out var minutes) || minutes <= 0)
        {
            await message.Channel.SendMessageAsync("💔 Invalid duration~");
            return;
        }

        var reason = parts.Length > 2 ? parts[2] : "No reason provided";

        try
        {
            await user.SetTimeOutAsync(TimeSpan.FromMinutes(minutes));

            _database.LogModAction(new ModAction
            {
                GuildId = guild.Id,
                ModeratorId = message.Author.Id,
                TargetId = userId,
                ActionType = "timeout",
                Reason = $"{reason} ({minutes} minutes)",
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });

            await message.Channel.SendMessageAsync(
                $"⏰ **Timed Out!**\nThink about what you did~ 😤\n\n" +
                $"**User:** <@{userId}>\n" +
                $"**Duration:** {minutes} minutes\n" +
                $"**Moderator:** <@{message.Author.Id}>**");
        }
        catch (Exception ex)
        {
            await message.Channel.SendMessageAsync($"💔 Failed to timeout: {ex.Message}");
        }
    }

    #endregion

    #region Clean

    public async Task CleanAsync(SocketSlashCommand command)
    {
        var count = Convert.ToInt32(command.Data.Options.FirstOrDefault(o => o.Name == "count")?.Value ?? 10);
        if (count > 100) count = 100;

        var channel = command.Channel as ITextChannel;
        if (channel == null)
        {
            await command.RespondAsync("💔 This command can only be used in a text channel~", ephemeral: true);
            return;
        }

        try
        {
            var messages = await channel.GetMessagesAsync(count + 1).FlattenAsync();
            var messagesToDelete = messages.Where(m => (DateTimeOffset.UtcNow - m.Timestamp).TotalDays < 14).ToList();

            await channel.DeleteMessagesAsync(messagesToDelete);

            await command.RespondAsync($"🧹 Deleted {messagesToDelete.Count - 1} messages~ 💕", ephemeral: true);
        }
        catch (Exception ex)
        {
            await command.RespondAsync($"💔 Failed to clean: {ex.Message}", ephemeral: true);
        }
    }

    public async Task CleanPrefixAsync(SocketUserMessage message, string args)
    {
        var channel = message.Channel as ITextChannel;
        if (channel == null) return;

        var count = 10;
        if (!string.IsNullOrEmpty(args) && int.TryParse(args, out var parsed))
        {
            count = Math.Min(parsed, 100);
        }

        try
        {
            var messages = await channel.GetMessagesAsync(count + 1).FlattenAsync();
            var messagesToDelete = messages.Where(m => (DateTimeOffset.UtcNow - m.Timestamp).TotalDays < 14).ToList();

            await channel.DeleteMessagesAsync(messagesToDelete);

            var confirmation = await message.Channel.SendMessageAsync($"🧹 Deleted {messagesToDelete.Count} messages~ 💕");
            await Task.Delay(3000);
            await confirmation.DeleteAsync();
        }
        catch (Exception ex)
        {
            await message.Channel.SendMessageAsync($"💔 Failed to clean: {ex.Message}");
        }
    }

    #endregion

    #region Mod Stats

    public async Task ModStatsAsync(SocketSlashCommand command)
    {
        var guild = (command.Channel as SocketGuildChannel)?.Guild;
        if (guild == null)
        {
            await command.RespondAsync("💔 This command can only be used in a server~", ephemeral: true);
            return;
        }

        var targetUser = (SocketGuildUser?)command.Data.Options.FirstOrDefault(o => o.Name == "user")?.Value;
        var userId = targetUser?.Id ?? command.User.Id;

        var (bans, kicks, timeouts) = _database.GetModStats(guild.Id, userId);
        var total = bans + kicks + timeouts;

        await command.RespondAsync(
            $"📊 **Moderation Statistics**\n" +
            $"Stats for <@{userId}>~ 💕\n\n" +
            $"**Total Actions:** {total}\n" +
            $"🔪 Bans: {bans}\n" +
            $"👢 Kicks: {kicks}\n" +
            $"⏰ Timeouts: {timeouts}");
    }

    public async Task ModStatsPrefixAsync(SocketUserMessage message, string args)
    {
        var guild = (message.Channel as SocketGuildChannel)?.Guild;
        if (guild == null) return;

        var userId = message.Author.Id;
        if (!string.IsNullOrEmpty(args))
        {
            userId = ParseUserMention(args);
            if (userId == 0) userId = message.Author.Id;
        }

        var (bans, kicks, timeouts) = _database.GetModStats(guild.Id, userId);
        var total = bans + kicks + timeouts;

        await message.Channel.SendMessageAsync(
            $"📊 **Moderation Statistics**\n" +
            $"Stats for <@{userId}>~ 💕\n\n" +
            $"**Total Actions:** {total}\n" +
            $"🔪 Bans: {bans}\n" +
            $"👢 Kicks: {kicks}\n" +
            $"⏰ Timeouts: {timeouts}");
    }

    #endregion

    private static ulong ParseUserMention(string mention)
    {
        // Handle <@123> or <@!123> format
        if (mention.StartsWith("<@") && mention.EndsWith(">"))
        {
            var idStr = mention[2..^1];
            if (idStr.StartsWith("!"))
                idStr = idStr[1..];
            if (ulong.TryParse(idStr, out var id))
                return id;
        }

        // Try parsing as raw ID
        if (ulong.TryParse(mention, out var rawId))
            return rawId;

        return 0;
    }
}
