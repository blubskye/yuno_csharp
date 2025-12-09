/*
 * Yuno Gasai 2 (C# Edition)
 * "I'll protect this server forever... just for you~" <3
 *
 * Copyright (C) 2025 blubskye
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Yuno;

public class YunoConfig
{
    [JsonPropertyName("discord_token")]
    public string DiscordToken { get; set; } = "";

    [JsonPropertyName("default_prefix")]
    public string DefaultPrefix { get; set; } = ".";

    [JsonPropertyName("database_path")]
    public string DatabasePath { get; set; } = "yuno.db";

    [JsonPropertyName("master_users")]
    public List<string> MasterUsers { get; set; } = new();

    [JsonPropertyName("spam_max_warnings")]
    public int SpamMaxWarnings { get; set; } = 3;

    [JsonPropertyName("ban_default_image")]
    public string? BanDefaultImage { get; set; }

    [JsonPropertyName("dm_message")]
    public string DmMessage { get; set; } = "I'm just a bot :'(. I can't answer to you.";

    [JsonPropertyName("insufficient_permissions_message")]
    public string InsufficientPermissionsMessage { get; set; } = "${author} You don't have permission to do that~";

    public static YunoConfig Load(string path)
    {
        if (!File.Exists(path))
        {
            return LoadFromEnvironment();
        }

        var json = File.ReadAllText(path);
        var config = JsonSerializer.Deserialize<YunoConfig>(json) ?? new YunoConfig();

        // Override with environment variables if present
        var envToken = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
        if (!string.IsNullOrEmpty(envToken))
            config.DiscordToken = envToken;

        return config;
    }

    public static YunoConfig LoadFromEnvironment()
    {
        var config = new YunoConfig
        {
            DiscordToken = Environment.GetEnvironmentVariable("DISCORD_TOKEN") ?? "",
            DefaultPrefix = Environment.GetEnvironmentVariable("DEFAULT_PREFIX") ?? ".",
            DatabasePath = Environment.GetEnvironmentVariable("DATABASE_PATH") ?? "yuno.db",
            DmMessage = Environment.GetEnvironmentVariable("DM_MESSAGE") ?? "I'm just a bot :'(. I can't answer to you."
        };

        var spamWarnings = Environment.GetEnvironmentVariable("SPAM_MAX_WARNINGS");
        if (int.TryParse(spamWarnings, out var warnings))
            config.SpamMaxWarnings = warnings;

        var masterUser = Environment.GetEnvironmentVariable("MASTER_USER");
        if (!string.IsNullOrEmpty(masterUser))
            config.MasterUsers.Add(masterUser);

        return config;
    }

    public bool IsMasterUser(ulong userId)
    {
        return MasterUsers.Contains(userId.ToString());
    }
}
