# Sdx17 NPC Framework

> **Version 0.1.0 — UNTESTED IN UNITY**
>
> This package was written without Unity available in this environment.
> No Editor import, compile, or Play-mode test has been run. Do not treat
> it as tested or production-ready until the TEST checklist below has been
> completed in Unity 2022.3 LTS.

NPC Framework for Unity 2022.3 LTS (URP-compatible, no paid dependencies):
data-driven NPCs with a state machine, NavMesh movement, patrol waypoints,
aggro range, health, a spawner, and dialogue / shop / quest hooks that work
without hard dependencies on other Sdx17 systems.

## What it is

- `NPCDefinition` ScriptableObject: one asset per NPC type, shared by many NPCs.
- `NPCController` state machine: **Idle / Patrol / Chase / Attack / Flee / Dead**.
- NavMeshAgent movement, patrol waypoints, aggro range, chase limit.
- `NPCHealth` implementing a local `IDamageable` interface (see Integration).
- `NPCSpawner`: spawn points, max alive, spawn interval, respawn timer.
- `NPCInteraction`: dialogue, shop, and quest-giver hooks via UnityEvents
  and C# events carrying string ids.
- Editor support: custom inspector for `NPCDefinition`, menu items to create
  an NPC Definition, an NPC GameObject, and an NPC Spawner.
- Demo driver and Play-mode self-test (see below).

## What it is NOT

- Not a complete game, AI, animation, or dialogue system. There is no
  animation controller, no dialogue UI, no shop UI, and no quest logic here —
  those systems consume the events this package fires.
- Not tested in Unity (see header). Import errors, NavMesh setup issues,
  and Play-mode behaviour still need to be verified by hand.
- Not a Combat package dependency: this package never references Combat
  types. Combat talks to NPCs through `IDamageable`.
- No demo scene file is included. The demo is a script plus a setup guide
  (`Samples~/Demo/README.md`).

## Setup (under 5 minutes)

1. **Install the package.** In Unity 2022.3 LTS, open Window > Package
   Manager > + > Add package from disk, and select this folder's
   `package.json`. Or copy the folder into your project's `Packages/`
   directory.
2. **Create a definition.** Right-click in the Project window >
   Create > Sdx17 > NPC Definition. Set a name, health, speed, aggro /
   attack ranges, and (if wanted) dialogue / shop / quest ids.
3. **Create an NPC.** GameObject > Sdx17 > NPC > Create NPC (with
   Controller + Health). Drag the definition onto the NPCController's
   Definition field.
4. **Bake a NavMesh** on your ground (Window > AI > Navigation > Bake).
   Without a baked NavMesh the NPC logs a warning and cannot move.
5. **Tag your player `Player`** (or assign a target with
   `NPCController.SetTarget`). Press Play.

## File table

