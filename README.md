<h1 align="center">🕯️ Clockwork Exorcism</h1>

<p align="center">
  <strong>A high-stakes first-person psychological horror & exorcism survival game developed for GMTK Game Jam 2026.</strong>
</p>

<p align="center">
  <img src="Screenshots/banner.png" alt="Clockwork Exorcism Banner" width="100%" />
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Unity-6000.5.5f1-black?style=for-the-badge&logo=unity" alt="Unity 6" />
  <img src="https://img.shields.io/badge/URP-Universal%20Render%20Pipeline-blue?style=for-the-badge" alt="URP" />
  <img src="https://img.shields.io/badge/Platform-WebGL%20%7C%20Windows-brightgreen?style=for-the-badge" alt="Platform" />
  <img src="https://img.shields.io/badge/Genre-Horror%20%7C%20Puzzle%20%7C%20Survival-red?style=for-the-badge" alt="Genre" />
  <img src="https://img.shields.io/badge/Event-GMTK%20Game%20Jam%202026-orange?style=for-the-badge" alt="GMTK 2026" />
</p>

---

## 📜 Overview

You wake up trapped inside a dark, cursed manor stalked by a restless spectral entity. An ominous grandfather clock is ticking down from **100 seconds**. As time bleeds away, reality itself corrupts: warm amber room lights gradually twist into a suffocating, blood-red glow, lights flicker violently, and your pounding heartbeat drowns out the silence.

Your only salvation is a solitary **wax candle** and sacred **Taoist sealing talismans**. To break the curse and unlock the escape door, you must explore the manor, find active cursed objects, and perform real-time talisman exorcisms before time expires or total darkness consumes your mind.

---

## 📸 In-Game Screenshots

<div align="center">

| **🕯️ Candle Exploration** | **👹 The Stalking Entity** |
| :---: | :---: |
| <img src="Screenshots/candle_exploration.png" alt="Candle Exploration" width="460" /><br><sub><i>Navigate pitch-black rooms using only the dim flame of your candle.</i></sub> | <img src="Screenshots/ghost_entity.png" alt="The Ghost Entity" width="460" /><br><sub><i>The clockwork entity lunges as time ticks away. Aim your light to banish it!</i></sub> |

| **🪆 Discovering Cursed Relics** | **📜 The Talisman Sealing Ritual** |
| :---: | :---: |
| <img src="Screenshots/cursed_doll.png" alt="Cursed Doll Relic" width="460" /><br><sub><i>Active cursed objects glow with an eerie supernatural aura. Press <code>E</code> to exorcise.</i></sub> | <img src="Screenshots/talisman_minigame.png" alt="Talisman Minigame" width="460" /><br><sub><i>Trace the moving arcane focus point with your cursor to scribe sacred runes.</i></sub> |

| **🖼️ Corrupted Picture Frame** | **🗝️ Cursed Music Box** |
| :---: | :---: |
| <img src="Screenshots/cursed_frame.png" alt="Corrupted Frame" width="460" /><br><sub><i>Randomized relics awaken throughout the house requiring your intervention.</i></sub> | <img src="Screenshots/cursed_box.png" alt="Cursed Music Box" width="460" /><br><sub><i>Antique relics emit sinister audio cues when ready to be sealed.</i></sub> |

</div>

<!-- ================================================================= -->
<!-- 📷 EXTRA SCREENSHOT PLACEHOLDER                                     -->
<!-- You can add more screenshots below anytime:                       -->
<!-- <p align="center"><img src="Screenshots/your_pic.png" width="80%" /></p> -->
<!-- ================================================================= -->

---

## 📖 Story & Prologue

<p align="center">
  <img src="Screenshots/prologue.png" alt="Prologue Intro" width="85%" />
</p>

> *"Long ago, an ancient force awakened in the shadows...  
> Many tried to stop it, but none succeeded.  
> Now, the fate of the realm rests in your hands..."*

---

## 🎮 How to Play

<p align="center">
  <img src="Screenshots/instructions.png" alt="In-Game Instructions" width="85%" />
</p>

1. **Explore the Manor**: Search through the rooms to find active cursed relics emitting a distinctive red/magenta glow.
2. **Keep Your Candle Lit**: Your candle is your lifeline. Total darkness will rapidly drain your sanity and trigger instant madness.
3. **Banish the Ghost**: Ghosts are attracted to light. If the entity approaches, **face it directly and focus your candle light on it for a few seconds** to force it to vanish. If it touches you, it will rapidly drain your candle flame.
4. **Draw the Talisman**: Press `E` to initiate the exorcism. Follow the moving arcane energy point with your mouse. Completing the drawing seals the object. Failing costs **30% candle power**!
5. **Break the Curse & Escape**: Seal **3 required cursed objects** to unbolt the manor's escape door. Race to the door before the master timer reaches zero!

---

## 🕹️ Controls

| Control | Action |
| :--- | :--- |
| <kbd>W</kbd> <kbd>A</kbd> <kbd>S</kbd> <kbd>D</kbd> | Move Character |
| <kbd>Mouse</kbd> | Look / Aim Candle Light |
| <kbd>E</kbd> | Interact / Start Exorcism / Open Escape Door / Exit Minigame |
| <kbd>Mouse Movement</kbd> | Track the arcane target point on the Talisman Paper |
| <kbd>Left Click</kbd> / <kbd>Space</kbd> | UI navigation & dialogue advance |

---

## ✨ Key Features & Gameplay Systems

