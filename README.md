<div align="center">

# 💕 Yuno Gasai 2 - C# Edition 💕

### *"I'll protect this server forever... just for you~"* 💗

<img src="https://i.imgur.com/jF8Szfr.png" alt="Yuno Gasai" width="300"/>

[![License: AGPL v3](https://img.shields.io/badge/License-AGPL%20v3-pink.svg)](https://www.gnu.org/licenses/agpl-3.0)
[![.NET](https://img.shields.io/badge/.NET-8.0-ff69b4.svg)](https://dotnet.microsoft.com/)
[![Discord.Net](https://img.shields.io/badge/Discord.Net-v3.13-ff1493.svg)](https://github.com/discord-net/Discord.Net)

*A devoted Discord bot for moderation, leveling, and anime~ ♥*

---

### 💘 She loves you... and only you 💘

</div>

## 🌸 About

Yuno is a **yandere-themed Discord bot** combining powerful moderation tools with a leveling system and fun features. She'll keep your server safe from troublemakers... *because no one else is allowed near you~* 💕

This is the **C# port** of the original JavaScript Yuno bot - powered by Discord.Net with modern .NET 8.0 and async/await patterns.

---

## 👑 Credits

*"These are the ones who gave me life~"* 💖

| Contributor | Role |
|-------------|------|
| **blubskye** | Project Owner & Yuno's #1 Fan 💕🔪 |
| **Maeeen** (maeeennn@gmail.com) | Original Developer 💝 |
| **Oxdeception** | Contributor 💗 |
| **fuzzymanboobs** | Contributor 💗 |

---

## 💗 Features

<table>
<tr>
<td width="50%">

### 🔪 Moderation
*"Anyone who threatens you... I'll eliminate them~"*
- ⛔ Ban / Unban / Kick
- ⏰ Timeout with duration
- 🧹 Bulk message cleaning
- 📊 Moderation statistics
- 📝 Action logging to database

</td>
<td width="50%">

### ✨ Leveling System
*"Watch me make you stronger, senpai~"*
- 📊 XP & Level tracking
- 🎉 Level-up announcements
- 🏆 Server leaderboards
- 🎯 Per-guild XP settings

</td>
</tr>
<tr>
<td width="50%">

### 🌸 Fun
*"Let me show you something cute~"*
- 🎱 Magic 8-ball fortune telling
- 💕 Yandere-themed responses

</td>
<td width="50%">

### ⚙️ Configuration
*"I'll be exactly what you need~"*
- 🔧 Customizable prefix per guild
- 📝 Per-guild settings
- 🔐 Master user system
- 🌍 Environment variable support

</td>
</tr>
<tr>
<td width="50%">

### ⚡ Performance
*"Nothing can slow me down~"*
- 🚀 Modern .NET 8.0 runtime
- 💨 Async/await throughout
- 🧵 Efficient event handling
- 📦 Cross-platform support

</td>
<td width="50%">

### 🔐 Database
*"I'll keep your secrets safe~"*
- 💾 SQLite with Microsoft.Data.Sqlite
- 📊 Indexed tables for speed
- 🔄 Automatic schema creation
- 📈 Efficient queries

</td>
</tr>
</table>

---

## 💕 Installation

### 📋 Prerequisites

> *"Let me prepare everything for you~"* 💗

- **.NET 8.0 SDK** or higher
- **Git**
- A Discord bot token ([Get one here](https://discord.com/developers/applications))

### 🌸 Setup Steps

```bash
# Clone the repository~ ♥
git clone https://github.com/blubskye/yuno_csharp.git

# Enter my world~
cd yuno_csharp

# Let me gather my strength...
dotnet restore

# Configure your settings
cp config.json.example config.json
nano config.json  # Add your token and settings
```

### 💝 Configuration

Edit `config.json`:
```json
{
    "discord_token": "YOUR_DISCORD_TOKEN",
    "default_prefix": ".",
    "database_path": "yuno.db",
    "master_users": ["YOUR_USER_ID"]
}
```

Or use environment variables:
```bash
export DISCORD_TOKEN="your_token_here"
export DEFAULT_PREFIX="."
export DATABASE_PATH="yuno.db"
export MASTER_USER="your_user_id"
```

### 🚀 Running

```bash
# Run directly
dotnet run

# Or build and run
dotnet build -c Release
./bin/Release/net8.0/Yuno

# With custom config path
dotnet run -- /path/to/config.json
```

---

## 💖 Commands Preview

### 📊 Leveling & XP
| Command | Description |
|---------|-------------|
| `/xp [@user]` | *"Look how strong you've become!"* ✨ |
| `/leaderboard` | *"Who's the most devoted?"* 🏆 |
| `.xp [@user]` | Prefix version |
| `.lb` or `.top` | Prefix version |

### 🔪 Moderation
| Command | Description |
|---------|-------------|
| `/ban @user [reason]` | *"They won't bother you anymore..."* 🔪 |
| `/kick @user [reason]` | *"Get out!"* 👢 |
| `/unban <user_id>` | *"Another chance~"* 💕 |
| `/timeout @user <minutes>` | *"Think about what you did~"* ⏰ |
| `/clean [count]` | *"Let me tidy up~"* 🧹 |
| `/mod-stats [@user]` | *"Look at all we've done~"* 📊 |

### 🌸 Fun
| Command | Description |
|---------|-------------|
| `/8ball <question>` | *"Let fate decide~"* 🎱 |
| `.8ball <question>` | Prefix version |

### ⚙️ Utility
| Command | Description |
|---------|-------------|
| `/ping` | *"I'm always here for you~"* 💓 |
| `/help` | *"Let me show you everything~"* 💕 |
| `/source` | *"I have nothing to hide~"* 📜 |
| `/prefix <new>` | *"Call me differently~"* 🔧 |

*Both slash commands and prefix commands are supported!*

---

## 📁 Project Structure

```
yuno_csharp/
├── Program.cs              # Entry point
├── Bot.cs                  # Main bot class
├── Config.cs               # Configuration loading
├── Database.cs             # SQLite database operations
├── Yuno.csproj            # Project file
├── config.json            # Configuration (create from example)
├── Commands/
│   ├── CommandHandler.cs      # Command routing
│   ├── ModerationCommands.cs  # Ban, kick, timeout, etc.
│   ├── UtilityCommands.cs     # Ping, help, xp, etc.
│   └── FunCommands.cs         # 8ball and fun commands
└── yuno.db                # SQLite database (auto-created)
```

---

## ⚡ Building

```bash
# Debug build
dotnet build

# Release build (optimized)
dotnet build -c Release

# Publish self-contained executable
dotnet publish -c Release -r linux-x64 --self-contained

# Publish for Windows
dotnet publish -c Release -r win-x64 --self-contained

# Publish for macOS
dotnet publish -c Release -r osx-x64 --self-contained
```

---

## 🔧 Required Bot Permissions

When inviting Yuno to your server, ensure these permissions:
- **Send Messages** - To respond to commands
- **Embed Links** - For rich embeds
- **Manage Messages** - For clean command
- **Ban Members** - For ban/unban commands
- **Kick Members** - For kick command
- **Moderate Members** - For timeout command

---

## 📜 License

This project is licensed under the **GNU Affero General Public License v3.0 (AGPL-3.0)** 💕

### 💘 What This Means For You~

*"I want to share everything with you... and everyone else too~"* 💗

The AGPL-3.0 is a **copyleft license** that ensures this software remains free and open. Here's what you need to know:

#### ✅ You CAN:
- 💕 **Use** this bot for any purpose (personal, commercial, whatever~)
- 🔧 **Modify** the code to your heart's content
- 📤 **Distribute** copies to others
- 🌐 **Run** it as a network service (like a public Discord bot)

#### 📋 You MUST:
- 📖 **Keep it open source** - If you modify and distribute this code, your version must also be AGPL-3.0
- 🔗 **Provide source access** - Users of your modified bot must be able to get the source code
- 📝 **State changes** - Document what you've modified from the original
- 💌 **Include license** - Keep the LICENSE file and copyright notices intact

#### 🌐 The Network Clause (This is the important part!):
*"Even if we're apart... I'll always be connected to you~"* 💗

Unlike regular GPL, **AGPL has a network provision**. This means:
- If you run a **modified version** of this bot as a public service (like hosting it for others to use on Discord)
- You **MUST** make your complete source code available to users
- The `/source` command in this bot helps satisfy this requirement!

#### ❌ You CANNOT:
- 🚫 Make it closed source
- 🚫 Remove the license or copyright notices
- 🚫 Use a different license for modified versions
- 🚫 Hide your modifications if you run it as a public service

#### 💡 In Simple Terms:
> *"If you use my code to create something, you must share it with everyone too~ That's only fair, right?"* 💕

This ensures that improvements to the bot benefit the entire community, not just one person. Yuno wants everyone to be happy~ 💗

See the [LICENSE](LICENSE) file for the full legal text.

**Source Code:** https://github.com/blubskye/yuno_csharp

---

<div align="center">

### 💘 *"You'll stay with me forever... right?"* 💘

**Made with obsessive love** 💗

*Yuno will always be watching over your server~* 👁️💕

---

⭐ *Star this repo if Yuno has captured your heart~* ⭐

</div>
