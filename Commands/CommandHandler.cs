/*
 * Yuno Gasai 2 (C# Edition) - Command Handler
 * Copyright (C) 2025 blubskye
 * SPDX-License-Identifier: AGPL-3.0-or-later
 */

using Discord;
using Discord.WebSocket;

namespace Yuno.Commands;

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly YunoConfig _config;
    private readonly YunoDatabase _database;

    private readonly ModerationCommands _moderation;
    private readonly UtilityCommands _utility;
    private readonly FunCommands _fun;

    public CommandHandler(DiscordSocketClient client, YunoConfig config, YunoDatabase database)
    {
        _client = client;
        _config = config;
        _database = database;

        _moderation = new ModerationCommands(config, database);
        _utility = new UtilityCommands(config, database);
        _fun = new FunCommands();
    }

    public async Task HandleSlashCommandAsync(SocketSlashCommand command)
    {
        try
        {
            switch (command.CommandName)
            {
                // Utility
                case "ping":
                    await _utility.PingAsync(command);
                    break;
                case "help":
                    await _utility.HelpAsync(command);
                    break;
                case "source":
                    await _utility.SourceAsync(command);
                    break;
                case "prefix":
                    await _utility.PrefixAsync(command);
                    break;
                case "xp":
                    await _utility.XpAsync(command);
                    break;
                case "leaderboard":
                    await _utility.LeaderboardAsync(command);
                    break;

                // Moderation
                case "ban":
                    await _moderation.BanAsync(command);
                    break;
                case "kick":
                    await _moderation.KickAsync(command);
                    break;
                case "unban":
                    await _moderation.UnbanAsync(command);
                    break;
                case "timeout":
                    await _moderation.TimeoutAsync(command);
                    break;
                case "clean":
                    await _moderation.CleanAsync(command);
                    break;
                case "mod-stats":
                    await _moderation.ModStatsAsync(command);
                    break;

                // Fun
                case "8ball":
                    await _fun.EightBallAsync(command);
                    break;

                default:
                    await command.RespondAsync("💔 Unknown command~", ephemeral: true);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💔 Error handling slash command: {ex.Message}");
            try
            {
                await command.RespondAsync("💔 Something went wrong~", ephemeral: true);
            }
            catch { }
        }
    }

    public async Task HandlePrefixCommandAsync(SocketUserMessage message, int argPos)
    {
        var content = message.Content[argPos..].Trim();
        var parts = content.Split(' ', 2);
        var commandName = parts[0].ToLowerInvariant();
        var args = parts.Length > 1 ? parts[1] : "";

        try
        {
            switch (commandName)
            {
                // Utility
                case "ping":
                    await _utility.PingPrefixAsync(message);
                    break;
                case "help":
                    await _utility.HelpPrefixAsync(message);
                    break;
                case "source":
                    await _utility.SourcePrefixAsync(message);
                    break;
                case "prefix":
                    await _utility.PrefixPrefixAsync(message, args);
                    break;
                case "xp":
                case "level":
                case "rank":
                    await _utility.XpPrefixAsync(message, args);
                    break;
                case "leaderboard":
                case "lb":
                case "top":
                    await _utility.LeaderboardPrefixAsync(message);
                    break;

                // Moderation
                case "ban":
                    await _moderation.BanPrefixAsync(message, args);
                    break;
                case "kick":
                    await _moderation.KickPrefixAsync(message, args);
                    break;
                case "unban":
                    await _moderation.UnbanPrefixAsync(message, args);
                    break;
                case "timeout":
                    await _moderation.TimeoutPrefixAsync(message, args);
                    break;
                case "clean":
                    await _moderation.CleanPrefixAsync(message, args);
                    break;
                case "mod-stats":
                case "modstats":
                    await _moderation.ModStatsPrefixAsync(message, args);
                    break;

                // Fun
                case "8ball":
                    await _fun.EightBallPrefixAsync(message, args);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💔 Error handling prefix command: {ex.Message}");
        }
    }
}
