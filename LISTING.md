# Sdx17 NPC Framework — Asset Store Listing (DRAFT)

> **DRAFT ONLY — no Asset Store listing exists yet.**
> Do not submit this text until every item in
> `SELLING_REQUIREMENTS.md` has passed. Version 0.1.0 is
> **UNTESTED IN UNITY** — nothing in this listing may claim the
> package has been tested, reviewed, or sold.

---

## Title

**Sdx17 NPC Framework**

## Short description (under 200 characters)

Data-driven NPCs for Unity: state machine, NavMesh patrol, aggro, health, spawner, and dialogue, shop and quest hooks — no paid dependencies.

*(141 characters — verified by character count before submission;
re-count if this line is edited.)*

## Long description

**Sdx17 NPC Framework** gives your Unity game data-driven NPCs in
minutes — no paid dependencies, no hard dependencies on other
systems.

Create one `NPCDefinition` asset per NPC type (a Village Guard, a
Wandering Merchant, a Quest Giver), drop it on an NPC, bake a
NavMesh, and press Play. The NPC patrols its waypoints, notices
the player inside its aggro range, chases, attacks on a cooldown,
flees when its health gets low, and dies — all driven by a
six-state machine: **Idle / Patrol / Chase / Attack / Flee / Dead**.

**What you get**

- **Data-driven design** — one ScriptableObject per NPC type, shared
  by many NPCs. Balance changes are one edit in one asset.
- **State machine** — `NPCController` with Idle, Patrol, Chase,
  Attack, Flee and Dead, with UnityEvents and C# events on every
  state change.
- **NavMesh movement** — patrol waypoints with wait times, aggro
  range, chase limit, and flee behaviour.
- **Health** — `NPCHealth` implementing a local `IDamageable`
  interface, so any damage source (combat system, trap, test
  script) can hurt an NPC without knowing about NPC types. Health
  bars read `HealthFraction` (0–1).
- **Spawner** — spawn points, max alive, spawn interval, respawn
  timer, and corpse lifetime. One spawner keeps a village or a
  dungeon populated.
- **Interaction hooks** — dialogue, shop, and quest-giver hooks
  fired as UnityEvents and C# events carrying string ids. Wire
  them to your dialogue, inventory, or quest system in the
  Inspector — or install nothing at all and the NPC still works.
- **Editor support** — custom inspector for `NPCDefinition` with
  validation warnings, and menu items to create a definition, an
  NPC, or a spawner.
- **Demo and self-test** — a demo driver script with a step-by-step
  scene setup guide, and a Play-mode self-test that logs PASS /
  FAIL per check.

**What it is not**

This is a framework, not a finished game. There is no animation
controller, no dialogue UI, no shop UI, and no quest logic here —
those systems consume the events this package fires. There is no
line-of-sight check (aggro is distance-based) and no networking;
multiplayer use needs a server-authoritative wrapper (see the
Guide). There is no demo scene file — the demo is a script plus a
setup guide, and you build the scene by hand in about 5 minutes.

**Honest status**

Version 0.1.0 is **untested in Unity**. The code was written
without Unity available, and no Editor import, compile, or
Play-mode test has been run yet. The package will not be submitted
to the Asset Store until it passes the full checklist in
`SELLING_REQUIREMENTS.md` (clean import in Unity 2022.3 LTS, demo
scene playing, real screenshots and video). Buy the package for
what the code does — the full Guide documents every public class
and method from the actual source — not for any claim of testing,
sales, or reviews, because there are none yet.

## Keywords (10)

1. NPC
2. AI
3. state machine
4. NavMesh
5. patrol
6. spawner
7. dialogue
8. quest giver
9. merchant
10. enemy AI

## Category

**Tools / Game Toolkits** (suggested — re-check at listing time;
the Asset Store category list changes, and the publisher portal
may suggest a closer fit such as Tools / AI).

## Suggested price

**$25**

### Price justification

- **New publisher, no reviews yet.** A new publisher should price
  in the **$15–$35** band to get first reviews, **not $95 on day
  one**. At $95 with zero reviews, zero sales history, and an
  untested v0.1.0, buyers have no reason to take the risk; at $25
  the purchase is a low-risk try for a working NPC framework.
- **Comparable scope.** NPC / enemy-AI toolkits on the Asset Store
  commonly list well above this band once established, with
  broader feature sets (animation, behaviour trees, demo scenes)
  and review history behind them. This package is narrower — a
  focused state machine + health + spawner + hooks framework, no
  animation, no demo scene file — so it is priced below that
  established tier, not alongside it.
