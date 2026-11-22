using System;
using UnityEngine;
using UnityEngine.Events;

namespace Sdx17.NPC
{
    /// <summary>
    /// Interaction hooks for an NPC: dialogue, shop, and quest-giver.
    ///
    /// This class deliberately does NOT reference the Sdx17 Dialogue,
    /// Inventory, or Quest packages. Instead it fires UnityEvents and
    /// C# events carrying the string ids from the NPCDefinition
    /// (dialogueId / shopId / questId). A game wires those events to its
    /// own systems in the Inspector or in code — for example:
    ///   npcInteraction.OnDialogueRequested.AddListener(id => dialogueSystem.Open(id));
    /// With nothing wired up, Interact() still works: it logs which hook
    /// was requested so the missing wiring is visible instead of silent.
    ///
    /// Priority when several roles are set: dialogue first, then shop,
    /// then quest. Call the Try* methods directly to trigger one hook only.
    /// </summary>
    [RequireComponent(typeof(NPCController))]
    [DisallowMultipleComponent]
    public class NPCInteraction : MonoBehaviour
    {
        [Header("Events — wire these to your Dialogue / Shop / Quest systems")]
        [SerializeField] private UnityEvent<string> onDialogueRequested;
        [SerializeField] private UnityEvent<string> onShopRequested;
        [SerializeField] private UnityEvent<string> onQuestRequested;
        [SerializeField] private UnityEvent onInteracted;

        private NPCController controller;

        public UnityEvent<string> OnDialogueRequested => onDialogueRequested;
        public UnityEvent<string> OnShopRequested => onShopRequested;
        public UnityEvent<string> OnQuestRequested => onQuestRequested;
        public UnityEvent OnInteracted => onInteracted;

        /// <summary>Fired with the dialogue id when dialogue is requested.</summary>
        public event Action<string> DialogueRequested;
        /// <summary>Fired with the shop id when the shop is requested.</summary>
        public event Action<string> ShopRequested;
        /// <summary>Fired with the quest id when a quest is requested.</summary>
        public event Action<string> QuestRequested;

        private void Awake()
        {
            controller = GetComponent<NPCController>();
            if (controller == null)
            {
                Debug.LogError($"[Sdx17 NPC] {name} NPCInteraction needs an NPCController on the same GameObject.", this);
            }
        }

        /// <summary>
        /// The single entry point for player interaction (E key, raycast click,
        /// proximity prompt — whatever the game uses). Fires the highest
        /// priority available hook. Returns true if any hook fired.
        /// Dead NPCs cannot be interacted with.
        /// </summary>
        public bool Interact()
        {
            if (controller == null) return false;
            if (controller.State == NPCState.Dead) return false;

            onInteracted?.Invoke();

            // Dialogue first: a talking NPC greets the player before
            // offering a shop or a quest.
            if (TryOpenDialogue()) return true;
            if (TryOpenShop()) return true;
            if (TryOfferQuest()) return true;

            Debug.Log($"[Sdx17 NPC] {DisplayName()} has no dialogue, shop, or quest configured.", this);
            return false;
        }

        /// <summary>Requests dialogue if the definition has dialogue enabled and an id set.</summary>
        public bool TryOpenDialogue()
        {
            NPCDefinition definition = controller?.Definition;
            if (definition == null || !definition.HasDialogue) return false;
            if (string.IsNullOrEmpty(definition.DialogueId))
            {
                Debug.Log($"[Sdx17 NPC] {DisplayName()} has dialogue enabled but no Dialogue Id set.", this);
                return false;
            }

            onDialogueRequested?.Invoke(definition.DialogueId);
            DialogueRequested?.Invoke(definition.DialogueId);
            return true;
        }

        /// <summary>Requests the shop if the NPC is a merchant and a shop id is set.</summary>
        public bool TryOpenShop()
        {
            NPCDefinition definition = controller?.Definition;
            if (definition == null || !definition.IsMerchant) return false;
            if (string.IsNullOrEmpty(definition.ShopId))
            {
                Debug.Log($"[Sdx17 NPC] {DisplayName()} is a merchant but has no Shop Id set.", this);
                return false;
            }

            onShopRequested?.Invoke(definition.ShopId);
            ShopRequested?.Invoke(definition.ShopId);
            return true;
        }

        /// <summary>Requests a quest if the NPC is a quest giver and a quest id is set.</summary>
        public bool TryOfferQuest()
        {
            NPCDefinition definition = controller?.Definition;
            if (definition == null || !definition.IsQuestGiver) return false;
            if (string.IsNullOrEmpty(definition.QuestId))
            {
                Debug.Log($"[Sdx17 NPC] {DisplayName()} is a quest giver but has no Quest Id set.", this);
                return false;
            }

            onQuestRequested?.Invoke(definition.QuestId);
            QuestRequested?.Invoke(definition.QuestId);
            return true;
        }

        private string DisplayName()
        {
            NPCDefinition definition = controller?.Definition;
            return definition != null && !string.IsNullOrEmpty(definition.DisplayName)
                ? definition.DisplayName
                : name;
        }
    }
}