| Path | Purpose |
| --- | --- |
| `package.json` | UPM package manifest (`com.sdx17.npc`, 0.1.0, Unity 2022.3). |
| `Runtime/NPCDefinition.cs` | ScriptableObject data asset for one NPC type. |
| `Runtime/IDamageable.cs` | Damage contract — the only way other systems hurt an NPC. |
| `Runtime/NPCHealth.cs` | Health component implementing `IDamageable`, with damaged / healed / died events. |
| `Runtime/NPCController.cs` | State machine (Idle / Patrol / Chase / Attack / Flee / Dead), NavMesh movement, aggro, attack, flee, revive. |
| `Runtime/NPCInteraction.cs` | Dialogue / shop / quest hooks (UnityEvents + C# events, string ids). |
| `Runtime/NPCSpawner.cs` | Spawn points, max alive, spawn interval, respawn timer. |
| `Runtime/Sdx17.NPC.Runtime.asmdef` | Runtime assembly definition (`Sdx17.NPC.Runtime`). |
| `Editor/NPCDefinitionEditor.cs` | Custom inspector for `NPCDefinition` with validation warnings. |
| `Editor/NPCMenu.cs` | Menu items: create NPC Definition, NPC GameObject, NPC Spawner. |
| `Editor/Sdx17.NPC.Editor.asmdef` | Editor assembly definition (references runtime). |
| `Samples~/Demo/NPCDemo.cs` | Demo driver: I = interact, K = deal 25 damage, logs state changes and hook requests. |
| `Samples~/Demo/README.md` | Step-by-step demo scene setup (no scene file included). |
| `Tests~/NPCSelfTest.cs` | Play-mode self-test MonoBehaviour logging PASS / FAIL per check. |
| `CHANGELOG.md`, `LICENSE`, `.gitignore` | Release notes, MIT licence, Unity-standard ignores. |

## Config options (`NPCDefinition`)

| Field | Default | Meaning |
| --- | --- | --- |
| Display Name | `NPC` | Name shown in logs / UI by consuming systems. |
| Description | — | Free text for designers. |
| Max Health | 100 | Health set by `NPCHealth.Initialize` when the definition is applied. |
| Flee Health Fraction | 0.25 | Below this fraction of max health the NPC flees when damaged. 0 = never flees. |
| Move Speed | 3.5 | NavMeshAgent speed. |
| Patrol Wait Time | 2 s | Pause at each waypoint before moving to the next. |
| Is Aggressive | off | On: chases / attacks a target inside Aggro Range. Off: only flees or defends when damaged. |
| Aggro Range | 10 | Distance at which an aggressive NPC notices the target. |
| Attack Range | 2 | Distance at which the NPC switches from Chase to Attack. |
| Attack Damage | 10 | Damage dealt per attack through `IDamageable`. |
| Attack Cooldown | 1 s | Seconds between attacks. |
| Chase Max Distance | 25 | Gives up the chase beyond this distance and returns to patrol / idle. |
| Is Merchant / Is Quest Giver / Has Dialogue | off / off / on | Which interaction roles the NPC has. |
| Dialogue Id / Shop Id / Quest Id | — | String ids handed to the Dialogue / Inventory / Quest systems via events. Empty = no hook for that role. |

`NPCSpawner` config: NPC Prefab, Spawn Points, Max Alive (3), Spawn
Interval (5 s), Respawn Delay (10 s), Corpse Lifetime (5 s), Spawn On Start.

## Public API

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

## Integration with other Sdx17 systems

All integration is by **events and string ids** — install none, some, or
all of the other systems and the NPC still works.

- **Combat (health):** Combat never references NPC types. It calls
  `IDamageable.TakeDamage(amount, source)` on whatever it hits, and an
  NPC attacks the same way: `NPCController` looks for `IDamageable` on
  its target. Give the player an `IDamageable` health component if NPCs
  should be able to damage the player; otherwise attacks find nothing
  and do nothing. `NPCHealth` fires `OnDamaged` / `OnHealed` / `OnDied`
  (UnityEvents) and `Damaged` / `Healed` / `Died` (C# events).
- **Dialogue:** set `Dialogue Id` on the definition and wire
  `NPCInteraction.OnDialogueRequested` (UnityEvent) or
  `DialogueRequested` (C# event) to your dialogue system's open method.
  With nothing wired, `Interact()` logs which dialogue was requested.
- **Quest (quest giver):** set `Is Quest Giver` and `Quest Id`, wire
  `OnQuestRequested` / `QuestRequested` to the quest system.
- **Inventory (shop):** set `Is Merchant` and `Shop Id`, wire
  `OnShopRequested` / `ShopRequested` to the shop / inventory system.
- **Interaction priority:** `Interact()` tries dialogue first, then
  shop, then quest. Call `TryOpenDialogue()` / `TryOpenShop()` /
  `TryOfferQuest()` directly to trigger one hook only.

## TEST checklist (run in Unity 2022.3 LTS)

- [ ] Import: package imports with **zero compile errors** in the Console.
- [ ] Definition: create an NPC Definition asset; the custom inspector
      shows grouped sections and a warning when Attack Range > Aggro Range.
- [ ] Menu: GameObject > Sdx17 > NPC creates a Definition asset, an NPC
      GameObject (Health + Controller + Interaction + NavMeshAgent), and a
      Spawner.
- [ ] Demo scene: follow `Samples~/Demo/README.md` (plane, baked NavMesh,
      Player-tagged capsule, NPC, `NPCDemo` driver). Press Play.
- [ ] Patrol: NPC with waypoints walks between them and waits at each one.
- [ ] Aggro: aggressive NPC chases the player inside Aggro Range and
      returns to patrol past Chase Max Distance.
- [ ] Attack: NPC in Attack Range deals damage on its cooldown to a target
      with an `IDamageable` component.
- [ ] Flee: damage the NPC below its flee-health fraction (demo: press K);
      it flees, then recovers when the player is far away.
- [ ] Death: lethal damage sets state Dead, stops the agent, fires
      `OnDeath`. Spawner removes the corpse after Corpse Lifetime and
      respawns after Respawn Delay.
- [ ] Interaction: press I near the NPC; the demo logs the dialogue /
      shop / quest id from the definition. Wire one event to a real system
      and confirm it opens.
- [ ] Self-test: add `NPCSelfTest` to a GameObject, press Play, confirm
      the summary line reads **0 FAIL**.
- [ ] Spawner: set Max Alive to 3, kill NPCs, confirm at most 3 are alive
      and respawns keep coming.

## Licence

MIT — Copyright 2026 Sdx17. See `LICENSE`.
