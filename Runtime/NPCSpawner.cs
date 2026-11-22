using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Sdx17.NPC
{
    /// <summary>
    /// Spawns NPCs from a prefab at configured spawn points, keeps at most
    /// <see cref="MaxAlive"/> alive at once, and respawns on a timer after
    /// an NPC dies or is removed.
    ///
    /// The spawner listens to each spawned NPC's health death event, so a
    /// dead NPC is counted as gone immediately and its GameObject is
    /// destroyed after <see cref="corpseLifetime"/> seconds (0 destroys it
    /// at once). Respawned NPCs are placed at a random spawn point.
    /// Assign the NPC prefab, at least one spawn point, and the spawner
    /// does the rest — no other setup is required.
    /// </summary>
    [DisallowMultipleComponent]
    public class NPCSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject npcPrefab;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField, Min(1)] private int maxAlive = 3;
        [SerializeField, Min(0f)] private float spawnInterval = 5f;
        [SerializeField, Min(0f)] private float respawnDelay = 10f;
        [SerializeField, Min(0f)] private float corpseLifetime = 5f;
        [SerializeField] private bool spawnOnStart = true;

        [Header("Events")]
        [SerializeField] private UnityEvent<GameObject> onSpawned;
        [SerializeField] private UnityEvent<GameObject> onDespawned;

        private readonly List<GameObject> alive = new List<GameObject>();
        private float spawnTimer;
        private float respawnTimer;
        private bool pendingRespawn;

        public GameObject NpcPrefab => npcPrefab;
        public int MaxAlive => maxAlive;
        public int AliveCount => alive.Count;
        public IReadOnlyList<GameObject> Alive => alive.AsReadOnly();
        public UnityEvent<GameObject> OnSpawned => onSpawned;
        public UnityEvent<GameObject> OnDespawned => onDespawned;

        private void Start()
        {
            if (spawnOnStart)
            {
                SpawnUpToMax();
            }
        }

        private void Update()
        {
            // Keep the population topped up on a slow interval, and honour
            // the longer respawn delay after a death.
            if (AliveCount < maxAlive)
            {
                if (pendingRespawn)
                {
                    respawnTimer += Time.deltaTime;
                    if (respawnTimer >= respawnDelay)
                    {
                        pendingRespawn = false;
                        respawnTimer = 0f;
                        SpawnOne();
                    }
                }
                else
                {
                    spawnTimer += Time.deltaTime;
                    if (spawnTimer >= spawnInterval)
                    {
                        spawnTimer = 0f;
                        SpawnOne();
                    }
                }
            }
        }

        /// <summary>Spawns one NPC at a random spawn point, if under the alive limit.</summary>
        public GameObject SpawnOne()
        {
            if (npcPrefab == null)
            {
                Debug.LogError($"[Sdx17 NPC] {name} Spawner has no NPC Prefab assigned.", this);
                return null;
            }
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError($"[Sdx17 NPC] {name} Spawner has no Spawn Points assigned.", this);
                return null;
            }
            if (AliveCount >= maxAlive) return null;

            Transform point = RandomSpawnPoint();
            if (point == null) return null;

            GameObject instance = Instantiate(npcPrefab, point.position, point.rotation);
            alive.Add(instance);

            NPCHealth health = instance.GetComponent<NPCHealth>();
            if (health != null)
            {
                health.Died += () => HandleNpcDied(instance);
            }

            onSpawned?.Invoke(instance);
            return instance;
        }

        /// <summary>Spawns NPCs until the alive limit is reached. Returns how many were spawned.</summary>
        public int SpawnUpToMax()
        {
            int spawned = 0;
            while (AliveCount < maxAlive)
            {
                if (SpawnOne() == null) break;
                spawned++;
            }
            return spawned;
        }

        /// <summary>Removes an NPC immediately (destroys its GameObject) and schedules a respawn.</summary>
        public void Despawn(GameObject npc)
        {
            if (npc == null) return;
            if (!alive.Remove(npc)) return;

            onDespawned?.Invoke(npc);
            Destroy(npc);
            ScheduleRespawn();
        }

        /// <summary>Destroys all alive NPCs and clears the respawn schedule.</summary>
        public void DespawnAll()
        {
            foreach (GameObject npc in alive)
            {
                if (npc != null)
                {
                    onDespawned?.Invoke(npc);
                    Destroy(npc);
                }
            }
            alive.Clear();
            pendingRespawn = false;
            respawnTimer = 0f;
        }

        private void HandleNpcDied(GameObject npc)
        {
            if (npc == null) return;
            // The corpse stays for corpseLifetime seconds, then the NPC is
            // removed from the alive list and a respawn is scheduled.
            StartCoroutine(RemoveAfterDelay(npc, corpseLifetime));
        }

        private System.Collections.IEnumerator RemoveAfterDelay(GameObject npc, float delay)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            if (npc == null) yield break;

            alive.Remove(npc);
            onDespawned?.Invoke(npc);
            Destroy(npc);
            ScheduleRespawn();
        }

        private void ScheduleRespawn()
        {
            if (respawnDelay <= 0f)
            {
                SpawnOne();
                return;
            }
            pendingRespawn = true;
            respawnTimer = 0f;
        }

        private Transform RandomSpawnPoint()
        {
            if (spawnPoints == null || spawnPoints.Length == 0) return null;
            // Skip null entries instead of crashing on a half-filled list.
            for (int attempt = 0; attempt < spawnPoints.Length; attempt++)
            {
                Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
                if (point != null) return point;
            }
            return null;
        }
    }
}
