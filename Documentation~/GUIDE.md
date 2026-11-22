# Sdx17 NPC Framework — User Guide

> **Version 0.1.0 — UNTESTED IN UNITY**
>
> This package was written without Unity available in this environment.
> No Editor import, compile, or Play-mode test has been run. Do not treat
> it as tested or production-ready until the TEST checklist in `README.md`
> has been completed in Unity 2022.3 LTS. Everything in this guide
> describes the code as written, not behaviour verified in Unity.

- Package: `com.sdx17.npc`, version 0.1.0, Unity 2022.3 LTS
- Namespace: `Sdx17.NPC`
- Licence: MIT — Copyright 2026 Sdx17
- Requirements: Unity 2022.3 LTS, NavMesh (AI Navigation module),
  URP or Built-in render pipeline. No paid dependencies.
- Companion file: `README.md` (setup summary, file table, TEST checklist)

---

## 1. Quickstart (5 minutes)

Follow these steps exactly and you will have a patrolling NPC that
chases and attacks the player.

1. **Install the package.** In Unity 2022.3 LTS, open
   Window > Package Manager > **+** > Add package from disk, and select
   this folder's `package.json`. Or copy the folder into your project's
   `Packages/` directory.
2. **Create a ground plane.** GameObject > 3D Object > Plane, scale it
   to about 30 x 30. This is the walkable surface.
3. **Bake a NavMesh.** Select the plane, open Window > AI > Navigation,
   go to the Bake tab, and press **Bake**. The plane must be marked
   Navigation Static (it is by default for a new plane in most Unity
   versions — check the Inspector). Without a baked NavMesh the NPC logs
   a warning and cannot move. See §8, Q1 if the Navigation window is
   missing.
4. **Create a player.** GameObject > 3D Object > Capsule, name it
   `Player`, set its Tag to `Player`. Add a simple movement script, or
   move it with the Transform tools in Play mode. Give it an
   `NPCHealth` component only if you want the NPC to be able to damage
   it — the controller looks for `IDamageable` on the target, and a
   target without it simply takes no damage.
5. **Create a definition.** Right-click in the Project window >
   Create > Sdx17 > NPC Definition. Set, for example: Aggro Range 10,
   Attack Range 2, Is Aggressive on, Max Health 100.
6. **Create an NPC.** GameObject > Sdx17 > NPC > Create NPC (with
   Controller + Health). Drag the definition onto the NPCController's
   Definition field.
7. **Patrol (optional).** Create 3 empty GameObjects as waypoints
   around the plane and drag them into the NPCController's Patrol
   Waypoints list.
8. **Press Play.** Walk the player near the NPC: an aggressive NPC
   should Chase, then Attack in range. A passive NPC with waypoints
   should Patrol.

To try the included demo driver, follow `Samples~/Demo/README.md`:
add the `NPCDemo` script to an empty GameObject, then in Play mode
press **I** within 3 m of the NPC to interact, or **K** within 3 m to
deal 25 damage. Watch the NPC flee below its flee-health fraction and
die at 0 health.

---

## 2. Concepts

### 2.1 One definition, many NPCs

`NPCDefinition` is a ScriptableObject: one asset per NPC type
(for example "Village Guard" or "Wandering Merchant"). Many NPC
GameObjects share one definition, so a balance change (more health,
longer aggro range) is one edit in one asset — no scene or prefab
changes. Create one via right-click in the Project window >
Create > Sdx17 > NPC Definition, or GameObject > Sdx17 > NPC >
Create NPC Definition.

### 2.2 The state machine

`NPCController` drives one NPC through six states (`NPCState`):

| State | What the NPC does |
| --- | --- |
| `Idle` | Stands still, watches for a target in Aggro Range. |
| `Patrol` | Walks between waypoints, waiting `Patrol Wait Time` at each one. |
| `Chase` | Moves toward the target while it is in Aggro Range and within Chase Max Distance; gives up past that limit and returns to Patrol / Idle. |
| `Attack` | In Attack Range, faces the target and deals `Attack Damage` through `IDamageable` every `Attack Cooldown` seconds. |
| `Flee` | When health drops below the definition's Flee Health Fraction, runs directly away from the target until out of Aggro Range x 1.5, then recovers to Patrol / Idle. |
| `Dead` | Stops the NavMeshAgent, fires the death event, stays down until `Revive()` is called. |

State changes fire two notifications: the UnityEvent
`OnStateChanged` (assignable in the Inspector) and the C# event
`StateChanged`. Both carry the new `NPCState`.

Only aggressive NPCs chase on sight. A passive NPC (`Is Aggressive`
off) never chases a target it merely sees; it flees when damaged
below its flee fraction, or chases its attacker if aggressive and
damaged (see `HandleDamaged` behaviour in §3.3). An NPC with no
waypoints starts in `Idle`; one with waypoints starts in `Patrol`.

### 2.3 Health and the damage contract

Health lives in `NPCHealth`, which implements the local
`IDamageable` interface. The package defines its own interface on
purpose: the Sdx17 Combat package is **not** a dependency, so Combat
— or any other damage source, a trap, a test script — talks to NPCs
through `IDamageable` instead of through a concrete NPC type.
Likewise, an NPC damages its target only through `IDamageable` on
that target. Give the player an `IDamageable` health component if
NPCs should be able to damage the player; otherwise attacks find
nothing and do nothing.

Death fires once and stays dead until `Revive()` is called
(`NPCHealth.Revive()` restores full health; `NPCController.Revive()`
also returns the state machine to Patrol / Idle).

### 2.4 Interaction hooks — events and string ids, not dependencies

`NPCInteraction` deliberately does **not** reference the Sdx17
Dialogue, Inventory, or Quest packages. It fires UnityEvents and
C# events carrying the string ids from the `NPCDefinition`
(`DialogueId` / `ShopId` / `QuestId`). Your game wires those events
to its own systems in the Inspector or in code. With nothing wired
up, `Interact()` still works: it logs which hook was requested, so
missing wiring is visible instead of silent.

Priority when several roles are set: **dialogue first, then shop,
then quest**. Call `TryOpenDialogue()` / `TryOpenShop()` /
`TryOfferQuest()` directly to trigger one hook only. Dead NPCs
cannot be interacted with — `Interact()` returns `false`.

