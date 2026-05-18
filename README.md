# 👻 Phantom Detective

> *"The dead cannot rest. The truth must be found."*

A first-person horror survival puzzle game built in Unity. Explore an abandoned asylum, collect memory fragments, solve the crime, and free three trapped souls — all while avoiding the ghosts that haunt its halls.

---

## 🎬 Gameplay Video

▶️ **[Watch Gameplay on YouTube](https://your-video-link-here.com)**

---

## 📖 About The Game

In **Phantom Detective**, you play as a detective hired to investigate an abandoned asylum. Armed with only a flashlight, you must explore 24 rooms across two floors and a basement, collect scattered memory fragments, and piece together a jigsaw puzzle that reveals the truth behind each ghost's death.

Three ghosts patrol the asylum. They cannot be killed. They cannot be reasoned with. Your only options are to **hide**, **evade**, and **investigate**.

| Detail | Info |
|---|---|
| Engine | Unity 6 (URP) |
| Genre | Horror / Survival / Puzzle |
| Perspective | First-Person |
| Players | Single Player |
| Setting | Abandoned Asylum — 24 Rooms, 2 Floors + Basement |

---

## 🕹️ Controls

| Action | Key |
|---|---|
| Move | W / A / S / D |
| Look Around | Mouse |
| Open / Close Doors | Left Mouse Click |
| Crouch | Ctrl or C |
| Toggle Flashlight | F |
| Collect Fragments | E |
| Elevator | Y |
| Pause | P |
| Jump | Space |

---

## 🎯 How To Win

1. Explore all 24 rooms across the asylum
2. Collect all **9 memory fragments** hidden throughout
3. Navigate to the **Puzzle Board**
4. Arrange fragments correctly to reconstruct the crime scene
5. Complete the puzzle → the truth is revealed → you escape

---

## 👾 The Ghosts

Three ghosts patrol the asylum. Each one is a trapped soul reliving the loop of its own murder.

- **Ghost 1 — Ball Demon** — patrols the upper floors
- **Ghost 2 — Hell Demon** — patrols the basement corridors  
- **Ghost 3 — Alien Demon** — patrols the central hall

Each ghost has:
- A **sight cone** (field of view detection)
- **Hearing** based on your noise level
- A **patrol route** between waypoints
- **Chase** and **Search** states when you're detected

Walls and closed doors block their vision. **Crouch and hide** when they're near.

---

## 🚀 How To Run The Project

### Prerequisites
- Unity **6000.3.9f1** (Unity 6 LTS) or later
- Universal Render Pipeline (URP) support
- Git installed

### Step-by-Step Setup

**Step 1 — Create a New Unity Project**
```
1. Open Unity Hub
2. Click New Project
3. Select 3D (URP) template  ← important: must be URP
4. Name it: Phantom_Detective
5. Click Create Project
```

**Step 2 — Clone This Repository**
```bash
git clone https://github.com/Phantom-Detective/Phantom-Detective.git
```

**Step 3 — Copy Repository Contents Into Your Project**
```
1. Open File Explorer
2. Navigate to the cloned repo folder
3. Copy these folders into your Unity project's root:
   - Assets/
   - Packages/
   - ProjectSettings/
4. Replace existing folders when prompted
```

**Step 4 — Open In Unity**
```
1. Go back to Unity Hub
2. Click Add → Add project from disk
3. Select your Phantom_Detective folder
4. Click Open
5. Wait for Unity to import all assets (may take a few minutes)
```

**Step 5 — Install Required Packages**

If any packages are missing, install via Window → Package Manager:
```
- AI Navigation
- Input System
- TextMeshPro
- Universal RP
```

**Step 6 — Run The Game**
```
1. In Project panel → Assets → Scenes
2. Open StarterScene  ← start here!
3. Press Play in Unity Editor
   OR
   File → Build Settings → Build And Run
```

> ⚠️ **Important:** Always open `StarterScene` first. Do NOT open `GamePlayScene` directly — the game requires the intro sequence to initialize properly.

---

## 📁 Project Structure

```
Assets/
├── Abandoned_Asylum/     ← mansion environment
├── Audio/                ← all sound files
├── M4_Puzzle/            ← puzzle system
├── MonsterMutant 7/      ← ghost character 1
├── Demon/                ← ghost character 2
├── Snake3D/              ← ghost character 3
├── Prefabs/
│   ├── Ghosts/           ← ghost prefabs
│   ├── Player/           ← player prefab
│   ├── HidingSpots/      ← hiding spot prefabs
│   └── Waypoints/        ← patrol waypoints
├── Scripts/
│   ├── Ghost/            ← all ghost AI scripts
│   ├── Player/           ← player scripts
│   └── Managers/         ← game management
└── Scenes/
    ├── StarterScene      ← intro / launch here
    └── GamePlayScene     ← main game
```

---

## 🧠 Ghost AI System

The ghost AI is built on a **State Machine** with 4 states:

```
PATROL  →  (detects player)  →  CHASE
                                   ↓
                              (loses player)
                                   ↓
                              SEARCH  →  (timeout)  →  PATROL
                                   ↓
                         (all shards collected)
                                   ↓
                              DISAPPEAR
```

Detection uses:
- **Sight cone** (field of view + raycast through walls)
- **Hearing** (based on player noise level — walking vs running)
- **Obstacle mask** (walls and doors block detection)

---

## 👥 Team

| Name | ID |
|---|---|
| زيد هاني محمد صالح الدين | 2022170175 |
| نيره احمد شفيق مصطفى | 2022170477 |
| أمنية صالح محمود حامد | 2022170074 |
| سما خالد ابراهيم محمد | 2022170191 |
| نوران هيثم عثمان عثمان | 2022170473 |

---

## 📄 Documentation

Full game design document available in the `/Documment` folder of this repository.

---

## 🛠️ Built With

- Unity 6 (URP)
- C#
- Unity NavMesh AI
- Unity Input System (New)
- TextMeshPro
- Unity Animator State Machines

---

*Phantom Detective — where the dead cannot rest, and the truth cannot hide.*
