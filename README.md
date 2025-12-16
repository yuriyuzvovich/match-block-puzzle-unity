# Match Block Puzzle

Swipe-to-match prototype built with Unity 6 (6000.2.7f2). Slide blocks around a grid, let gravity settle them, and clear lines of matching elements while balloons and a layered backdrop keep the scene alive.

## Overview
- Swipe a block to move into an empty neighbor or swap with an occupied one; gravity runs after every move and matches of 3+ identical blocks (rows or columns) are destroyed.
- Progression is data-driven: levels are JSON files resolved via `Assets/MatchBlockPuzzle/GameAssets/Common/Resources/Levels/LevelConfiguration.asset` and `Levels/Data`.
- Visual polish comes from DOTween animations, ScriptableObject-driven block definitions (fire, water), URP camera framing, and floating balloons rendered from pooled prefabs.
- Saves persist between sessions (PlayerPrefs); the game resumes where you left off and auto-advances when a level is cleared.

## Quick Start
1) Open with Unity `6000.2.7f2` (or newer in the 6.x line).  
2) Load `Assets/MatchBlockPuzzle/Scenes/GameScene.unity` — it only contains the `Bootstrapper` that wires up services, UI, background, balloons, and the grid.  
3) Press Play. Click/tap and drag on a block to set the swipe direction (upward swipes swap if a block is present, downward/sideways can push into empty cells). Clear the board to advance; use the in-game buttons to restart or skip to the next level.

## Project Layout
- `Assets/MatchBlockPuzzle/Scripts` — domain (grid, normalization), application services/commands, input/camera/pooling/logging, and feature modules for background, balloons, and UI.
- `Assets/MatchBlockPuzzle/GameAssets/Common/Resources/BlockTypes` — block type ScriptableObjects (art, animators, editor colors).
- `Assets/MatchBlockPuzzle/GameAssets/Common/Resources/Levels` — `LevelConfiguration.asset` plus JSON level data under `Levels/Data`.
- `Assets/MatchBlockPuzzle/GameAssets/Common/Resources/Settings` — tuning knobs: grid/camera/match/game/input, animation speeds, pooling, persistence, and asset key maps.
- `Assets/MatchBlockPuzzle/GameAssets/Features` — prefabs and settings for the background, balloon ambience, and UI.

## Authoring Notes
- Add or tweak levels by editing/adding JSON in `Assets/MatchBlockPuzzle/GameAssets/Common/Resources/Levels/Data` and listing them in `LevelConfiguration.asset`.
- Create new block types by duplicating a `BlockTypeData` asset in `Resources/BlockTypes` and assigning sprites/animators; reference them in `GameSettings.asset`.
- Adjust feel and visuals via `MatchSettings.asset` (match length, delays), `BlockAnimationSettings.asset` (move/fall/destroy timings), `CameraSettings.asset`, `InputSettings.asset`, and `BalloonSettings.asset`.

## Tech
Unity Input System, Cysharp UniTask, DOTween, TextMeshPro, URP. Licensed under the MIT License (`LICENSE`).