### 2.5 Spawning

`NPCSpawner` spawns NPCs from a prefab at configured spawn points,
keeps at most `MaxAlive` alive at once, and respawns on a timer
after an NPC dies or is removed. It listens to each spawned NPC's
health death event, so a dead NPC is counted as gone immediately
and its GameObject is destroyed after `Corpse Lifetime` seconds
(0 destroys it at once). Respawned NPCs are placed at a random
spawn point. Assign the NPC prefab and at least one spawn point and
the spawner does the rest.

### 2.6 What this package is NOT

- Not a complete game, AI, animation, or dialogue system. There is
  no animation controller, no dialogue UI, no shop UI, and no quest
  logic here — those systems consume the events this package fires.
- No demo scene file is included. The demo is a script plus a setup
  guide (`Samples~/Demo/README.md`).
- No saving, networking, or admin features. Those belong to the
  sibling Sdx17 systems and are integrated only through the events
  described in §6.

---

## 3. API reference

All classes are in the `Sdx17.NPC` namespace. This reference was
written from the actual files in `Runtime/` — every public class,
property, event, and method listed below exists in the code as of
v0.1.0. Private helpers (`TryAggro`, `TickIdle`, `TickPatrol`,
`TickChase`, `TickAttack`, `TickFlee`, `HandleDamaged`,
`HandleDeath`, `DealDamageToTarget`, `ApplyDefinition`,
`AdvanceWaypoint`, `MoveToCurrentWaypoint`, `SetDestination`,
`StopAgentIfNeeded`, `LogAgentWarningOnce`, `SetState`,
`ReturnToPatrolOrIdle`, `RandomSpawnPoint`, `HandleNpcDied`,
`RemoveAfterDelay`, `ScheduleRespawn`, `DisplayName`) are internal
implementation and are not part of the public API.

### 3.1 `NPCDefinition` — `Runtime/NPCDefinition.cs`

ScriptableObject data asset for one NPC type.
Create via: right-click in the Project window >
Create > Sdx17 > NPC Definition
(`[CreateAssetMenu(fileName = "NewNPCDefinition", menuName = "Sdx17/NPC Definition")]`).

Read-only properties (backed by serialized fields edited in the
Inspector; see §4 for defaults):

| Property | Type | Meaning |
| --- | --- | --- |
| `DisplayName` | `string` | Name shown in logs / UI by consuming systems. |
| `Description` | `string` | Free text for designers. |
| `MaxHealth` | `float` | Health set by `NPCHealth.Initialize` when the definition is applied. |
| `FleeHealthFraction` | `float` | Below this fraction of max health the NPC flees when damaged. 0 = never flees. |
| `MoveSpeed` | `float` | NavMeshAgent speed. |
| `PatrolWaitTime` | `float` | Pause at each waypoint before moving to the next. |
| `IsAggressive` | `bool` | On: chases / attacks a target inside Aggro Range. Off: only flees or defends when damaged. |
| `AggroRange` | `float` | Distance at which an aggressive NPC notices the target. |
| `AttackRange` | `float` | Distance at which the NPC switches from Chase to Attack. |
| `AttackDamage` | `float` | Damage dealt per attack through `IDamageable`. |
| `AttackCooldown` | `float` | Seconds between attacks. |
| `ChaseMaxDistance` | `float` | Gives up the chase beyond this distance and returns to Patrol / Idle. |
| `IsMerchant` | `bool` | Which interaction roles the NPC has. |
| `IsQuestGiver` | `bool` | Which interaction roles the NPC has. |
| `HasDialogue` | `bool` | Which interaction roles the NPC has. |
| `DialogueId` | `string` | String id handed to the Dialogue system via events. Empty = no hook. |
| `ShopId` | `string` | String id handed to the Inventory / shop system via events. Empty = no hook. |
| `QuestId` | `string` | String id handed to the Quest system via events. Empty = no hook. |

Method:

- `void Validate()` — Clamps values that would break the state
  machine and fixes inconsistent combinations (for example an
  attack range larger than the aggro range, which could never be
  reached; `ChaseMaxDistance` is raised to at least `AggroRange`;
  `MaxHealth` is clamped to at least 1; `AttackCooldown` to at
  least 0.1; a blank display name becomes `"NPC"`; null ids become
  `""`). Called by the editor and whenever the definition is
  applied. `OnValidate()` calls it automatically in the Editor, so
  you normally never call it yourself.

### 3.2 `IDamageable` — `Runtime/IDamageable.cs`

Contract for anything that can take damage. Implemented by
`NPCHealth`; any other component (player health, another NPC, a
Combat health component) can implement it too.

Properties:

| Member | Type | Meaning |
| --- | --- | --- |
| `CurrentHealth` | `float` (get) | Current health. |
| `MaxHealth` | `float` (get) | Maximum health. |
| `IsDead` | `bool` (get) | True when health is at zero. |

