using System;
using UnityEngine;

namespace Sdx17.NPC
{
    /// <summary>
    /// Contract for anything that can take damage. The NPC framework defines
    /// its own interface on purpose: the Sdx17 Combat package is not a
    /// dependency, so Combat (or any other damage source) talks to NPCs
    /// through this interface instead of through a concrete NPC type.
    /// A Combat system can call <see cref="TakeDamage"/> on any
    /// <see cref="IDamageable"/> it hits, and an NPC can likewise damage a
    /// player that implements the same interface.
    /// </summary>
    public interface IDamageable
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        bool IsDead { get; }

        /// <summary>Fired after health changes from damage. Argument is the amount taken.</summary>
        event Action<float> Damaged;
        /// <summary>Fired after health changes from healing. Argument is the amount healed.</summary>
        event Action<float> Healed;
        /// <summary>Fired once when health reaches zero.</summary>
        event Action Died;

        /// <summary>Applies damage. Negative or zero amounts are ignored.</summary>
        void TakeDamage(float amount, GameObject source);
        /// <summary>Restores health, clamped to MaxHealth. Does nothing when dead.</summary>
        void Heal(float amount);
    }
}
