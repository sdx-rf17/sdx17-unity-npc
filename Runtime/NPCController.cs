using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Sdx17.NPC
{
    /// <summary>States of the NPC state machine.</summary>
    public enum NPCState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Flee,
        Dead
    }

    /// <summary>
    /// Drives one NPC: state machine, NavMesh movement, patrol waypoints,
    /// aggro detection, attacking, fleeing on low health, and death.
    ///
    /// Behaviour summary:
    /// - Idle: stands still, watches for a target in aggro range.
    /// - Patrol: walks between waypoints, waiting at each one.
    /// - Chase: moves toward the target while it is in aggro range and
    ///   within the chase limit; gives up past that limit.
    /// - Attack: in attack range, deals damage on a cooldown through
    ///   <see cref="IDamageable"/> on the target (no Combat dependency).
    /// - Flee: when health drops below the definition's flee fraction,
    ///   moves away from the target until out of aggro range, then recovers.
    /// - Dead: stops the NavMeshAgent, fires the death event, stays down
    ///   until <see cref="Revive"/> is called (for example by a spawner).
    ///
    /// The target is found by tag ("Player" by default) or assigned
    /// directly with <see cref="SetTarget"/>. If no NavMeshAgent is present
    /// the NPC still runs its state machine but cannot move — a warning is
    /// logged once so the missing component is easy to spot.
    /// </summary>
    [RequireComponent(typeof(NPCHealth))]
    [DisallowMultipleComponent]
    public class NPCController : MonoBehaviour
    {
        [SerializeField] private NPCDefinition definition;
        [SerializeField] private Transform[] patrolWaypoints;
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private Transform targetOverride;

        [Header("Events")]
        [SerializeField] private UnityEvent<NPCState> onStateChanged;
        [SerializeField] private UnityEvent onAggro;
        [SerializeField] private UnityEvent onDeath;

        private NPCHealth health;
        private NavMeshAgent agent;
        private Transform target;
        private NPCState state = NPCState.Idle;
        private int currentWaypointIndex;
        private float waitTimer;
        private float attackTimer;
        private bool agentWarningLogged;

        public NPCDefinition Definition => definition;
        public NPCState State => state;
        public Transform Target => target;
        public NPCHealth Health => health;
        public UnityEvent<NPCState> OnStateChanged => onStateChanged;
        public UnityEvent OnAggro => onAggro;
        public UnityEvent OnDeath => onDeath;

        public event Action<NPCState> StateChanged;

        private void Awake()
        {
            health = GetComponent<NPCHealth>();
            agent = GetComponent<NavMeshAgent>();
            if (health == null)
            {
                Debug.LogError($"[Sdx17 NPC] {name} has no NPCHealth component.", this);
            }
        }

        private void Start()
        {
            ApplyDefinition();
            if (targetOverride != null)
            {
                target = targetOverride;
            }
            else if (!string.IsNullOrEmpty(playerTag))
            {
                GameObject player = GameObject.FindGameObjectWithTag(playerTag);
                if (player != null) target = player.transform;
            }

            if (health != null)
            {
                health.Died += HandleDeath;
                health.Damaged += HandleDamaged;
            }

            SetState(patrolWaypoints != null && patrolWaypoints.Length > 0 ? NPCState.Patrol : NPCState.Idle);
            MoveToCurrentWaypoint();
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Died -= HandleDeath;
                health.Damaged -= HandleDamaged;
            }
        }

        /// <summary>Assigns a definition and applies its values to health and movement.</summary>
        public void SetDefinition(NPCDefinition newDefinition)
        {
            definition = newDefinition;
            ApplyDefinition();
        }

        /// <summary>Assigns patrol waypoints at runtime. Pass null or an empty array to stop patrolling.</summary>
        public void SetPatrolWaypoints(Transform[] waypoints)
        {
            patrolWaypoints = waypoints;
            currentWaypointIndex = 0;
            if (state == NPCState.Idle && patrolWaypoints != null && patrolWaypoints.Length > 0)
            {
                SetState(NPCState.Patrol);
                MoveToCurrentWaypoint();
            }
        }

        /// <summary>Overrides the chase / attack target. Pass null to clear it.</summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void ApplyDefinition()
        {
            if (definition == null)
            {
                Debug.LogWarning($"[Sdx17 NPC] {name} has no NPCDefinition assigned. Using default values.", this);
                return;
            }

            definition.Validate();
            if (health != null) health.Initialize(definition.MaxHealth);
            if (agent != null) agent.speed = definition.MoveSpeed;
        }

        private void Update()
        {
            if (state == NPCState.Dead) return;
            if (definition == null) return;

            switch (state)
            {
                case NPCState.Idle:
                    TickIdle();
                    break;
                case NPCState.Patrol:
                    TickPatrol();
                    break;
                case NPCState.Chase:
                    TickChase();
                    break;
                case NPCState.Attack:
                    TickAttack();
                    break;
                case NPCState.Flee:
                    TickFlee();
                    break;
            }
        }

        private void TickIdle()
        {
            if (TryAggro()) return;
            // Idle NPCs stand still; the wait timer is not needed here.
        }

        private void TickPatrol()
        {
            if (TryAggro()) return;
            if (agent == null || !agent.isOnNavMesh) return;

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.05f)
            {
                waitTimer += Time.deltaTime;
                if (waitTimer >= definition.PatrolWaitTime)
                {
                    waitTimer = 0f;
                    AdvanceWaypoint();
                }
            }
        }

        private void TickChase()
        {
            if (target == null)
            {
                ReturnToPatrolOrIdle();
                return;
            }

            float distance = Vector3.Distance(transform.position, target.position);
            if (distance > definition.ChaseMaxDistance)
            {
                ReturnToPatrolOrIdle();
                return;
            }

            if (distance <= definition.AttackRange)
            {
                attackTimer = 0f;
                SetState(NPCState.Attack);
                return;
            }

            StopAgentIfNeeded(false);
            SetDestination(target.position);
        }

        private void TickAttack()
        {
            if (target == null)
            {
                ReturnToPatrolOrIdle();
                return;
            }

            float distance = Vector3.Distance(transform.position, target.position);
            if (distance > definition.AttackRange + 0.5f)
            {
                SetState(NPCState.Chase);
                return;
            }

            // Face the target while attacking.
            Vector3 direction = target.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction),
                    Time.deltaTime * 8f);
            }

            attackTimer += Time.deltaTime;
            if (attackTimer >= definition.AttackCooldown)
            {
                attackTimer = 0f;
                DealDamageToTarget();
            }
        }

        private void TickFlee()
        {
            if (target == null)
            {
                ReturnToPatrolOrIdle();
                return;
            }

            float distance = Vector3.Distance(transform.position, target.position);
            if (distance > definition.AggroRange * 1.5f)
            {
                ReturnToPatrolOrIdle();
                return;
            }

            // Run directly away from the target.
            Vector3 away = transform.position - target.position;
            away.y = 0f;
            if (away.sqrMagnitude < 0.001f) away = transform.forward;
            SetDestination(transform.position + away.normalized * definition.AggroRange);
        }

        /// <summary>Checks whether the target is close enough to aggro. Returns true if aggro started.</summary>
        private bool TryAggro()
        {
            if (target == null) return false;
            if (health != null && health.IsDead) return false;

            float distance = Vector3.Distance(transform.position, target.position);
            if (distance > definition.AggroRange) return false;

            if (definition.IsAggressive)
            {
                onAggro?.Invoke();
                SetState(distance <= definition.AttackRange ? NPCState.Attack : NPCState.Chase);
                return true;
            }

            return false;
        }

        private void HandleDamaged(float amount)
        {
            if (state == NPCState.Dead) return;
            if (definition == null || health == null) return;

            // Being hit makes a passive NPC flee once its health is low,
            // and makes an aggressive NPC chase its attacker.
            if (definition.FleeHealthFraction > 0f && health.HealthFraction <= definition.FleeHealthFraction)
            {
                SetState(NPCState.Flee);
            }
            else if (definition.IsAggressive && target != null)
            {
                SetState(NPCState.Chase);
            }
        }

        private void HandleDeath()
        {
            SetState(NPCState.Dead);
            StopAgentIfNeeded(true);
            onDeath?.Invoke();
        }

        private void DealDamageToTarget()
        {
            if (target == null || definition == null) return;
            // Damage goes through IDamageable so any target that implements
            // it (player health, another NPC, a Combat health component)
            // can receive damage without this class knowing its type.
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null && !damageable.IsDead)
            {
                damageable.TakeDamage(definition.AttackDamage, gameObject);
            }
        }

        /// <summary>Brings a dead NPC back to life at full health, returning to patrol or idle.</summary>
        public void Revive()
        {
            if (health == null) return;
            health.Revive();
            StopAgentIfNeeded(false);
            ReturnToPatrolOrIdle();
        }

        private void ReturnToPatrolOrIdle()
        {
            if (patrolWaypoints != null && patrolWaypoints.Length > 0)
            {
                SetState(NPCState.Patrol);
                MoveToCurrentWaypoint();
            }
            else
            {
                SetState(NPCState.Idle);
                StopAgentIfNeeded(true);
            }
        }

        private void AdvanceWaypoint()
        {
            if (patrolWaypoints == null || patrolWaypoints.Length == 0) return;
            currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
            MoveToCurrentWaypoint();
        }

        private void MoveToCurrentWaypoint()
        {
            if (patrolWaypoints == null || patrolWaypoints.Length == 0) return;
            Transform waypoint = patrolWaypoints[currentWaypointIndex];
            if (waypoint == null) return;
            SetDestination(waypoint.position);
        }

        private void SetDestination(Vector3 destination)
        {
            if (agent == null)
            {
                LogAgentWarningOnce();
                return;
            }
            if (!agent.isOnNavMesh) return;
            agent.isStopped = false;
            agent.SetDestination(destination);
        }

        private void StopAgentIfNeeded(bool stop)
        {
            if (agent == null)
            {
                LogAgentWarningOnce();
                return;
            }
            agent.isStopped = stop;
        }

        private void LogAgentWarningOnce()
        {
            if (agentWarningLogged) return;
            agentWarningLogged = true;
            Debug.LogWarning($"[Sdx17 NPC] {name} has no NavMeshAgent, so it cannot move. Add a NavMeshAgent and bake a NavMesh (see Samples~/Demo/README.md).", this);
        }

        private void SetState(NPCState newState)
        {
            if (state == newState) return;
            state = newState;
            onStateChanged?.Invoke(newState);
            StateChanged?.Invoke(newState);
        }
    }
}