Events (C#):

| Event | Argument | Fired |
| --- | --- | --- |
| `Damaged` | `float` amount taken | After health changes from damage. |
| `Healed` | `float` amount healed | After health changes from healing. |
| `Died` | — | Once when health reaches zero. |

Methods:

- `void TakeDamage(float amount, GameObject source)` — Applies
  damage. Negative or zero amounts are ignored.
- `void Heal(float amount)` — Restores health, clamped to
  `MaxHealth`. Does nothing when dead.

### 3.3 `NPCController` — `Runtime/NPCController.cs`

Drives one NPC: state machine, NavMesh movement, patrol waypoints,
aggro detection, attacking, fleeing on low health, and death.
`[RequireComponent(typeof(NPCHealth))]`,
`[DisallowMultipleComponent]`. If no NavMeshAgent is present the NPC
still runs its state machine but cannot move — a warning is logged
once.

Enum `NPCState`: `Idle`, `Patrol`, `Chase`, `Attack`, `Flee`, `Dead`.

Properties:

| Member | Type | Meaning |
| --- | --- | --- |
| `Definition` | `NPCDefinition` (get) | The assigned definition (may be null — see §8, Q3). |
| `State` | `NPCState` (get) | Current state. |
| `Target` | `Transform` (get) | Current chase / attack target, or null. |
| `Health` | `NPCHealth` (get) | The `NPCHealth` on the same GameObject. |
| `OnStateChanged` | `UnityEvent<NPCState>` (get) | Inspector-assignable; fires with the new state. |
| `OnAggro` | `UnityEvent` (get) | Inspector-assignable; fires when aggro starts. |
| `OnDeath` | `UnityEvent` (get) | Inspector-assignable; fires on death. |

Event (C#):

- `event Action<NPCState> StateChanged` — Fired with the new state
  whenever the state changes.

Methods:

- `void SetDefinition(NPCDefinition newDefinition)` — Assigns a
  definition and applies its values to health and movement
  (`NPCHealth.Initialize(definition.MaxHealth)`,
  `NavMeshAgent.speed = definition.MoveSpeed`).
- `void SetPatrolWaypoints(Transform[] waypoints)` — Assigns patrol
  waypoints at runtime. Pass null or an empty array to stop
  patrolling. If the NPC is Idle and waypoints are supplied, it
  switches to Patrol immediately.
- `void SetTarget(Transform newTarget)` — Overrides the chase /
  attack target. Pass null to clear it. If no override is set, the
  controller finds the target by tag at `Start()` (see §4.3).
- `void Revive()` — Brings a dead NPC back to life at full health,
  returning to Patrol or Idle.

Behaviour notes (from the code, not verified in Play mode):

- `Start()` applies the definition, resolves the target, subscribes
  to `health.Died` / `health.Damaged`, and starts in Patrol if
  waypoints exist, otherwise Idle.
- Being hit makes a passive NPC flee once its health is at or below
  its flee fraction, and makes an aggressive NPC chase its attacker.
- In `Attack`, the NPC faces the target (Slerp, rate 8) and deals
  damage on its cooldown; if the target moves beyond
  `AttackRange + 0.5`, it returns to Chase.
- In `Flee`, the NPC runs directly away from the target until the
  distance exceeds `AggroRange * 1.5`, then recovers.
- In `Chase`, the NPC gives up beyond `ChaseMaxDistance` and returns
  to Patrol / Idle.
- `Update()` does nothing while `Dead`, and nothing at all while no
  definition is assigned.

### 3.4 `NPCHealth` — `Runtime/NPCHealth.cs`

Health component for an NPC. Implements `IDamageable` so any damage
source can hurt the NPC without knowing about `NPCController`.
`[DisallowMultipleComponent]`. Health starts full;
`Initialize()` (called by the controller, or manually) sets the
maximum from the definition. Death fires once and stays dead until
`Revive()` is called.

Properties:

| Member | Type | Meaning |
| --- | --- | --- |
| `CurrentHealth` | `float` (get) | Current health. |
| `MaxHealth` | `float` (get) | Maximum health. |
| `IsDead` | `bool` (get) | True when `CurrentHealth <= 0`. |
| `HealthFraction` | `float` (get) | Health as a 0–1 fraction, useful for health bars and flee checks. |
| `OnDamaged` | `UnityEvent<float>` (get) | Inspector-assignable; fires with the amount taken. |
| `OnHealed` | `UnityEvent<float>` (get) | Inspector-assignable; fires with the amount healed. |
| `OnDied` | `UnityEvent` (get) | Inspector-assignable; fires once on death. |

Events (C#, from `IDamageable`): `Damaged` (`float`),
`Healed` (`float`), `Died`.

Methods:

- `void Initialize(float newMaxHealth)` — Sets a new maximum (from
  an `NPCDefinition`) and refills health. Values below 1 are clamped
  to 1.
- `void TakeDamage(float amount, GameObject source)` — Applies
  damage. Ignored when dead or when `amount <= 0`. Fires
  `OnDamaged` / `Damaged`, then `OnDied` / `Died` if health reaches
  zero.
- `void Heal(float amount)` — Restores health, clamped to
  `MaxHealth`. Does nothing when dead or when `amount <= 0`. Fires
  `OnHealed` / `Healed` with the amount actually healed.
- `void Revive()` — Restores full health after death, so a spawner
  can reuse the NPC.
- `void SetHealth(float value)` — Sets health directly, clamped to
  0..`MaxHealth`. Used by tests and save loading.

### 3.5 `NPCInteraction` — `Runtime/NPCInteraction.cs`

Interaction hooks for an NPC: dialogue, shop, and quest-giver.
`[RequireComponent(typeof(NPCController))]`,
`[DisallowMultipleComponent]`. Requires an `NPCController` on the
same GameObject (logs an error in `Awake()` if missing).

Properties (UnityEvents, Inspector-assignable):

| Member | Type | Meaning |
| --- | --- | --- |
| `OnDialogueRequested` | `UnityEvent<string>` (get) | Fires with the Dialogue Id. |
| `OnShopRequested` | `UnityEvent<string>` (get) | Fires with the Shop Id. |
| `OnQuestRequested` | `UnityEvent<string>` (get) | Fires with the Quest Id. |
| `OnInteracted` | `UnityEvent` (get) | Fires on every successful `Interact()` call, before the hook. |

Events (C#):

- `event Action<string> DialogueRequested` — Fired with the dialogue id.
- `event Action<string> ShopRequested` — Fired with the shop id.
- `event Action<string> QuestRequested` — Fired with the quest id.

Methods:

- `bool Interact()` — The single entry point for player
  interaction (E key, raycast click, proximity prompt — whatever
  the game uses). Fires the highest-priority available hook:
  dialogue, then shop, then quest. Returns `true` if any hook
  fired. Returns `false` for a dead NPC, a missing controller, or
  an NPC with no dialogue, shop, or quest configured (and logs
  which case it was).
- `bool TryOpenDialogue()` — Requests dialogue if the definition
  has dialogue enabled (`HasDialogue`) and an id set. Returns
  `false` (and logs) if dialogue is enabled but no Dialogue Id is
  set.
- `bool TryOpenShop()` — Requests the shop if the NPC is a merchant
  (`IsMerchant`) and a Shop Id is set. Returns `false` (and logs)
  if it is a merchant with no Shop Id.
- `bool TryOfferQuest()` — Requests a quest if the NPC is a quest
  giver (`IsQuestGiver`) and a Quest Id is set. Returns `false`
  (and logs) if it is a quest giver with no Quest Id.

### 3.6 `NPCSpawner` — `Runtime/NPCSpawner.cs`

Spawns NPCs from a prefab at configured spawn points, keeps at most
`MaxAlive` alive at once, and respawns on a timer after an NPC dies
or is removed. `[DisallowMultipleComponent]`.

Properties:

| Member | Type | Meaning |
| --- | --- | --- |
| `NpcPrefab` | `GameObject` (get) | The NPC prefab to spawn. |
| `MaxAlive` | `int` (get) | Maximum NPCs alive at once. |
| `AliveCount` | `int` (get) | Number of NPCs currently alive. |
| `Alive` | `IReadOnlyList<GameObject>` (get) | Read-only view of the alive NPCs. |
| `OnSpawned` | `UnityEvent<GameObject>` (get) | Inspector-assignable; fires with the spawned instance. |
| `OnDespawned` | `UnityEvent<GameObject>` (get) | Inspector-assignable; fires with the removed instance. |

Methods:

- `GameObject SpawnOne()` — Spawns one NPC at a random spawn
  point, if under the alive limit. Returns the instance, or null
  if no prefab is assigned (logs an error), no spawn points are
  assigned (logs an error), the alive limit is reached, or no
  non-null spawn point could be picked.
- `int SpawnUpToMax()` — Spawns NPCs until the alive limit is
  reached. Returns how many were spawned.
- `void Despawn(GameObject npc)` — Removes an NPC immediately
  (destroys its GameObject) and schedules a respawn. Does nothing
  for null or for an NPC the spawner does not track.
- `void DespawnAll()` — Destroys all alive NPCs and clears the
  respawn schedule.

Behaviour notes (from the code, not verified in Play mode):

- If `Spawn On Start` is on, `Start()` calls `SpawnUpToMax()`.
- `Update()` tops the population up: after a death it waits
  `Respawn Delay`, otherwise it spawns on `Spawn Interval` while
  below `MaxAlive`.
- A dead NPC's GameObject is destroyed after `Corpse Lifetime`
  seconds, then a respawn is scheduled. If `Respawn Delay` is 0,
  the respawn happens immediately.

### 3.7 Editor support (not Runtime, listed for completeness)

- `Editor/NPCDefinitionEditor.cs` — Custom inspector for
  `NPCDefinition` with grouped sections and validation warnings
  (for example when Attack Range > Aggro Range).
- `Editor/NPCMenu.cs` — Menu items: create an NPC Definition asset,
  create an NPC GameObject (Controller + Health + Interaction +
  NavMeshAgent), create an NPC Spawner.
- `Samples~/Demo/NPCDemo.cs` — Demo driver: finds the NPC and
  player, logs state changes and hook requests; **I** = interact,
  **K** = deal 25 damage (within 3 m).
- `Tests~/NPCSelfTest.cs` — Play-mode self-test MonoBehaviour
  logging PASS / FAIL per check; the summary line should read
  **0 FAIL**.

---

## 4. Config reference

### 4.1 `NPCDefinition` fields

| Field | Default | Meaning |
| --- | --- | --- |
| Display Name | `NPC` | Name shown in logs / UI by consuming systems. |
| Description | (empty) | Free text for designers. |
| Max Health | 100 | Health set by `NPCHealth.Initialize` when the definition is applied. Minimum 1. |
| Flee Health Fraction | 0.25 | Below this fraction of max health the NPC flees when damaged. 0 = never flees. Range 0–1. |
| Move Speed | 3.5 | NavMeshAgent speed. |
| Patrol Wait Time | 2 s | Pause at each waypoint before moving to the next. |
| Is Aggressive | off | On: chases / attacks a target inside Aggro Range. Off: only flees or defends when damaged. |
| Aggro Range | 10 | Distance at which an aggressive NPC notices the target. |
| Attack Range | 2 | Distance at which the NPC switches from Chase to Attack. |
| Attack Damage | 10 | Damage dealt per attack through `IDamageable`. |
| Attack Cooldown | 1 s | Seconds between attacks. Minimum 0.1. |
| Chase Max Distance | 25 | Gives up the chase beyond this distance and returns to Patrol / Idle. `Validate()` raises it to at least Aggro Range. |
| Is Merchant | off | NPC offers a shop. |
| Is Quest Giver | off | NPC offers a quest. |
| Has Dialogue | on | NPC has dialogue. |
| Dialogue Id | (empty) | String id handed to the Dialogue system. Empty = no hook. |
| Shop Id | (empty) | String id handed to the Inventory / shop system. Empty = no hook. |
| Quest Id | (empty) | String id handed to the Quest system. Empty = no hook. |

Validation: `Validate()` (and `OnValidate()` in the Editor)
clamps invalid values — see §3.1. The custom inspector also warns
when Attack Range > Aggro Range.

### 4.2 `NPCSpawner` fields

| Field | Default | Meaning |
| --- | --- | --- |
| NPC Prefab | (none — required) | Prefab to spawn. Must contain `NPCController` + `NPCHealth`. |
| Spawn Points | (none — required) | Array of Transforms; spawn picks one at random, skipping null entries. |
| Max Alive | 3 | Maximum NPCs alive at once. Minimum 1. |
| Spawn Interval | 5 s | How often the spawner tops the population up while below Max Alive. |
| Respawn Delay | 10 s | Delay after a death / despawn before respawning. 0 = respawn immediately. |
| Corpse Lifetime | 5 s | Seconds a dead NPC's GameObject stays before being destroyed. 0 = destroy at once. |
| Spawn On Start | on | Spawn up to Max Alive in `Start()`. |

### 4.3 `NPCController` Inspector fields

| Field | Default | Meaning |
| --- | --- | --- |
| Definition | (none) | The `NPCDefinition` to apply. Without one, the controller logs a warning, uses default values, and `Update()` does nothing. |
| Patrol Waypoints | (empty) | Array of Transforms. Empty = Idle; non-empty = Patrol. |
| Player Tag | `Player` | Tag used to find the target at `Start()` when no override is set. Empty string = no tag lookup. |
| Target Override | (none) | Direct Transform target. Takes priority over the tag lookup. |
| On State Changed | — | `UnityEvent<NPCState>` fired on every state change. |
| On Aggro | — | `UnityEvent` fired when aggro starts. |
| On Death | — | `UnityEvent` fired on death. |

### 4.4 `NPCHealth` Inspector fields

| Field | Default | Meaning |
| --- | --- | --- |
| Max Health | 100 | Overwritten by `Initialize()` when a definition is applied. Minimum 1. |
| Current Health | 100 | Starting health, clamped to 0..Max Health in `Awake()`. |
| On Damaged | — | `UnityEvent<float>` with the amount taken. |
| On Healed | — | `UnityEvent<float>` with the amount healed. |
| On Died | — | `UnityEvent` fired once on death. |

### 4.5 `NPCInteraction` Inspector fields

| Field | Meaning |
| --- | --- |
| On Dialogue Requested | `UnityEvent<string>` — wire to your dialogue system's open method. |
| On Shop Requested | `UnityEvent<string>` — wire to your shop / inventory system's open method. |
| On Quest Requested | `UnityEvent<string>` — wire to your quest system's offer method. |
| On Interacted | `UnityEvent` — fires on every successful `Interact()` call. |

### 4.6 Example definitions

**Village Guard (aggressive, patrols)**

- Max Health 120, Move Speed 4, Is Aggressive on, Aggro Range 12,
  Attack Range 2, Attack Damage 15, Attack Cooldown 1, Chase Max
  Distance 30, Flee Health Fraction 0.2, Patrol Wait Time 3,
  Has Dialogue on, Dialogue Id `guard_greeting`.

**Wandering Merchant (passive, flees, sells)**

- Max Health 80, Move Speed 3, Is Aggressive off, Aggro Range 8,
  Flee Health Fraction 0.4, Is Merchant on, Shop Id `merchant_general`,
  Has Dialogue on, Dialogue Id `merchant_greeting`.

**Quest Giver (passive, never flees, talks)**

- Max Health 100, Move Speed 0, Is Aggressive off, Flee Health
  Fraction 0, Is Quest Giver on, Quest Id `quest_rats`,
  Has Dialogue on, Dialogue Id `elder_intro`.

---

## 5. Code examples

```csharp
using Sdx17.NPC;
using UnityEngine;

// --- Definition ---
NPCDefinition def = controller.Definition;
def.Validate(); // clamps invalid values (also runs in the editor)

// --- Controller ---
controller.SetDefinition(def);
controller.SetTarget(playerTransform);          // null clears the target
controller.SetPatrolWaypoints(waypoints);       // null / empty stops patrolling
controller.Revive();                            // dead NPC back to full health
NPCState state = controller.State;              // Idle, Patrol, Chase, Attack, Flee, Dead
controller.StateChanged += s => Debug.Log(s);

// --- Health (IDamageable) ---
IDamageable damageable = npc.GetComponent<IDamageable>();
damageable.TakeDamage(25f, attackerGameObject);
damageable.Heal(10f);
bool dead = damageable.IsDead;
float fraction = npcHealth.HealthFraction;       // 0..1, for health bars

// --- Interaction (hooks) ---
NPCInteraction interaction = npc.GetComponent<NPCInteraction>();
interaction.DialogueRequested += id => dialogueSystem.Open(id);
interaction.ShopRequested     += id => shopSystem.Open(id);
interaction.QuestRequested    += id => questSystem.Offer(id);
bool handled = interaction.Interact();          // dialogue, then shop, then quest

// --- Spawner ---
NPCSpawner spawner = GetComponent<NPCSpawner>();
GameObject npc = spawner.SpawnOne();            // null if at max alive / misconfigured
int spawned = spawner.SpawnUpToMax();
spawner.Despawn(npc);
spawner.DespawnAll();
int alive = spawner.AliveCount;
```

Wiring in the Inspector instead of code: select the NPC, find the
`NPCInteraction` component, and drag your dialogue / shop / quest
system's GameObject into `On Dialogue Requested` /
`On Shop Requested` / `On Quest Requested`, choosing its open
method. The string id from the definition is passed automatically.

Health bar (UI): read `NPCHealth.HealthFraction` (0–1) every frame
or subscribe to `Damaged` / `Healed` and update the bar there.
Subscribe to `NPCController.StateChanged` to swap an icon or label
when the NPC aggros, flees, or dies.

---

## 6. Integration with the other Sdx17 systems

**Honest status:** all integration in this package is by events and
string ids — install none, some, or all of the other systems and the
NPC still works. The NPC side of every integration below is real
code in `Runtime/` (verified by reading it). The other systems'
side — their exact class and method names — is **not** documented
here, because it was not verified against those packages while
writing this guide. Wire the NPC events to whatever open / offer /
damage method the other system actually exposes, and test the pair
together in Unity before relying on it. No cross-package integration
has been tested in Unity yet.

The 11 sibling systems are the other folders under
`~/workspace/unity/` (Combat, Dialogue, Quest, Inventory, Terrain
Tools, UI, Multiplayer, Matchmaking, Secure Save, Admin, Developer
Plugin). Each section below says what the NPC fires or accepts, and
what the other system is expected to do with it.

### 6.1 Combat — health and damage

- **NPC side (verified):** `NPCHealth` implements `IDamageable`
  (`TakeDamage(float, GameObject)`, `Heal(float)`, `CurrentHealth`,
  `MaxHealth`, `IsDead`, events `Damaged` / `Healed` / `Died`).
  `NPCController` damages its target only through `IDamageable`
  on that target (`DealDamageToTarget`), and reacts to being
  damaged via `health.Damaged`.
- **Wiring:** Combat never references NPC types. It calls
  `IDamageable.TakeDamage(amount, source)` on whatever it hits.
  Give the player an `IDamageable` health component if NPCs should
  be able to damage the player; otherwise attacks find nothing and
  do nothing.
- **Events to consume:** `NPCHealth.OnDamaged` / `OnHealed` /
  `OnDied` (UnityEvents) and `Damaged` / `Healed` / `Died`
  (C# events) — for hit flashes, damage numbers, death handling,
  kill credit, or XP.

### 6.2 Dialogue — talking NPCs

- **NPC side (verified):** set `Has Dialogue` and `Dialogue Id` on
  the definition. `NPCInteraction.TryOpenDialogue()` fires
  `OnDialogueRequested` (UnityEvent) and `DialogueRequested`
  (C# event) with the id.
- **Wiring:** wire either event to your dialogue system's open
  method, e.g. `interaction.DialogueRequested += id => dialogueSystem.Open(id);`
  (replace `Open` with the Dialogue package's real method name).
  With nothing wired, `Interact()` logs which dialogue was
  requested.
- **Note:** this package contains no dialogue UI and no dialogue
  lines — the id is only a key the Dialogue system resolves.

### 6.3 Quest — quest givers

- **NPC side (verified):** set `Is Quest Giver` and `Quest Id`.
  `NPCInteraction.TryOfferQuest()` fires `OnQuestRequested` and
  `QuestRequested` with the id.
- **Wiring:** wire either event to the quest system's offer method,
  e.g. `interaction.QuestRequested += id => questSystem.Offer(id);`
  (replace `Offer` with the Quest package's real method name).
- **Priority note:** `Interact()` tries dialogue first, so a quest
  giver with dialogue set will greet first. Call
  `TryOfferQuest()` directly if the quest must open without the
  greeting, or leave `Has Dialogue` off for a pure quest giver.

### 6.4 Inventory — merchant shops

- **NPC side (verified):** set `Is Merchant` and `Shop Id`.
  `NPCInteraction.TryOpenShop()` fires `OnShopRequested` and
  `ShopRequested` with the id.
- **Wiring:** wire either event to the shop / inventory system's
  open method, e.g. `interaction.ShopRequested += id => shopSystem.Open(id);`
  (replace `Open` with the Inventory package's real method name).
  The shop id is a key the Inventory system resolves to a stock
  list — this package holds no items, prices, or currency.

### 6.5 Terrain Tools — spawns

- **NPC side (verified):** `NPCSpawner` takes an array of spawn
  points (`Transform[]`) and picks one at random per spawn,
  skipping null entries. `SpawnOne()` / `SpawnUpToMax()` are
  public, so any system can trigger a spawn.
- **Suggested wiring (not verified against Terrain Tools):** use
  Terrain Tools to place spawn-point markers on the terrain (for
  example village gates, cave mouths, patrol posts) and drag those
  Transforms into the spawner's Spawn Points list. For terrain-
  driven placement at runtime, have the Terrain system call
  `spawner.SpawnOne()` / `SpawnUpToMax()` after positioning, or
  move spawn points to terrain-sampled positions before spawning.
  The spawner itself does no terrain sampling — it trusts the
  Transforms it is given, so a spawn point floating above the
  ground or inside a rock will spawn the NPC there.

### 6.6 UI — health bars, names, state

- **NPC side (verified):** `NPCHealth.HealthFraction` (0–1) for
  health bars; `NPCDefinition.DisplayName` for name plates;
  `NPCController.State` plus `StateChanged` / `OnStateChanged`
  for state icons and labels; `NPCHealth` events for damage
  flashes; `NPCSpawner.AliveCount` / `Alive` for population
  counters.
- **Suggested wiring (not verified against the UI package):**
  subscribe to `Damaged` / `Healed` and set a health-bar fill to
  `HealthFraction`; subscribe to `StateChanged` to show "!" on
  aggro, a flee icon, or a skull on death; read
  `controller.Definition.DisplayName` for the name plate. This
  package renders no UI itself.

### 6.7 Multiplayer — networked NPCs

- **NPC side (verified):** everything is local and single-player:
  the state machine runs in `Update()`, health is a local float,
  the spawner uses `Instantiate` / `Destroy`.
- **Suggested wiring (not verified against Multiplayer):** this
  package has no networking code. For multiplayer, treat the NPC
  as server-authoritative: run `NPCController` / `NPCHealth` /
  `NPCSpawner` on the server/host only, replicate `State`,
  `HealthFraction`, and position to clients, and route damage
  through the server (clients call a server RPC that calls
  `IDamageable.TakeDamage` on the server). Do **not** run a
  separate state machine per client — the states will diverge.
  `SetTarget`, `SetPatrolWaypoints`, `Revive`, `SpawnOne`, and
  `Despawn` are the server-side control points. Networked
  integration is untested and is not a claim of this package.

### 6.8 Matchmaking — spawning NPCs per match

- **NPC side (verified):** `NPCSpawner.SpawnUpToMax()` spawns the
  full population on demand; `DespawnAll()` clears it;
  `Spawn On Start` can be turned off so nothing spawns until told.
- **Suggested wiring (not verified against Matchmaking):** turn
  `Spawn On Start` off, and have the match flow call
  `SpawnUpToMax()` when a match starts (or per round / per wave)
  and `DespawnAll()` when it ends. Use `OnSpawned` /
  `OnDespawned` to keep a match roster or scoreboard in sync.
  Matchmaking itself is not part of this package.

### 6.9 Secure Save — saving NPC state

- **NPC side (verified):** the state a save system needs is all
  readable: `NPCHealth.CurrentHealth` / `MaxHealth` (and
  `SetHealth(float)` to restore it), `NPCController.State`,
  `NPCController.Definition` (save its asset name or id, not the
  object), position / rotation, and the spawner's `AliveCount`.
  `NPCHealth.SetHealth` exists specifically for tests and save
  loading.
- **Suggested wiring (not verified against Secure Save):** on save,
  record each NPC's definition id, health, state, and transform;
  on load, instantiate the NPC, call `SetDefinition(...)`,
  `SetHealth(...)`, and restore the transform. Note the limits:
  `SetHealth` does not revive a dead NPC — call
  `NPCController.Revive()` first if the saved state was alive —
  and the spawner's internal timers are not exposed, so a loaded
  game restarts the spawn timers from zero. Save integration is
  untested.

### 6.10 Admin — inspecting and controlling NPCs

- **NPC side (verified):** every useful value is a public
  read-only property (`State`, `Health`, `Target`, `Definition`,
  `AliveCount`, `Alive`), and every control action is a public
  method (`SetDefinition`, `SetTarget`, `SetPatrolWaypoints`,
  `Revive`, `SpawnOne`, `SpawnUpToMax`, `Despawn`, `DespawnAll`,
  `TakeDamage`, `Heal`, `SetHealth`).
- **Suggested wiring (not verified against Admin):** an admin
  panel can list `spawner.Alive` with each NPC's
  `Definition.DisplayName`, `State`, and `HealthFraction`, and
  offer buttons wired to `Despawn(npc)`, `Revive()`,
  `Heal(...)`, or `SpawnOne()`. The `OnSpawned` /
  `OnDespawned` / `OnDied` events keep the panel live. This
  package ships no admin UI.

### 6.11 Developer Plugin — debugging NPCs

- **NPC side (verified):** state changes are observable
  (`StateChanged`, `OnStateChanged`, `OnAggro`, `OnDeath`,
  `OnDamaged`, `OnHealed`, `OnDied`), the demo driver
  (`Samples~/Demo/NPCDemo.cs`) logs state changes and hook
  requests, and `Tests~/NPCSelfTest.cs` logs PASS / FAIL per
  check in Play mode.
- **Suggested wiring (not verified against the Developer Plugin):**
  hook the plugin's debug overlay to `StateChanged` and the
  health events for a live NPC inspector, and run
  `NPCSelfTest` from the plugin's test runner. All log lines from
  this package are prefixed `[Sdx17 NPC]`, so they can be
  filtered in the Console.

---

## 7. Performance notes

- **Per-NPC cost is small.** Each NPC is a handful of components
  (`NPCController`, `NPCHealth`, `NPCInteraction` if used,
  `NavMeshAgent`). `Update()` does one distance check per state
  tick and one `NavMeshAgent.SetDestination` call per movement
  change — no per-frame allocations in the steady state (the
  state machine uses no LINQ and no per-frame `GetComponent`).
- **The expensive part is NavMeshAgent, not this package.**
  Dozens of agents on a baked NavMesh are routine in Unity;
  hundreds need profiling. Keep `MaxAlive` modest (the default
  is 3) and raise it only after profiling in your scene.
- **Aggro checks are distance checks, not physics queries.**
  `TryAggro()` runs a single `Vector3.Distance` against the one
  assigned target — there is no sphere cast or overlap query, and
  no line-of-sight check. An NPC aggros through walls; if you need
  line of sight, add a raycast in your own code before calling
  `SetTarget`, or gate `IsAggressive` per NPC.
- **Spawner timers are cheap.** The spawner does one float
  comparison per frame while below `MaxAlive` and nothing while
  full. `Alive` returns `alive.AsReadOnly()` — a thin wrapper,
  not a copy — but do not call it every frame in a tight loop
  without need.
- **Events are the main coupling cost.** Every state change fires
  `OnStateChanged` and `StateChanged`; every hit fires damaged
  events twice (UnityEvent + C# event). If a UI or logging
  subscriber does heavy work per event, that work — not the NPC —
  becomes the bottleneck. Keep subscribers light.
- **Flee destination is a straight line.** `TickFlee()` sets a
  destination directly away from the target at `AggroRange`
  distance. On a NavMesh with obstacles the agent will path
  around them, which costs the usual NavMesh pathfinding, not
  anything extra from this package.
- **No numbers in this section are measured.** No profiling has
  been run — the package is untested in Unity (see §10). Treat
  the notes above as reasoning from the code, and profile in your
  own scene before publishing performance claims.

---

## 8. Troubleshooting / FAQ

**Q1. The Navigation window is missing / I cannot bake a NavMesh.
What do I do?**

In Unity 2022.3 the AI Navigation package ships as a built-in
module. Open Window > Package Manager, check it is installed /
enabled (under Project Settings if needed — in some installs the
module must be enabled before Window > AI > Navigation appears).
Then select your ground plane, open Window > AI > Navigation, go
to the Bake tab, and press Bake. The ground must be marked
Navigation Static — for a new plane it is by default in most Unity
versions; check the Inspector if the bake produces no blue
walkable area. If the bake still produces nothing, check that the
plane's layer is included in the bake and that its scale is not
zero. Without a baked NavMesh the NPC logs a warning and stands
still (see Q2).

**Q2. My NPC does not move. It just stands there.**

Check, in order: (1) Is a NavMesh baked on the ground? Without
one, `agent.isOnNavMesh` is false and `SetDestination` returns
early — the NPC logs `[Sdx17 NPC] ... has no NavMeshAgent, so it
cannot move` only when the agent component itself is missing; a
missing bake fails silently except for the state machine still
running. (2) Does the NPC GameObject have a NavMeshAgent
component? The menu item GameObject > Sdx17 > NPC > Create NPC
adds one; a hand-built NPC may be missing it. (3) Is the NPC
standing on the NavMesh? An NPC spawned or placed off the baked
area has `isOnNavMesh == false` and will not move — move it onto
the blue area. (4) Is a definition assigned? Without one,
`Update()` returns early and the NPC never leaves Idle.
(5) Is the NPC Dead? A dead NPC's `Update()` returns immediately.

**Q3. I get a warning: "has no NPCDefinition assigned. Using default values."**

The `NPCController`'s Definition field is empty. Create a
definition (right-click in the Project window > Create > Sdx17 >
NPC Definition) and drag it onto the field, or call
`controller.SetDefinition(def)` in code. The NPC will still run
with default values, but its `Update()` does nothing while the
definition is null — the warning is telling you the NPC is
effectively inert.

**Q4. The NPC never chases or attacks the player.**

Check: (1) `Is Aggressive` is on in the definition — a passive
NPC never chases on sight. (2) The player GameObject is tagged
`Player` (or the tag in the controller's Player Tag field), or a
target was assigned with `SetTarget` / Target Override. The
controller finds its target once, in `Start()`, by tag — a player
created after the NPC starts, or tagged differently, is never
found; call `SetTarget(playerTransform)` after spawning the
player. (3) The player is within Aggro Range (default 10) — check
the distance in the Scene view. (4) A definition is assigned
(see Q3).

**Q5. The NPC attacks but the player takes no damage.**

The NPC damages its target only through `IDamageable` on the
target GameObject. If the player has no component implementing
`IDamageable` (such as `NPCHealth` or your Combat health
component), `DealDamageToTarget` finds nothing and does nothing —
by design, silently. Add an `IDamageable` health component to the
player. The same applies in reverse: anything that wants to hurt
an NPC must call `IDamageable.TakeDamage` on it.

**Q6. Interact does nothing / logs "has no dialogue, shop, or quest configured."**

`Interact()` returns `false` and logs that line when the
definition has no usable hook: `HasDialogue` off or no Dialogue
Id, not a merchant or no Shop Id, not a quest giver or no Quest
Id. Set the role flags and the matching id on the definition.
If the log says "has dialogue enabled but no Dialogue Id set"
(or the merchant / quest-giver equivalent), the role is on but
the id is empty — fill in the id. If `Interact()` fires but
nothing visible happens, nothing is wired to the events — wire
`OnDialogueRequested` / `OnShopRequested` / `OnQuestRequested`
in the Inspector, or subscribe to `DialogueRequested` /
`ShopRequested` / `QuestRequested` in code (see §5). Dead NPCs
cannot be interacted with.

**Q7. The spawner never spawns anything / spawns too many.**

Never spawns: check the NPC Prefab and at least one Spawn Point
are assigned — both log an error and `SpawnOne()` returns null
if missing. Check `Spawn On Start` is on, or call
`SpawnUpToMax()` yourself. Check the spawn points are not null
entries (the spawner skips nulls and returns null if none are
usable). Too many: `MaxAlive` is a cap, not a target — the
spawner never exceeds it, but remember a dead NPC still counts
until its corpse is destroyed after Corpse Lifetime; if corpses
linger, `AliveCount` stays high and respawns wait. Also check you
do not have two spawners sharing one prefab population.

**Q8. The NPC flees forever / never flees.**

Never flees: `Flee Health Fraction` is 0 (0 = never flees), or the
NPC has not been damaged below that fraction — flee is triggered
by `HandleDamaged`, i.e. only when the NPC actually takes damage.
Flees forever: the NPC recovers only when the target is further
than `AggroRange * 1.5` away; if the player keeps following, or
the target is null and the NPC is stuck, it stays in Flee.
Flee also requires a working NavMesh (see Q2) — without one the
NPC cannot run away and will stand in Flee state.

**Q9. I see "NPCInteraction needs an NPCController on the same GameObject."**

`NPCInteraction` requires an `NPCController` on the same
GameObject (`[RequireComponent]` should add it automatically when
you add the component in the Editor, but a prefab edited by hand
or a script-added component can miss it). Add an
`NPCController` to the same GameObject. The same class of error
applies to `NPCController` without `NPCHealth` — it logs an error
in `Awake()` and `Health` will be null.

**Q10. There are compile errors after importing the package.**

This package is **untested in Unity** — an import error is
possible and is not yet a known issue with a known fix. First
check the obvious: Unity 2022.3 LTS (the package targets
`unity: 2022.3`), the AI Navigation module enabled (for
`UnityEngine.AI`), and no name clash with another package
defining `IDamageable`, `NPCController`, or the `Sdx17.NPC`
namespace. If the error names a type from another Sdx17 package,
the two packages may define the same shared type — report it
(see `LISTING.md` for the support contact) with the full Console
error text. Do not edit the package files to work around it
without noting the change; an edited package cannot be updated
cleanly.

---

## 9. Limitations (honest)

- **UNTESTED IN UNITY.** No Editor import, compile, or Play-mode
  test has been run. Import errors, NavMesh setup issues, and
  Play-mode behaviour still need to be verified by hand in Unity
  2022.3 LTS (TEST checklist in `README.md`). Do not treat this
  package as tested or production-ready.
- **No animation.** There is no animation controller, no blend
  tree, and no animation events. The NPC moves and rotates; you
  must drive your own Animator from `StateChanged` /
  `HealthFraction` if you want walk / attack / death animation.
- **No dialogue, shop, or quest logic.** Only string-id hooks.
  The Dialogue, Inventory, and Quest systems (or your own) must
  exist and be wired up, or interaction does nothing visible.
- **No line of sight.** Aggro is a distance check only. NPCs
  aggro through walls and floors. No hearing, vision cone, or
  faction system.
- **No pathfinding beyond NavMeshAgent.** If no NavMesh is baked,
  or the NPC is off the NavMesh, it cannot move. Fleeing picks a
  straight-line destination that may be unreachable; the agent
  will path around obstacles or stop.
- **Single target, single-player.** The controller tracks one
  target, found by tag at `Start()`. There is no target selection
  among several players, no threat table, and no networking —
  multiplayer use needs a server-authoritative wrapper (§6.7).
- **No saving.** NPC state is not persisted. A save system must
  read the public properties and restore them itself (§6.9), and
  spawner timers restart from zero on load.
- **Target found once at Start.** A player spawned after the NPC
  starts is never found automatically; call `SetTarget` yourself.
- **Spawner is simple.** Random spawn point, flat timers, no
  waves, no difficulty scaling, no spawn conditions beyond the
  alive cap. Corpses count toward the alive cap until destroyed.
- **No demo scene file.** The demo is a script plus a setup guide;
  you build the scene by hand (`Samples~/Demo/README.md`).
- **Editor support is minimal.** A custom inspector for
  `NPCDefinition` and three menu items. No gizmos for aggro /
  attack ranges, no waypoint editor, no spawner visualisation.

---

## 10. Version history

- **0.1.0 (2026-11-21)** — First release. `NPCDefinition`
  ScriptableObject; `NPCController` state machine (Idle / Patrol /
  Chase / Attack / Flee / Dead) with NavMesh movement, patrol
  waypoints, aggro, attack, flee; `NPCHealth` implementing
  `IDamageable`; `NPCInteraction` dialogue / shop / quest hooks;
  `NPCSpawner`; editor inspector and menu items; demo driver and
  Play-mode self-test. **UNTESTED IN UNITY.**

---

*Sdx17 NPC Framework — Guide v0.1.0. If a section of this guide
disagrees with the code in `Runtime/`, the code is right — please
report the discrepancy (see `LISTING.md` for the support contact)
so the guide can be fixed.*