- **What $25 buys:** a complete, documented framework (6 Runtime
  classes, editor support, demo driver, self-test, full Guide)
  that saves a developer the day or two it would take to write,
  wire, and balance the same state machine, health, spawner, and
  hooks from scratch.
- **Net revenue:** Unity's Asset Store publisher share is 70%.
  At $25 the publisher net is **$17.50 per sale before tax and
  refunds**. Price is not a guarantee of sales — at $25 the
  package still needs real screenshots, a real video, and the
  first honest reviews before it will sell on its own.
- **Re-check before listing.** Re-check comparable NPC/AI toolkit
  prices and the $15–$35 first-review band at listing time; if
  the market has moved, adjust the price and update this section
  rather than submitting a stale justification.

## What's included

- `Runtime/` — 6 C# classes (`NPCDefinition`, `NPCController`,
  `NPCHealth`, `NPCInteraction`, `NPCSpawner`, `IDamageable`),
  namespace `Sdx17.NPC`, assembly `Sdx17.NPC.Runtime`.
- `Editor/` — custom inspector for `NPCDefinition`, menu items
  (create definition / NPC / spawner), assembly
  `Sdx17.NPC.Editor`.
- `Samples~/Demo/` — demo driver script (`NPCDemo.cs`) and a
  step-by-step demo scene setup guide (`README.md`). No `.unity`
  scene file is included.
- `Tests~/` — Play-mode self-test (`NPCSelfTest.cs`) logging
  PASS / FAIL per check.
- `Documentation~/GUIDE.md` — full user guide: quickstart,
  concepts, complete API reference, config reference, code
  examples, integration with the other Sdx17 systems,
  troubleshooting / FAQ, performance notes, limitations.
- `README.md`, `CHANGELOG.md`, `LICENSE` (MIT),
  `ThirdPartyNotices.txt`.

## Requirements / Dependencies

- **Unity 2022.3 LTS** (package targets `unity: 2022.3`).
- **Render pipeline:** URP or Built-in compatible. No
  render-pipeline-specific code.
- **NavMesh:** the AI Navigation module must be enabled (it ships
  as a built-in module in Unity 2022.3); a baked NavMesh is
  required for NPC movement.
- **No paid dependencies.** No other packages are required. The
  Dialogue, Inventory, Quest, Combat, and other Sdx17 systems are
  all optional — integration is by events and string ids, and the
  NPC works with none of them installed.
- **Platforms:** any platform Unity 2022.3 supports that has
  NavMesh (desktop and mobile in principle — not tested on any
  platform yet; see the honest status note above).

## Demo scene description

The package includes a demo driver, not a scene file. The demo
scene is built by hand in about 5 minutes following
`Samples~/Demo/README.md`: a 30x30 ground plane with a baked
NavMesh, a capsule player tagged `Player`, one NPC with a
definition (aggressive, Aggro Range 10, Attack Range 2), three
patrol waypoints, and the `NPCDemo` driver. In Play mode the NPC
patrols, chases and attacks the player in range; pressing **I**
within 3 m interacts (logs the dialogue / shop / quest id) and
pressing **K** within 3 m deals 25 damage, showing flee and death.
An optional spawner (Max Alive 3) respawns NPCs after the respawn
delay. The screenshots and video for this listing must be captured
from this real demo scene in Unity — see
`SELLING_REQUIREMENTS.md`.

## Support contact

- **Publisher:** Sdx17
- **GitHub:** https://github.com/sdx-rf17
- **Email:** none listed — do not invent one. Support is via
  GitHub (issues on the repository) until a real support email
  exists. No fake email address may be added to this listing.

## Version history

- **0.1.0 (2026-11-21)** — First release. `NPCDefinition`
  ScriptableObject; `NPCController` state machine (Idle / Patrol /
  Chase / Attack / Flee / Dead) with NavMesh movement, patrol
  waypoints, aggro, attack, flee; `NPCHealth` implementing
  `IDamageable`; `NPCInteraction` dialogue / shop / quest hooks;
  `NPCSpawner`; editor inspector and menu items; demo driver and
  Play-mode self-test. **UNTESTED IN UNITY — not yet submitted.**

No earlier versions exist. No sales or reviews exist yet, and none
are claimed.
