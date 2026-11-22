using System;
using UnityEngine;
using UnityEngine.Events;

namespace Sdx17.NPC
{
    /// <summary>
    /// Health component for an NPC. Implements <see cref="IDamageable"/> so
    /// any damage source (Combat package, a trap, a test script) can hurt
    /// the NPC without knowing about <see cref="NPCController"/>.
    /// Health starts full; <see cref="Initialize"/> (called by the
    /// controller, or manually) sets the maximum from the definition.
    /// Death fires once and stays dead until <see cref="Revive"/> is called.
    /// </summary>
    [DisallowMultipleComponent]
    public class NPCHealth : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;

        [Header("Events")]
        [SerializeField] private UnityEvent<float> onDamaged;
        [SerializeField] private UnityEvent<float> onHealed;
        [SerializeField] private UnityEvent onDied;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead => currentHealth <= 0f;
        /// <summary>Health as a 0-1 fraction, useful for health bars and flee checks.</summary>
        public float HealthFraction => maxHealth > 0f ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;

        public UnityEvent<float> OnDamaged => onDamaged;
        public UnityEvent<float> OnHealed => onHealed;
        public UnityEvent OnDied => onDied;

        public event Action<float> Damaged;
        public event Action<float> Healed;
        public event Action Died;

        private void Awake()
        {
            // Clamp saved values: a scene saved with more health than the
            // maximum, or a negative value, would otherwise break the fraction.
            if (maxHealth < 1f) maxHealth = 1f;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        }

        /// <summary>Sets a new maximum (from an NPCDefinition) and refills health.</summary>
        public void Initialize(float newMaxHealth)
        {
            if (newMaxHealth < 1f) newMaxHealth = 1f;
            maxHealth = newMaxHealth;
            currentHealth = maxHealth;
        }

        public void TakeDamage(float amount, GameObject source)
        {
            if (IsDead) return;
            if (amount <= 0f) return;

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            onDamaged?.Invoke(amount);
            Damaged?.Invoke(amount);

            if (IsDead)
            {
                onDied?.Invoke();
                Died?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (IsDead) return;
            if (amount <= 0f) return;

            float healed = Mathf.Min(amount, maxHealth - currentHealth);
            if (healed <= 0f) return;

            currentHealth += healed;
            onHealed?.Invoke(healed);
            Healed?.Invoke(healed);
        }

        /// <summary>Restores full health after death, so a spawner can reuse the NPC.</summary>
        public void Revive()
        {
            currentHealth = maxHealth;
        }

        /// <summary>Sets health directly, clamped to 0..MaxHealth. Used by tests and save loading.</summary>
        public void SetHealth(float value)
        {
            currentHealth = Mathf.Clamp(value, 0f, maxHealth);
        }
    }
}
