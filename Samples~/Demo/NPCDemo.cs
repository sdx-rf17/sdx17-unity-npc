using UnityEngine;

namespace Sdx17.NPC.Samples
{
    /// <summary>
    /// Minimal demo driver (see Samples~/Demo/README.md for the scene setup).
    /// On Start it finds the demo NPC and the player, logs what it found,
    /// and wires the interaction events to plain log lines so the hooks can
    /// be seen working without any other Sdx17 system installed.
    /// Press I near the NPC to interact, and K to deal 25 damage to it
    /// (useful to watch the Flee / Dead states without a combat system).
    /// </summary>
    public class NPCDemo : MonoBehaviour
    {
        [SerializeField] private NPCController demoNpc;
        [SerializeField] private Transform player;
        [SerializeField, Min(0f)] private float interactDistance = 3f;
        [SerializeField, Min(0f)] private float demoDamage = 25f;

        private NPCInteraction interaction;

        private void Start()
        {
            if (demoNpc == null)
            {
                demoNpc = FindObjectOfType<NPCController>();
            }
            if (player == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null) player = playerObject.transform;
            }

            if (demoNpc == null)
            {
                Debug.LogError("[Sdx17 NPC Demo] No NPCController found in the scene. Follow Samples~/Demo/README.md to add one.", this);
                return;
            }

            interaction = demoNpc.GetComponent<NPCInteraction>();
            if (interaction != null)
            {
                interaction.DialogueRequested += id => Debug.Log($"[Sdx17 NPC Demo] Dialogue requested: {id}");
                interaction.ShopRequested += id => Debug.Log($"[Sdx17 NPC Demo] Shop requested: {id}");
                interaction.QuestRequested += id => Debug.Log($"[Sdx17 NPC Demo] Quest requested: {id}");
            }

            demoNpc.StateChanged += state => Debug.Log($"[Sdx17 NPC Demo] NPC state: {state}");
            Debug.Log($"[Sdx17 NPC Demo] Ready. NPC: {demoNpc.name}, state: {demoNpc.State}. Walk close and press I to interact, K to deal {demoDamage} damage.");
        }

        private void Update()
        {
            if (demoNpc == null || player == null) return;
            if (Vector3.Distance(player.position, demoNpc.transform.position) > interactDistance) return;

            if (Input.GetKeyDown(KeyCode.I))
            {
                bool handled = interaction != null && interaction.Interact();
                Debug.Log($"[Sdx17 NPC Demo] Interact pressed. Hook fired: {handled}");
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                NPCHealth health = demoNpc.GetComponent<NPCHealth>();
                if (health != null)
                {
                    health.TakeDamage(demoDamage, player.gameObject);
                    Debug.Log($"[Sdx17 NPC Demo] Dealt {demoDamage} damage. Health now {health.CurrentHealth}/{health.MaxHealth}.");
                }
            }
        }
    }
}
