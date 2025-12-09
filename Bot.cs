/*
 * Yuno Gasai 2 (C# Edition) - Bot Core
 * Copyright (C) 2025 blubskye
 * SPDX-License-Identifier: AGPL-3.0-or-later
 */

using Discord;
using Discord.WebSocket;
using Yuno.Commands;

namespace Yuno;

public class YunoBot : IDisposable
{
    private readonly DiscordSocketClient _client;
    private readonly YunoConfig _config;
    private readonly YunoDatabase _database;
    private readonly CommandHandler _commandHandler;
    private readonly Random _random = new();

    public YunoBot(YunoConfig config)
    {
        _config = config;
        _database = new YunoDatabase(config.DatabasePath);

        var socketConfig = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds |
                            GatewayIntents.GuildMessages |
                            GatewayIntents.GuildMembers |
                            GatewayIntents.MessageContent |
                            GatewayIntents.DirectMessages
        };

        _client = new DiscordSocketClient(socketConfig);
        _commandHandler = new CommandHandler(_client, _config, _database);

        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;
        _client.MessageReceived += MessageReceivedAsync;
        _client.SlashCommandExecuted += SlashCommandExecutedAsync;
    }

    public async Task StartAsync()
    {
        await _client.LoginAsync(TokenType.Bot, _config.DiscordToken);
        await _client.StartAsync();
    }

    public async Task StopAsync()
    {
        await _client.StopAsync();
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine($"[{log.Severity}] {log.Message}");
        if (log.Exception != null)
            Console.WriteLine(log.Exception);
        return Task.CompletedTask;
    }

    private async Task ReadyAsync()
    {
        Console.WriteLine($"💕 Yuno is online! Logged in as {_client.CurrentUser.Username}~ 💕");
        Console.WriteLine("💗 I'm watching over your servers for you~ 💗");

        await _client.SetGameAsync("over you~ 💕", type: ActivityType.Watching);
        await RegisterSlashCommandsAsync();
    }

    private async Task RegisterSlashCommandsAsync()
    {
        Console.WriteLine("💕 Registering slash commands~");

        var commands = new List<SlashCommandBuilder>
        {
            // Utility
            new SlashCommandBuilder()
                .WithName("ping")
                .WithDescription("Check if Yuno is awake~ 💓"),

            new SlashCommandBuilder()
                .WithName("help")
                .WithDescription("See what Yuno can do for you~ 💕"),

            new SlashCommandBuilder()
                .WithName("source")
                .WithDescription("See Yuno's source code~ 📜"),

            new SlashCommandBuilder()
                .WithName("prefix")
                .WithDescription("Set the server prefix~")
                .AddOption("prefix", ApplicationCommandOptionType.String, "The new prefix", isRequired: true),

            // Moderation
            new SlashCommandBuilder()
                .WithName("ban")
                .WithDescription("Ban a user from the server~ 🔪")
                .AddOption("user", ApplicationCommandOptionType.User, "The user to ban", isRequired: true)
                .AddOption("reason", ApplicationCommandOptionType.String, "Reason for the ban", isRequired: false),

            new SlashCommandBuilder()
                .WithName("kick")
                .WithDescription("Kick a user from the server~ 👢")
                .AddOption("user", ApplicationCommandOptionType.User, "The user to kick", isRequired: true)
                .AddOption("reason", ApplicationCommandOptionType.String, "Reason for the kick", isRequired: false),

            new SlashCommandBuilder()
                .WithName("unban")
                .WithDescription("Unban a user~ 💕")
                .AddOption("user_id", ApplicationCommandOptionType.String, "The user ID to unban", isRequired: true)
                .AddOption("reason", ApplicationCommandOptionType.String, "Reason for the unban", isRequired: false),

            new SlashCommandBuilder()
                .WithName("timeout")
                .WithDescription("Timeout a user~ ⏰")
                .AddOption("user", ApplicationCommandOptionType.User, "The user to timeout", isRequired: true)
                .AddOption("minutes", ApplicationCommandOptionType.Integer, "Duration in minutes", isRequired: true)
                .AddOption("reason", ApplicationCommandOptionType.String, "Reason for the timeout", isRequired: false),

            new SlashCommandBuilder()
                .WithName("clean")
                .WithDescription("Delete messages from the channel~ 🧹")
                .AddOption("count", ApplicationCommandOptionType.Integer, "Number of messages to delete", isRequired: false),

            new SlashCommandBuilder()
                .WithName("mod-stats")
                .WithDescription("View moderation statistics~ 📊")
                .AddOption("user", ApplicationCommandOptionType.User, "The moderator to check", isRequired: false),

            // XP
            new SlashCommandBuilder()
                .WithName("xp")
                .WithDescription("Check XP and level~ ✨")
                .AddOption("user", ApplicationCommandOptionType.User, "The user to check", isRequired: false),

            new SlashCommandBuilder()
                .WithName("leaderboard")
                .WithDescription("View the server leaderboard~ 🏆"),

            // Fun
            new SlashCommandBuilder()
                .WithName("8ball")
                .WithDescription("Ask the magic 8-ball~ 🎱")
                .AddOption("question", ApplicationCommandOptionType.String, "Your question", isRequired: true)
        };

        try
        {
            foreach (var command in commands)
            {
                await _client.CreateGlobalApplicationCommandAsync(command.Build());
            }
            Console.WriteLine($"💕 Registered {commands.Count} slash commands~");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💔 Failed to register slash commands: {ex.Message}");
        }
    }

    private async Task MessageReceivedAsync(SocketMessage rawMessage)
    {
        if (rawMessage is not SocketUserMessage message) return;
        if (message.Author.IsBot) return;

        // Handle DMs
        if (message.Channel is IDMChannel)
        {
            await message.Channel.SendMessageAsync(_config.DmMessage);
            return;
        }

        if (message.Channel is not SocketTextChannel textChannel) return;
        var guildId = textChannel.Guild.Id;

        var prefix = _database.GetPrefix(guildId, _config.DefaultPrefix);
        var argPos = 0;

        if (message.HasStringPrefix(prefix, ref argPos))
        {
            await _commandHandler.HandlePrefixCommandAsync(message, argPos);
        }
        else
        {
            // Add XP for chatting
            var settings = _database.GetGuildSettings(guildId);
            if (settings?.LevelingEnabled ?? true)
            {
                var xpGain = _random.Next(15, 26);
                _database.AddXp(message.Author.Id, guildId, xpGain);

                var userXp = _database.GetUserXp(message.Author.Id, guildId);
                var newLevel = (int)Math.Sqrt(userXp.Xp / 100.0);

                if (newLevel > userXp.Level)
                {
                    _database.SetLevel(message.Author.Id, guildId, newLevel);
                    await message.Channel.SendMessageAsync(
                        $"✨ **Level Up!** ✨\nCongratulations <@{message.Author.Id}>! You've reached level **{newLevel}**! 💕");
                }
            }
        }
    }

    private async Task SlashCommandExecutedAsync(SocketSlashCommand command)
    {
        await _commandHandler.HandleSlashCommandAsync(command);
    }

    public void Dispose()
    {
        _client?.Dispose();
        _database?.Dispose();
    }
}
