# Technical Portfolio: Systems & Architecture Deep Dive
### Monster Quest Simulator — Unity 6 / C# Engineering

This document provides a technical overview of the engineering decisions, design patterns, and optimization strategies implemented in **Monster Quest Simulator**. It is prepared as a companion document for technical leads and hiring committees reviewing the repository.

---

## Table of Contents
1. [Core Architectural Philosophy](#1-core-architectural-philosophy)
2. [Combat & Kinematics Engine](#2-combat--kinematics-engine)
3. [Asset Streaming & Memory Management](#3-asset-streaming--memory-management)
4. [Virtualized UI & Dynamic 3D Turntable](#4-virtualized-ui--dynamic-3d-turntable)
5. [Targeting & Telegraphing Subsystem](#5-targeting--telegraphing-subsystem)
6. [Data Persistence & Extensibility](#6-data-persistence--extensibility)
7. [Multiplayer Readiness (Netcode)](#7-multiplayer-readiness-netcode)

---

## 1. Core Architectural Philosophy

Massive simulation games often succumb to architectural entropy: monolithic MonoBehaviours that mix input, rendering, data loading, and physics. 

In this project, every module follows strict **Separation of Concerns**:
- **Pure Logic vs. Visual Representation**: The combat logic (`WeaponRuntimeStats`, `Enemy`, `PlayerCombat`) operates independently of 3D models and animators.
- **Service-Oriented Architecture (SOA)**: Centralized services (`WeaponAssetService`, `WeaponCatalogService`, `IconCacheService`, `WeaponPreviewService`) expose clean async APIs with `CancellationToken` support.
- **Zero Monolithic Dependencies**: `Resources.Load` was completely eradicated in favor of typed Addressables and a streaming JSON catalog.

---

## 2. Combat & Kinematics Engine

### Dynamic Formation Distribution
Rather than using static socket transforms on the player rig, floating weapons are autonomous entities managed by `WeaponOrbitVisualController`:

```csharp
// Excerpt from formation positioning
float angleStep = 360f / totalWeapons;
float currentAngle = (baseAngle + index * angleStep) * Mathf.Deg2Rad;

// Atom Orbit calculation combining radial and vertical harmonics
float x = Mathf.Cos(currentAngle) * orbitRadius;
float z = Mathf.Sin(currentAngle) * orbitRadius;
float y = Mathf.Sin(currentAngle * 2f + phaseOffset) * verticalAmplitude;

Vector3 targetOrbitPos = playerTransform.position + new Vector3(x, y + heightOffset, z);
```

### Zero-Physics Mathematical Thrusts
To allow 15+ weapons per player without PhysX overhead or Animator state graph evaluation:
1. **Anticipation (Windup)**: Slight backward pull to telegraph the incoming strike.
2. **Thrust (`EaseInBack`)**: Aggressive acceleration toward the target position.
3. **Contact & Damage Dispatch**: Instantaneous trigger on the `Enemy` health interface, emitting floating damage numbers.
4. **Recoil (`EaseInOutQuad`)**: Decelerated recovery curve seamlessly rejoining the orbit trajectory.

---

## 3. Asset Streaming & Memory Management

### Reference-Counted Addressables (`WeaponAssetService`)
When multiple players or weapons request the same 3D mesh (e.g., three identical swords equipped), loading three duplicate Addressable handles wastes VRAM and CPU cycles.

`WeaponAssetService` implements an internal reference-counted handle cache:
- **First request**: Calls `Addressables.LoadAssetAsync<GameObject>(key)`, tracks handle in internal cache, sets `refCount = 1`.
- **Subsequent requests**: Awaits the existing in-flight `Task` or returns an instantiated clone from the already-loaded handle, incrementing `refCount`.
- **Release**: Decrements `refCount`. The asset handle is only released via `Addressables.Release` when the last weapon instance is destroyed.

### LRU Icon Cache (`IconCacheService`)
UI grids scrolling through hundreds of weapons can easily exceed mobile memory limits if every texture remains resident in memory.
- Implements a Least-Recently-Used (LRU) dictionary structure.
- Bounded by maximum items and memory footprint.
- Evicts oldest unused textures when thresholds are crossed.
- Every async request passes a `CancellationToken`, instantly aborting downloads if a cell is scrolled out of view.

---

## 4. Virtualized UI & Dynamic 3D Turntable

### Interactive Turntable (`WeaponPreviewService`)
To inspect any weapon in full 3D within the UI:
- A dedicated miniature studio room exists in an isolated culling layer (`WeaponPreview`).
- A dedicated camera renders strictly to an off-screen `RenderTexture`.
- `WeaponPreviewInputHandler` translates pointer drag events into angular velocity around the model's Y-axis.
- When closed, the `RenderTexture` is cleanly released and models are returned to `WeaponAssetService`.

---

## 5. Targeting & Telegraphing Subsystem

### High-Depth Decal Projection (`TargetVisuals`)
Classic floating reticles often clip through character meshes or look disconnected on sloped terrain.
- Implemented via Universal Render Pipeline **`DecalProjector`**.
- Positioned high above the target with a calibrated vertical projection volume (10 units deep).
- Guarantees complete floor projection regardless of monster jump animations, squash-and-stretch hit reactions, or stepped terrain.
- Dual-state color encoding:
  - **Primary Target**: Vivid Green decal.
  - **Queued Target**: High-visibility Orange decal.

---

## 6. Data Persistence & Extensibility

### GUID-Based Instances (`WeaponInstanceData`)
Every weapon in the player's possession is represented as a unique instance:
```csharp
[Serializable]
public class WeaponInstanceData
{
    public string instanceId; // System.Guid.NewGuid()
    public int weaponId;      // References catalog
    public int level;
    public int enhancement;
    public long acquiredTimestamp;
}
```
This architecture supports individual weapon enhancements, custom enchantments, individual trading, and prevents duplication bugs when players own multiple copies of the same weapon ID.

---

## 7. Multiplayer Readiness (Netcode)

The project includes foundational integration with **Unity Netcode for GameObjects**:
- `NetworkedPlayer`: Player entity with server-authoritative state synchronization.
- Decoupled visual controllers that can run in client-prediction mode while damage checks remain server-validated.
- Clean shutdown handlers (`CleanNetworkShutdown`) preventing dangling socket connections on client disconnects.

---

*Authored by otmexa — Built with Unity 6 & C#*
