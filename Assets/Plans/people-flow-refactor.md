# Project Overview
- **Game Title:** People Flow (Refactored Prototype)
- **High-Level Concept:** A casual puzzle game where players push minions from a queue into a conveyor belt to reach specific color-coded goal holes. 
- **Players:** Single player
- **Inspiration / Reference Games:** People Flow (Antlerex)
- **Language Policy:** 100% English for all scripts, variable names, class names, strings, and hierarchy object names.
- **Clean Code Standard:** Implementing SOLID principles, descriptive naming, modular components, and event-driven communication.
- **Render Pipeline:** URP (existing configuration)

---

# Game Mechanics

## Core Gameplay Loop
1. **Queue Phase:** Minions are stacked in `WaitingArea` columns (MinionQueue).
2. **Push Logic:** Players tap/hold a `WaitingArea` column to release minions onto the conveyor belt.
3. **Traversal:** Minions travel along a spline-based conveyor path (`Road`). If the path is disconnected, they use `TeleportGate` (TravelDoor) to jump between path segments.
4. **Goal Logic:** `GoalContainer` (Factory) structures contain multiple `GoalGate` (Hole) objects. Minions of matching colors jump into active `GoalGate` slots.
5. **Progression:** When a `GoalGate` count reaches zero, it closes. When all `GoalGate` slots in a `GoalLine` are cleared, a `ClearBarrier` (Barrier) appears.
6. **Win/Fail:** Clear all goals to win. Fail if the conveyor overflows or time runs out.

## Controls and Input Methods
- **Tap/Hold Input:** Uses the New Input System to interact with `MinionQueue` objects.

---

# UI
- **HUD:** English labels for Timer, Capacity, and Status.
- **Panels:** English Win/Fail screens.

---

# Key Asset & Context

## Model Roles (Revised Mapping)
| Model | Role | Component |
|---|---|---|
| `Hole.fbx` | Goal / Gate Slot | `GoalGate.cs` |
| `Factory.fbx` | Goal Container | `GoalContainer.cs` |
| `WaitingArea.fbx` | Spawn Queue | `MinionQueue.cs` |
| `TravelDoor.fbx` | Teleporter | `TeleportGate.cs` |
| `Barrier.fbx` | Final Blockade | `ClearBarrier.cs` |
| `Minion.fbx` | Character | `MinionAgent.cs` |
| `Road.fbx` | Conveyor Visual | Visual Decoration |

---

# Implementation Steps

### Step 1: Prefab Refactoring
- **Description**: Reconstruct `Gate.prefab` using `Hole.fbx` and rename it to `GoalGate.prefab`. Update `MinionColumn.prefab` to use `WaitingArea.fbx` and rename it to `MinionQueue.prefab`. Create new prefabs for `GoalContainer` (using `Factory.fbx`), `ClearBarrier` (using `Barrier.fbx`), and `TeleportGate` (using `TravelDoor.fbx`).
- **Assigned role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

### Step 2: Core Script Refactoring (English & Clean Code)
- **Description**: Rewrite and rename existing scripts to enforce 100% English and apply Clean Code principles.
  - Rename `Gate.cs` -> `GoalGate.cs`
  - Rename `GateLine.cs` -> `GoalLine.cs`
  - Rename `MinionColumn.cs` -> `MinionQueue.cs`
  - Update `MinionAgent.cs` to support `TeleportGate` logic.
  - Implement `GoalContainer.cs` to manage children `GoalGate` objects and the `ClearBarrier` spawn.
- **Assigned role**: developer
- **Dependencies**: Step 1
- **Parallelizable**: No

### Step 3: Level Loading & Data Logic
- **Description**: Update `LevelLoader.cs` and `LevelConfig.cs` to support the new `GoalContainer` parenting and the correct assignment of `MinionQueue` locations.
- **Assigned role**: developer
- **Dependencies**: Step 2
- **Parallelizable**: No

### Step 4: UI & Event Bus Translation
- **Description**: Clean up `EventBus.cs` and `HudController.cs` to ensure all events, public properties, and displayed text are in English.
- **Assigned role**: developer
- **Dependencies**: Step 3
- **Parallelizable**: Yes

### Step 5: Verification & Play-Test
- **Description**: Rebuild the sample scene using the new prefabs and level configuration. Verify the full loop: Queue -> Conveyor -> Teleport (if applicable) -> Goal Hole -> Barrier.
- **Assigned role**: developer
- **Dependencies**: Step 4
- **Parallelizable**: No

---

# Verification & Testing
- **Visual Check**: All hierarchy objects named correctly in English (e.g., `Conveyor_Loop`, `Goal_Factory_1`).
- **Logic Check**: `GoalGate` count decrements when correct color enters. `ClearBarrier` appears only after the line is cleared.
- **Runtime Check**: Play mode test confirms no errors and correct behavior in English.