### 🕯️ 1. Candle Power & Sanity Management
* **Dual Resource**: The candle provides vision while preserving your sanity.
* **Rapid Darkness Penalty**: In zero candle light, sanity decay accelerates drastically (+5.0/sec), rapidly leading to an insanity game over.
* **Risk vs Reward**: Failed exorcisms cost 30% of your current candle power, directly threatening your survival.

### 📜 2. Dynamic Talisman Drawing Minigame
* **Cursor Tracking**: An arcane tracking point darts across ancient talisman parchment.
* **Visual Juice**: Includes dynamic pulsing, color-coded feedback (blue for on-target, warning red for off-target), particle motion trails, and calligraphy ink revealing itself as progress builds.
* **Audio Layering**: Continuous ink-brush calligraphy loop audio that pauses whenever the player veers off-target.

### ⏳ 3. Environmental Horror & Escalation
* **Color Shifting**: Room lights smoothly transition from natural warm ambient lighting (`#D46428`) to an ominous corrupted red (`#650000`).
* **Dynamic Flickering**: Perlin-noise-driven light flicker severity intensifies as time runs down.
* **Layered Audio**: Ticking clock tempo speeds up, pitch elevates, and a rhythmic low-frequency heartbeat kicks in when under 30 seconds.

### 👻 4. Intelligent Apparition AI
* **NavMesh Stalker**: Ghosts dynamically choose non-player rooms to manifest in, tracking the player through the house with 3D spatial whispers.
* **Candle Ward Reaction**: Utilizing dot-product line-of-sight and raycast obstacle detection, holding candle light on the ghost triggers an agonized groan before it fades away.

---

## 📂 Project Architecture

```
GMTK2026/
├── Assets/
│   ├── Scripts/
│   │   ├── Controllers/
│   │   │   └── CandleController.cs    # Candle power level, drain logic, and light intensity scaling
│   │   ├── Enemies/
│   │   │   ├── GhostAI.cs             # NavMesh stalker AI, whisper audio, candle drain on contact
│   │   │   └── GhostVanishTrigger.cs  # Candle line-of-sight & light exposure ghost-banishing mechanic
│   │   ├── Interaction/
│   │   │   ├── InteractionSystem.cs   # Viewport raycast interaction and HUD prompts
│   │   │   ├── CursedObject.cs        # Relic state machine, audio triggers, materials, particle VFX
│   │   │   ├── GhostSpawner.cs        # Probability-based ghost spawner across rooms
│   │   │   ├── RoomDetector.cs        # Room boundary trigger detectors
│   │   │   └── EscapeDoor.cs          # Tracks exorcism progress, audio rattles, and victory trigger
│   │   ├── Managers/
│   │   │   ├── GameManager.cs         # Master game loop, 100s timer, lighting decay, game over states
│   │   │   └── SanityManager.cs       # 3-phase sanity decay curve & darkness logic
│   │   └── UI/
│   │       ├── MainMenu.cs            # Typewriter prologue, async scene loader, options
│   │       ├── TalismanMiniGame.cs    # Mouse-tracking minigame, trail effects, calligraphy logic
│   │       └── UIButtonJuice.cs       # Smooth hover & click juice animations
│   ├── Scenes/
│   │   └── MainMenuScene.unity        # Main menu scene with starry sky and prologue
│   └── MapRoom/
│       └── Room.unity                 # Main haunted house gameplay level
├── Screenshots/                       # Organized gameplay screenshots and banner
└── pictures/                          # Original capture gallery
```

---

## 🛠️ Tech Stack & Requirements

* **Game Engine**: Unity 6 (`6000.5.5f1`)
* **Render Pipeline**: Universal Render Pipeline (URP)
* **Platforms**: WebGL & PC Standalone (Windows)
* **Audio**: Spatial 3D Audio, Dynamic Pitch Modulation, Procedural Ambience
* **Version Control**: Git LFS (Large File Storage) for 3D GLB models and audio clips

---

## 🚀 Setup & Installation

### 1. Clone the Repository
```bash
git clone https://github.com/Tiongalex/GMTK2026.git
cd GMTK2026
```

### 2. Pull Git LFS Assets
Make sure you have Git LFS installed so that 3D models and audio clips download properly:
```bash
git lfs install
git lfs pull
```

### 3. Open in Unity
1. Launch **Unity Hub**.
2. Click **Add** > **Add project from disk**.
3. Select the `GMTK2026` folder.
4. Ensure the editor version is set to **Unity 6 (6000.5.5f1)** or compatible.

### 4. Running the Game
* In the Project window, navigate to [`Assets/Scenes/MainMenuScene.unity`](file:///C:/Users/clare/Desktop/Project/GMTK2026/Assets/Scenes/MainMenuScene.unity).
* Press the **Play** button at the top of the Unity Editor.

---

## 👥 Credits & Acknowledgments

* **Event**: Created for the **GMTK Game Jam 2026**.
* **Developed By**: Tiongalex & Friends.
* **3D Models & Assets**:
  * *AK STUDIO ART* &mdash; Vintage Radio & Environment Props
  * *DarkDropStudio* &mdash; Stylized Cursed Voodoo Prop
  * *Tensori* &mdash; Skinless Zombie Entity
  * *Free Wood Door Pack* &mdash; Interactive Escape Door
  * *Yughues Free Flooring Materials* &mdash; Interior Floor Textures
* **Character Controller**: Mini First Person Controller.

---

<p align="center">
  <sub>Built with ❤️ for <b>GMTK Game Jam 2026</b>. Will you escape the manor before the final tick?</sub>
</p>
