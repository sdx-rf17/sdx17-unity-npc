# Changelog

All notable changes to the Sdx17 NPC Framework will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [0.1.0] - 2026-11-21

### Added

- `NPCDefinition` ScriptableObject: name, health, speed, aggro / attack ranges, damage, cooldown, flee threshold, patrol wait time, and dialogue / shop / quest ids.
- `NPCController` state machine: Idle, Patrol, Chase, Attack, Flee, Dead, with NavMeshAgent movement, patrol waypoints, and aggro-range detection.
- `NPCHealth` implementing the local `IDamageable` interface, with damaged / healed / died UnityEvents. No hard reference to the Sdx17 Combat package.
- `NPCInteraction`: dialogue, shop, and quest-giver hooks fired as UnityEvents / C# events carrying string ids, so the NPC works with or without those systems installed.
- `NPCSpawner`: spawn points, maximum alive count, spawn interval, and respawn timer.
- Editor support: custom inspector for `NPCDefinition` and menu items to create an NPC Definition asset and an NPC Spawner GameObject.
- `Samples~/Demo`: demo script and setup guide for a minimal demo scene (no scene file is included).
- `Tests~`: `NPCSelfTest` Play-mode MonoBehaviour that logs PASS / FAIL for core paths.

### Status

- Version 0.1.0 — **UNTESTED IN UNITY**. No Unity Editor or Play-mode test has been run in this environment. Do not treat this as tested or production-ready until the TEST checklist in `README.md` has been completed in Unity 2022.3 LTS.
