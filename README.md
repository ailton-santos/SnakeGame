# 🐍 Snake Game Deluxe

**Advanced Terminal-Based Arcade Experience featuring RPG Elements and Dynamic Physics**
Developed by Ailton Dos Santos

![C#](https://img.shields.io/badge/Language-C%23-239120?style=flat-square) ![.NET](https://img.shields.io/badge/Framework-.NET_9.0-512BD4?style=flat-square) ![Status](https://img.shields.io/badge/Status-Playable-brightgreen?style=flat-square)

## 📄 Project Overview

**Snake Game Deluxe** is a high-performance console application that reimagines the classic arcade mechanic. Unlike standard implementations, this engine introduces **Object-Oriented Logic**, **Dynamic Obstacles**, and a **Complex Power-up System**.

The system renders graphics using `System.Console` raw manipulation, ensuring zero-latency input response. It features a progressive difficulty system across 10 levels, introducing moving walls, teletransportation portals, and protective shields.

## ⚙️ Core Mechanics

* **Dynamic Entity Management:** Utilizes a custom `GameObject` inheritance system to handle polymorfic entities (Food, Obstacles, Player).
* **Adaptive Difficulty:** The game engine adjusts speed (Tick Rate) and map complexity as the player advances levels.
* **Advanced Collision System:**
    * **Static Objects:** Walls (█) that cause immediate termination.
    * **Dynamic Threats:** Moving obstacles (▲) with AI pathing logic.
* **Power-up Ecosystem:**
    * 🛡️ **Shield:** Grants temporary invulnerability against collisions.
    * 🔮 **Portal:** Implements non-Euclidean geometry (teleportation).
    * ⚡ **Speed Boost:** Temporarily modifies the game loop duration.
* **Persistent State:** Local Highscore tracking system using file I/O.

## 🛠️ Technologies Used

* **Core Engine:** C# (Microsoft .NET 9.0)
* **Architecture:** OOP (Object-Oriented Programming)
* **Rendering:** Native Console Buffer Manipulation
* **Logic:** `System.Threading` for game loop control and `System.Diagnostics` for precise timing.

## 🎮 How to Play

### Controls
| Key | Action |
| :---: | :--- |
| **↑ ↓ ← →** | Directional Control |
| **P** | Pause / Resume Engine |
| **ESC** | Terminate Process |

### Legend (Symbols)
* `●` **Regular Food:** +10 Pts
* `♦` **Special:** +30 Pts
* `■` **Shield:** Invulnerability
* `○` **Portal:** Teleport
* `▲` **Enemy:** Moving Obstacle

## 🚀 Installation & Build

1. **Prerequisites:** Ensure **.NET 9.0 SDK** is installed.
2. **Clone the repository:**
   ```bash
   git clone [https://github.com/ailton-santos/snake-game-deluxe.git](https://github.com/ailton-santos/snake-game-deluxe.git)
