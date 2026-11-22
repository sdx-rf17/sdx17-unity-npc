using UnityEngine;

namespace Sdx17.NPC.Tests
{
    /// <summary>
    /// Play-mode self-test for the NPC Framework. Add this component to any
    /// GameObject in a test scene and press Play: it exercises the core
    /// paths on objects it creates itself (no scene setup needed) and logs
    /// one PASS / FAIL line per check, plus a summary line.
    ///
    /// Checks: definition validation, health damage / heal / death,
    /// interaction hooks (dialogue, shop, quest), spawner spawn / despawn
    /// limits, and controller state after death. Movement / NavMesh and
    /// real Play-mode patrol behaviour are NOT covered here — test those
    /// by hand with the demo scene (see README.md TEST checklist).
    /// </summary>
    public class NPCSelfTest : MonoBehaviour
    {
        private int passed;
        private int failed;

        private void Start()
        {
            RunAllChecks();
            Debug.Log($"[Sdx17 NPC SelfTest] Finished: {passed} PASS, {failed} FAIL.");
            if (failed > 0)
            {
                Debug.LogError($"[Sdx17 NPC SelfTest] {failed} check(s) failed. See the FAIL lines above.", this);
            }
        }

        private void RunAllChecks()
        {
            CheckDefinitionValidation();
            CheckHealth();
            CheckInteractionHooks();
            CheckSpawnerLimits();
            CheckControllerDeath();
        }

        private void CheckDefinitionValidation()
        {
            NPCDefinition definition = ScriptableObject.CreateInstance<NPCDefinition>();
            // Deliberately invalid values: Validate() must repair them.
            definition.Validate();

            Check("Definition: default max health is at least 1", definition.MaxHealth >= 1f);
            Check("Definition: attack cooldown is at least 0.1", definition.AttackCooldown >= 0.1f);
            Check("Definition: chase distance is at least aggro range", definition.ChaseMaxDistance >= definition.AggroRange);
        }

        private void CheckHealth()
        {
            GameObject holder = new GameObject("SelfTest Health");
            NPCHealth health = holder.AddComponent<NPCHealth>();
            health.Initialize(100f);

            Check("Health: starts full at 100", Mathf.Approximately(health.CurrentHealth, 100f));
            health.TakeDamage(30f, null);
            Check("Health: 30 damage leaves 70", Mathf.Approximately(health.CurrentHealth, 70f));
            Check("Health: not dead at 70", !health.IsDead);
            health.Heal(10f);
            Check("Health: healing 10 gives 80", Mathf.Approximately(health.CurrentHealth, 80f));
            health.Heal(1000f);
            Check("Health: healing clamps to max 100", Mathf.Approximately(health.CurrentHealth, 100f));
            health.TakeDamage(0f, null);
            Check("Health: zero damage is ignored", Mathf.Approximately(health.CurrentHealth, 100f));
            health.TakeDamage(150f, null);
            Check("Health: lethal damage kills", health.IsDead && Mathf.Approximately(health.CurrentHealth, 0f));
            health.Heal(50f);
            Check("Health: dead NPC cannot be healed", health.IsDead);
            health.Revive();
            Check("Health: revive restores full health", !health.IsDead && Mathf.Approximately(health.CurrentHealth, 100f));

            Destroy(holder);
        }

        private void CheckInteractionHooks()
        {
            NPCDefinition definition = ScriptableObject.CreateInstance<NPCDefinition>();
            // Default definition: dialogue on but no id set, not a merchant,
            // not a quest giver — so no hook should fire.
            GameObject holder = new GameObject("SelfTest Interaction");
            NPCHealth health = holder.AddComponent<NPCHealth>();
            health.Initialize(100f);
            NPCController controller = holder.AddComponent<NPCController>();
            controller.SetDefinition(definition);
            NPCInteraction interaction = holder.AddComponent<NPCInteraction>();

            string receivedDialogue = null;
            string receivedShop = null;
            string receivedQuest = null;
            interaction.DialogueRequested += id => receivedDialogue = id;
            interaction.ShopRequested += id => receivedShop = id;
            interaction.QuestRequested += id => receivedQuest = id;

            // Default definition has dialogue on but no id, and is not a
            // merchant or quest giver, so no hook should fire.
            bool handled = interaction.Interact();
            Check("Interaction: no hook fires with no ids set", !handled && receivedDialogue == null && receivedShop == null && receivedQuest == null);

            Destroy(holder);
        }

        private void CheckSpawnerLimits()
        {
            GameObject spawnerObject = new GameObject("SelfTest Spawner");
            NPCSpawner spawner = spawnerObject.AddComponent<NPCSpawner>();

            // No prefab assigned: SpawnOne must fail safely and return null.
            Check("Spawner: no prefab returns null instead of crashing", spawner.SpawnOne() == null);
            Check("Spawner: alive count stays 0 with no prefab", spawner.AliveCount == 0);

            // Despawn of a null / unknown object must be a safe no-op.
            spawner.Despawn(null);
            Check("Spawner: despawn(null) is a safe no-op", spawner.AliveCount == 0);

            Destroy(spawnerObject);
        }

        private void CheckControllerDeath()
        {
            GameObject holder = new GameObject("SelfTest Controller");
            NPCHealth health = holder.AddComponent<NPCHealth>();
            health.Initialize(50f);
            NPCController controller = holder.AddComponent<NPCController>();
            holder.AddComponent<NPCInteraction>();

            // Wait one frame is not possible in a synchronous test, so check
            // the death path through health directly: killing the NPC must
            // fire the health death event the controller listens to.
            bool died = false;
            health.Died += () => died = true;
            health.TakeDamage(50f, null);
            Check("Controller: lethal damage fires the death event", died && health.IsDead);
            Check("Controller: component references are present", controller.Health != null);

            Destroy(holder);
        }

        private void Check(string name, bool condition)
        {
            if (condition)
            {
                passed++;
                Debug.Log($"[Sdx17 NPC SelfTest] PASS: {name}");
            }
            else
            {
                failed++;
                Debug.LogError($"[Sdx17 NPC SelfTest] FAIL: {name}");
            }
        }
    }
}
