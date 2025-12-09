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
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>.
 */

namespace Yuno;

class Program
{
    private static YunoBot? _bot;

    static async Task Main(string[] args)
    {
        PrintBanner();

        Console.CancelKeyPress += OnCancelKeyPress;
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

        // Determine config path
        var configPath = "config.json";
        if (args.Length > 0)
        {
            configPath = args[0];
        }
        else
        {
            var envPath = Environment.GetEnvironmentVariable("CONFIG_PATH");
            if (!string.IsNullOrEmpty(envPath))
                configPath = envPath;
        }

        // Load configuration
        YunoConfig config;
        try
        {
            config = YunoConfig.Load(configPath);
            if (File.Exists(configPath))
                Console.WriteLine($"💖 Loaded config from {configPath}~");
            else
                Console.WriteLine("📝 Config file not found, using environment variables~");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to load configuration: {ex.Message}");
            return;
        }

        // Validate token
        if (string.IsNullOrEmpty(config.DiscordToken) || config.DiscordToken == "YOUR_DISCORD_BOT_TOKEN_HERE")
        {
            Console.WriteLine("❌ Error: No valid Discord token provided!");
            Console.WriteLine("Set DISCORD_TOKEN environment variable or add it to config.json");
            return;
        }

        // Initialize and run bot
        Console.WriteLine("💕 Yuno is waking up... please wait~");

        try
        {
            _bot = new YunoBot(config);
            await _bot.StartAsync();

            // Keep the application running
            await Task.Delay(Timeout.Infinite);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💔 Fatal error: {ex.Message}");
        }
        finally
        {
            _bot?.Dispose();
            Console.WriteLine("💔 Yuno has gone to sleep... see you next time~ 💔");
        }
    }

    private static void PrintBanner()
    {
        Console.WriteLine();
        Console.WriteLine("    💕 ╔═══════════════════════════════════════════╗ 💕");
        Console.WriteLine("       ║     Yuno Gasai 2 (C# Edition)             ║");
        Console.WriteLine("       ║     \"I'll protect you forever~\" 💗        ║");
        Console.WriteLine("       ╚═══════════════════════════════════════════╝");
        Console.WriteLine();
    }

    private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        Console.WriteLine("\n💔 Yuno is shutting down... goodbye, my love~ 💔");
        _bot?.StopAsync().GetAwaiter().GetResult();
        Environment.Exit(0);
    }

    private static void OnProcessExit(object? sender, EventArgs e)
    {
        _bot?.Dispose();
    }
}
