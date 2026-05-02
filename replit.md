# Phantom Detective – Abandoned Asylum

## Project Overview

A first-person horror exploration game set inside a haunted asylum, built with Unity 6 (6000.3.9f1) and the Universal Render Pipeline (URP 17.3).

Since Unity projects cannot run directly in a browser environment, this Replit environment serves a static HTML overview page that documents the project's scripts, controls, and assets.

## Project Type

- **Engine:** Unity 6000.3.9f1
- **Render Pipeline:** Universal Render Pipeline 17.3.0
- **Input System:** New Input System 1.18.0
- **Navigation:** AI Navigation 2.0.10
- **Scene:** SampleScene (Assets/Scenes/)

## Running in Replit

The workflow serves `index.html` via Python's built-in HTTP server on port 5000:

```
python3 -m http.server 5000 --bind 0.0.0.0
```

## Project Structure

```
Assets/
  Abandoned_Asylum/
    Scripts/Environment/   # Game C# scripts
    Prefabs/               # 100+ asset prefabs
    Textures/              # 2048×2048 PBR textures
    Materials/             # 144 materials
    Models/                # 109 3D models
    Scenes/                # SampleScene
  Editor/                  # Unity Editor utility scripts
  Scenes/                  # Main scene files
  Settings/                # URP render pipeline assets
Packages/                  # Unity package manifest
ProjectSettings/           # Unity project settings
index.html                 # Project overview page (served in Replit)
```

## Game Scripts

| Script | Category | Purpose |
|---|---|---|
| PlayerMovement.cs | Environment | First-person WASD movement + mouse-look |
| PlayerHealth.cs | Environment | 3-lives system with respawn |
| PlayerNoise.cs | Environment | Noise level tracking for AI |
| PlayerHiding.cs | Environment | Hiding state tracker |
| DoubleDoorToggle.cs | Environment | F-key animated double doors |
| MouseDrivenDoor.cs | Environment | Mouse-drag door interaction |
| MouseLook.cs | Environment | Camera look controller |
| HorrorCrosshair.cs | UI | Procedural glowing horror crosshair |
| FixDoubleDoorSetup.cs | Editor | Batch door component setup tool |
| FixMouseDrivenDoorHinges.cs | Editor | Hinge root correction tool |

## Player Controls

- **WASD / Arrow Keys** — Move
- **Left Shift** — Run (increases noise)
- **Mouse** — Look around
- **F** — Interact with double doors
- **Left Click + Drag** — Grab and push/pull mouse-driven doors

## Deployment

Configured as a static site deployment (`publicDir: "."`).
