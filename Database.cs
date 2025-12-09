/*
 * Yuno Gasai 2 (C# Edition) - Database
 * Copyright (C) 2025 blubskye
 * SPDX-License-Identifier: AGPL-3.0-or-later
 */

using Microsoft.Data.Sqlite;

namespace Yuno;

public class YunoDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public YunoDatabase(string path)
    {
        _connection = new SqliteConnection($"Data Source={path}");
        _connection.Open();
        Initialize();
    }

    private void Initialize()
    {
        ExecuteNonQuery(@"
            CREATE TABLE IF NOT EXISTS guild_settings (
                guild_id TEXT PRIMARY KEY,
                prefix TEXT DEFAULT '.',
                spam_filter_enabled INTEGER DEFAULT 0,
                leveling_enabled INTEGER DEFAULT 1
            )");

        ExecuteNonQuery(@"
            CREATE TABLE IF NOT EXISTS user_xp (
                user_id TEXT NOT NULL,
                guild_id TEXT NOT NULL,
                xp INTEGER DEFAULT 0,
                level INTEGER DEFAULT 0,
                PRIMARY KEY (user_id, guild_id)
            )");

        ExecuteNonQuery(@"
            CREATE TABLE IF NOT EXISTS mod_actions (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                guild_id TEXT NOT NULL,
                moderator_id TEXT NOT NULL,
                target_id TEXT NOT NULL,
                action_type TEXT NOT NULL,
                reason TEXT,
                timestamp INTEGER NOT NULL
            )");

        ExecuteNonQuery(@"
            CREATE TABLE IF NOT EXISTS auto_clean_config (
                guild_id TEXT NOT NULL,
                channel_id TEXT NOT NULL,
                interval_minutes INTEGER DEFAULT 60,
                message_count INTEGER DEFAULT 100,
                enabled INTEGER DEFAULT 1,
                PRIMARY KEY (guild_id, channel_id)
            )");

        ExecuteNonQuery(@"
            CREATE TABLE IF NOT EXISTS spam_warnings (
                user_id TEXT NOT NULL,
                guild_id TEXT NOT NULL,
                warnings INTEGER DEFAULT 0,
                last_warning INTEGER,
                PRIMARY KEY (user_id, guild_id)
            )");

        // Create indexes
        ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_mod_actions_guild ON mod_actions(guild_id)");
        ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_mod_actions_moderator ON mod_actions(moderator_id)");
        ExecuteNonQuery("CREATE INDEX IF NOT EXISTS idx_user_xp_guild ON user_xp(guild_id)");
    }

    private void ExecuteNonQuery(string sql)
    {
        using var cmd = new SqliteCommand(sql, _connection);
        cmd.ExecuteNonQuery();
    }

    #region Guild Settings

    public GuildSettings? GetGuildSettings(ulong guildId)
    {
        using var cmd = new SqliteCommand(
            "SELECT prefix, spam_filter_enabled, leveling_enabled FROM guild_settings WHERE guild_id = @guildId",
            _connection);
        cmd.Parameters.AddWithValue("@guildId", guildId.ToString());

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new GuildSettings
            {
                GuildId = guildId,
                Prefix = reader.GetString(0),
                SpamFilterEnabled = reader.GetInt32(1) == 1,
                LevelingEnabled = reader.GetInt32(2) == 1
            };
        }
        return null;
    }

    public void SetGuildSettings(GuildSettings settings)
    {
        using var cmd = new SqliteCommand(@"
            INSERT OR REPLACE INTO guild_settings (guild_id, prefix, spam_filter_enabled, leveling_enabled)
            VALUES (@guildId, @prefix, @spamFilter, @leveling)",
            _connection);
        cmd.Parameters.AddWithValue("@guildId", settings.GuildId.ToString());
        cmd.Parameters.AddWithValue("@prefix", settings.Prefix);
        cmd.Parameters.AddWithValue("@spamFilter", settings.SpamFilterEnabled ? 1 : 0);
        cmd.Parameters.AddWithValue("@leveling", settings.LevelingEnabled ? 1 : 0);
        cmd.ExecuteNonQuery();
    }

    public string GetPrefix(ulong guildId, string defaultPrefix)
    {
        var settings = GetGuildSettings(guildId);
        return settings?.Prefix ?? defaultPrefix;
    }

    public void SetPrefix(ulong guildId, string prefix)
    {
        var settings = GetGuildSettings(guildId) ?? new GuildSettings
        {
            GuildId = guildId,
            SpamFilterEnabled = false,
            LevelingEnabled = true
        };
        settings.Prefix = prefix;
        SetGuildSettings(settings);
    }

    #endregion

    #region User XP

    public UserXp GetUserXp(ulong userId, ulong guildId)
    {
        using var cmd = new SqliteCommand(
            "SELECT xp, level FROM user_xp WHERE user_id = @userId AND guild_id = @guildId",
            _connection);
        cmd.Parameters.AddWithValue("@userId", userId.ToString());
        cmd.Parameters.AddWithValue("@guildId", guildId.ToString());

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new UserXp
            {
                UserId = userId,
                GuildId = guildId,
                Xp = reader.GetInt64(0),
                Level = reader.GetInt32(1)
            };
        }
        return new UserXp { UserId = userId, GuildId = guildId, Xp = 0, Level = 0 };
    }

    public void AddXp(ulong userId, ulong guildId, long amount)
    {
        using var cmd = new SqliteCommand(@"
            INSERT INTO user_xp (user_id, guild_id, xp, level)
            VALUES (@userId, @guildId, @amount, 0)
            ON CONFLICT(user_id, guild_id) DO UPDATE SET xp = xp + @amount",
            _connection);
        cmd.Parameters.AddWithValue("@userId", userId.ToString());
        cmd.Parameters.AddWithValue("@guildId", guildId.ToString());
        cmd.Parameters.AddWithValue("@amount", amount);
        cmd.ExecuteNonQuery();
    }

    public void SetLevel(ulong userId, ulong guildId, int level)
    {
        using var cmd = new SqliteCommand(
            "UPDATE user_xp SET level = @level WHERE user_id = @userId AND guild_id = @guildId",
            _connection);
        cmd.Parameters.AddWithValue("@level", level);
        cmd.Parameters.AddWithValue("@userId", userId.ToString());
        cmd.Parameters.AddWithValue("@guildId", guildId.ToString());
        cmd.ExecuteNonQuery();
    }

    public List<UserXp> GetLeaderboard(ulong guildId, int limit = 10)
    {
        var results = new List<UserXp>();
        using var cmd = new SqliteCommand(
            "SELECT user_id, xp, level FROM user_xp WHERE guild_id = @guildId ORDER BY xp DESC LIMIT @limit",
            _connection);
        cmd.Parameters.AddWithValue("@guildId", guildId.ToString());
        cmd.Parameters.AddWithValue("@limit", limit);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            results.Add(new UserXp
            {
                UserId = ulong.Parse(reader.GetString(0)),
                GuildId = guildId,
                Xp = reader.GetInt64(1),
                Level = reader.GetInt32(2)
            });
        }
        return results;
    }

    #endregion

    #region Mod Actions

    public void LogModAction(ModAction action)
    {
        using var cmd = new SqliteCommand(@"
            INSERT INTO mod_actions (guild_id, moderator_id, target_id, action_type, reason, timestamp)
            VALUES (@guildId, @modId, @targetId, @actionType, @reason, @timestamp)",
            _connection);
        cmd.Parameters.AddWithValue("@guildId", action.GuildId.ToString());
        cmd.Parameters.AddWithValue("@modId", action.ModeratorId.ToString());
        cmd.Parameters.AddWithValue("@targetId", action.TargetId.ToString());
        cmd.Parameters.AddWithValue("@actionType", action.ActionType);
        cmd.Parameters.AddWithValue("@reason", action.Reason ?? "No reason provided");
        cmd.Parameters.AddWithValue("@timestamp", action.Timestamp);
        cmd.ExecuteNonQuery();
    }

    public List<ModAction> GetModActions(ulong guildId, int limit = 100)
    {
        var results = new List<ModAction>();
        using var cmd = new SqliteCommand(@"
            SELECT id, moderator_id, target_id, action_type, reason, timestamp
            FROM mod_actions WHERE guild_id = @guildId ORDER BY timestamp DESC LIMIT @limit",
            _connection);
        cmd.Parameters.AddWithValue("@guildId", guildId.ToString());
        cmd.Parameters.AddWithValue("@limit", limit);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            results.Add(new ModAction
            {
                Id = reader.GetInt64(0),
                GuildId = guildId,
                ModeratorId = ulong.Parse(reader.GetString(1)),
                TargetId = ulong.Parse(reader.GetString(2)),
                ActionType = reader.GetString(3),
                Reason = reader.IsDBNull(4) ? null : reader.GetString(4),
                Timestamp = reader.GetInt64(5)
            });
        }
        return results;
    }

    public (int bans, int kicks, int timeouts) GetModStats(ulong guildId, ulong moderatorId)
    {
        int bans = 0, kicks = 0, timeouts = 0;
        using var cmd = new SqliteCommand(@"
            SELECT action_type, COUNT(*) FROM mod_actions
            WHERE guild_id = @guildId AND moderator_id = @modId
            GROUP BY action_type",
            _connection);
        cmd.Parameters.AddWithValue("@guildId", guildId.ToString());
        cmd.Parameters.AddWithValue("@modId", moderatorId.ToString());

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var type = reader.GetString(0);
            var count = reader.GetInt32(1);
            switch (type)
            {
                case "ban": bans = count; break;
                case "kick": kicks = count; break;
                case "timeout": timeouts = count; break;
            }
        }
        return (bans, kicks, timeouts);
    }

    #endregion

    #region Spam Warnings

    public int AddSpamWarning(ulong userId, ulong guildId)
    {
        using var cmd = new SqliteCommand(@"
            INSERT INTO spam_warnings (user_id, guild_id, warnings, last_warning)
            VALUES (@userId, @guildId, 1, @timestamp)
            ON CONFLICT(user_id, guild_id) DO UPDATE SET warnings = warnings + 1, last_warning = @timestamp",
            _connection);
        cmd.Parameters.AddWithValue("@userId", userId.ToString());
        cmd.Parameters.AddWithValue("@guildId", guildId.ToString());
        cmd.Parameters.AddWithValue("@timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        cmd.ExecuteNonQuery();

        return GetSpamWarnings(userId, guildId);
    }

    public int GetSpamWarnings(ulong userId, ulong guildId)
    {
        using var cmd = new SqliteCommand(
            "SELECT warnings FROM spam_warnings WHERE user_id = @userId AND guild_id = @guildId",
            _connection);
        cmd.Parameters.AddWithValue("@userId", userId.ToString());
        cmd.Parameters.AddWithValue("@guildId", guildId.ToString());

        var result = cmd.ExecuteScalar();
        return result != null ? Convert.ToInt32(result) : 0;
    }

    public void ResetSpamWarnings(ulong userId, ulong guildId)
    {
        using var cmd = new SqliteCommand(
            "DELETE FROM spam_warnings WHERE user_id = @userId AND guild_id = @guildId",
            _connection);
        cmd.Parameters.AddWithValue("@userId", userId.ToString());
        cmd.Parameters.AddWithValue("@guildId", guildId.ToString());
        cmd.ExecuteNonQuery();
    }

    #endregion

    public void Dispose()
    {
        _connection?.Dispose();
    }
}

#region Models

public class GuildSettings
{
    public ulong GuildId { get; set; }
    public string Prefix { get; set; } = ".";
    public bool SpamFilterEnabled { get; set; }
    public bool LevelingEnabled { get; set; } = true;
}

public class UserXp
{
    public ulong UserId { get; set; }
    public ulong GuildId { get; set; }
    public long Xp { get; set; }
    public int Level { get; set; }
}

public class ModAction
{
    public long Id { get; set; }
    public ulong GuildId { get; set; }
    public ulong ModeratorId { get; set; }
    public ulong TargetId { get; set; }
    public string ActionType { get; set; } = "";
    public string? Reason { get; set; }
    public long Timestamp { get; set; }
}

#endregion
