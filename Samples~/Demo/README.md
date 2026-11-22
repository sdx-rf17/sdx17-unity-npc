# NPC Framework — Demo

Minimal demo scene setup. **No `.unity` scene file is included** — build the
scene by hand with the steps below (about 5 minutes).

## Scene setup

1. Create a new scene (File > New Scene) and save it, for example as `NPCDemo.unity`.
2. Create a ground plane: GameObject > 3D Object > Plane. Scale it to about
   30 x 30. This is the walkable surface.
3. **Bake a NavMesh** (required for movement — without it the NPC logs a
   warning and stands still):
   - Select the Plane, open Window > AI > Navigation, go to the Bake tab,
     and press Bake. The plane must be marked Navigation Static (it is by
     default for a new plane in most Unity versions; check the Inspector).
   - If the Navigation window is missing, install the AI Navigation package
     via Window > Package Manager (Unity 2022.3: it ships as a built-in
     module — enable it under Project Settings if needed).
4. Create a player: GameObject > 3D Object > Capsule, name it `Player`,
   set its Tag to `Player`, and add a simple movement script or move it
   with the Transform tools in Play mode. Give it an `NPCHealth` component
   only if you want the NPC to be able to damage it (the controller looks
   for `IDamageable` on the target).
5. Create the NPC: GameObject > Sdx17 > NPC > Create NPC (with Controller + Health).
   - Assign an NPC Definition: GameObject > Sdx17 > NPC > Create NPC Definition,
     set its values (try Aggro Range 10, Attack Range 2, Is Aggressive on),
     then drag the asset onto the NPCController's Definition field.
   - Patrol (optional): create 3 empty GameObjects as waypoints around the
     plane, drag them into the NPCController's Patrol Waypoints list.
6. Add the demo driver: create an empty GameObject named `Demo`, add the
   `NPCDemo` script (in `Samples~/Demo/NPCDemo.cs`), and drag the NPC and
   the Player into its fields (it also finds them automatically if left empty).
7. Optional — spawner: GameObject > Sdx17 > NPC > Create NPC Spawner.
   Drag the NPC GameObject into the Project window to make it a prefab,
   assign that prefab to the Spawner, add 2–3 spawn-point empty GameObjects,
   and set Max Alive to 3.

## What to try in Play mode

- Walk the player near the NPC: an aggressive NPC should Chase, then Attack
  in range. A passive NPC with waypoints should Patrol.
- Press **I** within 3 m of the NPC: the demo logs which hook fired
  (dialogue / shop / quest, from the definition's ids).
- Press **K** within 3 m: deals 25 damage. Watch the NPC Flee below its
  flee-health fraction, and die at 0 health (state Dead, agent stopped).
- With a spawner: kill the NPC and watch a new one respawn after the
  respawn delay.

## Files

| File | Purpose |
| --- | --- |
| `NPCDemo.cs` | Demo driver: finds the NPC and player, logs state changes and hook requests, I = interact, K = deal 25 damage. |
| `README.md` | This file. |
