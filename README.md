# Coffin Clash

> A 2-player local combat game where you push a coffin toward your rival — then defend your tower from what crawls out of it.

![Gameplay](coffin-clash.gif)

## 🎮 About The Game
Coffin Clash is a local 2-player game built around a tug-of-war coffin and a wave-defense twist. Two towers face off with a coffin caught between them. Firing arrows at the coffin knocks it toward your opponent's side — land enough hits and it slams into their tower.

The game runs in a repeating three-phase loop:

- **⚔️ Combat:** Fire arrows to push the coffin toward your rival's tower. Every hit also earns gold.
- **🛒 Shop:** Spend gold between rounds to upgrade your tower's push power and level.
- **💀 Invasion:** Waves of skeletons (and eventually a boss) spawn from the coffin and march on both towers — defend yourself before the next round begins.

Whoever's tower runs out of health first loses.

## 🕹️ Controls
- **Player 1:** `D` to fire, `W` to upgrade in the Shop.
- **Player 2:** `Left Arrow` to fire, `Up Arrow` to upgrade in the Shop.
- **Space:** Start game / confirm in Shop / return to menu after Game Over.
- **Esc:** Quit.

## 🛠️ Tech Stack
- **C#**
- **MonoGame Framework**

## 🏁 Getting Started
1. Clone this repository: `git clone https://github.com/cadakerem/coffin-clash.git`
2. Open the `.sln` solution file in **Visual Studio**.
3. Restore NuGet packages, build, and run the project! (All necessary MonoGame Content assets are included).

## 🧑‍💻 Developer & Contributions
Developed by Kerem Barbaros Karnabat ([@cadakerem](https://github.com/cadakerem)).

> **Note on Repository Structure:** This Unity-based multiplayer/co-op game houses all of its networked logic, prefabs, and models within the `Assets/` directory. The project is designed to be built via the standard Unity build pipeline for target platforms.

Contributions, issues, and feature requests are welcome! Feel free to check the [Issues page](../../issues).

## 📜 License
This project is licensed under the [MIT License](LICENSE).
