<div align="center">
  <img src="https://pan.samyyc.dev/s/VYmMXE" />
  <h2><strong>CS2-MapDecals</strong></h2>
  <h3>A SwiftlyS2 plugin that allows server owners to place decals on maps at predefined locations.</h3>
  <br><br>
</div>

![GitHub tag (with filter)](https://img.shields.io/github/v/tag/Cruze03/CS2-MapDecals-SwiftlyS2?style=for-the-badge&label=Version)
![GitHub Repo stars](https://img.shields.io/github/stars/Cruze03/CS2-MapDecals-SwiftlyS2?style=for-the-badge)
![GitHub issues](https://img.shields.io/github/issues/Cruze03/CS2-MapDecals-SwiftlyS2?style=for-the-badge)
![GitHub all releases](https://img.shields.io/github/downloads/Cruze03/CS2-MapDecals-SwiftlyS2/total?style=for-the-badge)
[![Discord](https://img.shields.io/badge/Discord-Join%20Server-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://cruzecoding.com/discordserver)

<p align="center">
  <a href="https://github.com/Cruze03/CS2-MapDecals-SwiftlyS2/releases/latest"><strong>Download</strong></a>
  ·
  <a href="https://github.com/Cruze03/CS2-MapDecals-SwiftlyS2/issues/new?assignees=Cruze03&labels=bug&title=%5BBUG%5D"><strong>Report Bug</strong></a>
  ·
  <a href="https://github.com/Cruze03/CS2-MapDecals-SwiftlyS2/issues/new?assignees=Cruze03&labels=enhancement&title=%5BREQ%5D"><strong>Request Feature</strong></a>
</p>

</div>

---

## ❤️ Support the Project

<div align="center">

This project is developed and maintained purely out of passion.  
If this plugin has helped you or your community, please consider supporting its development!

<a href="https://paypal.me/ssachin03">
  <img src="https://img.shields.io/badge/PayPal-Support%20Development-00457C?style=for-the-badge&logo=paypal&logoColor=white" />
</a>

</div>

---

## 🎥 Preview

[![Watch Preview Video](https://raw.githubusercontent.com/Cruze03/CS2-MapDecals-SwiftlyS2/main/.git-assets/mapdecal-preview1.png)](https://youtu.be/_wMVWcOAkM8)
[![Watch Preview Video2](https://raw.githubusercontent.com/Cruze03/CS2-MapDecals-SwiftlyS2/main/.git-assets/mapdecal-preview2.png)](https://youtu.be/_wMVWcOAkM8)

## ✨ Features

- Place decals at predefined locations on maps
- Supports forced and non-forced decals
- Player preference saving for non-forced decals
- Multi-database support for flexibility
- Easy configuration and automatic database setup

---

## 📦 Dependencies

The plugin requires the following dependencies to function correctly:

- **[SwiftlyS2 - v1.1.1-beta.38+](https://github.com/swiftly-solution/swiftlys2)** – Required core framework
- **[Cookies](https://github.com/swiftly-solution/cookies)**  
  Used to store player preferences for toggling non-forced decals

---

### Supported Databases

You can use **any one** of the following databases:
- **MySQL / MariaDB**
- **PostgreSQL**
- **SQLite** (recommended for single-server setups)

---

## 🚀 Installation

1. Add a database entry named `cc_mapdecals` to SwiftlyS2’s `database.jsonc` file  
   (supports MySQL, PostgreSQL, and SQLite).
2. Download the **[latest release](https://github.com/Cruze03/CS2-MapDecals-SwiftlyS2/releases/latest)**.
3. Extract the files into:
   ```
   addons/swiftlys2/plugins/
   ```
4. Start the server once to generate configuration files.
5. Configure the plugin in:
   ```
   addons/swiftlys2/configs/plugins/cc.mapdecals/
   ```
6. Restart the server.  
   Database tables will be created automatically.

---

## 🕹️ Commands

| Command | Description |
|------|------------|
| **!mapdecal** | Opens the main decal menu to place decals and edit their settings. <br> *(Admin-only by default, configurable in `config.json`)* |
| **!decal** | Toggles visibility of non-forced decals for the player. <br> *(VIP-only by default, configurable in `config.json`)* |

---

## 👾 Known Issues
- [ ] Per player transmit doesn't seem to work. Will fix in future version.

---

## 🧠 Credits & Inspiration

- **[Poor Map Decals](https://github.com/Letaryat/CS2-Poor-MapDecals)**  
  Original concept and partial code inspiration.
- **[K4-LevelRanks-SwiftlyS2](https://github.com/K4ryuu/K4-LevelRanks-SwiftlyS2)**  
  Architecture and structural inspiration.