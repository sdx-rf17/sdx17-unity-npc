using UnityEngine;

namespace Sdx17.NPC
{
    /// <summary>
    /// Data asset for one kind of NPC. Designers create one asset per NPC type
    /// (for example "Village Guard" or "Wandering Merchant") and assign it to
    /// an <see cref="NPCController"/>. Keeping data in a ScriptableObject means
    /// many NPCs can share one definition without duplicating values, and
    /// balance changes do not require touching any scene or prefab.
    /// Create via: right-click in the Project window &gt; Create &gt; Sdx17 &gt; NPC Definition.
    /// </summary>
    [CreateAssetMenu(fileName = "NewNPCDefinition", menuName = "Sdx17/NPC Definition")]
    public class NPCDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string displayName = "NPC";
        [SerializeField, TextArea(2, 4)] private string description = "";

        [Header("Health")]
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        // Fraction of max health (0-1) below which a non-aggressive NPC flees.
        // 0 means the NPC never flees on low health.
        [SerializeField, Range(0f, 1f)] private float fleeHealthFraction = 0.25f;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float moveSpeed = 3.5f;
        [SerializeField, Min(0f)] private float patrolWaitTime = 2f;

        [Header("Combat / Aggro")]
        [SerializeField] private bool isAggressive;
        [SerializeField, Min(0f)] private float aggroRange = 10f;
        [SerializeField, Min(0f)] private float attackRange = 2f;
        [SerializeField, Min(0f)] private float attackDamage = 10f;
        [SerializeField, Min(0.1f)] private float attackCooldown = 1f;
        // How far the NPC chases before giving up and returning to patrol/idle.
        [SerializeField, Min(0f)] private float chaseMaxDistance = 25f;

        [Header("Roles")]
        [SerializeField] private bool isMerchant;
        [SerializeField] private bool isQuestGiver;
        [SerializeField] private bool hasDialogue = true;

        [Header("Hook Ids (string ids, no hard dependencies)")]
        [Tooltip("Dialogue id handed to the Dialogue system, if one is installed. Empty means no dialogue line set.")]
        [SerializeField] private string dialogueId = "";
        [Tooltip("Shop id handed to the Inventory / shop system, if one is installed.")]
        [SerializeField] private string shopId = "";
        [Tooltip("Quest id handed to the Quest system, if one is installed.")]
        [SerializeField] private string questId = "";

        public string DisplayName => displayName;
        public string Description => description;
        public float MaxHealth => maxHealth;
        public float FleeHealthFraction => fleeHealthFraction;
        public float MoveSpeed => moveSpeed;
        public float PatrolWaitTime => patrolWaitTime;
        public bool IsAggressive => isAggressive;
        public float AggroRange => aggroRange;
        public float AttackRange => attackRange;
        public float AttackDamage => attackDamage;
        public float AttackCooldown => attackCooldown;
        public float ChaseMaxDistance => chaseMaxDistance;
        public bool IsMerchant => isMerchant;
        public bool IsQuestGiver => isQuestGiver;
        public bool HasDialogue => hasDialogue;
        public string DialogueId => dialogueId;
        public string ShopId => shopId;
        public string QuestId => questId;

        /// <summary>
        /// Clamps values that would break the state machine, and fixes
        /// inconsistent combinations (for example an attack range larger
        /// than the aggro range, which could never be reached).
        /// Called by the editor and whenever the definition is applied.
        /// </summary>
        public void Validate()
        {
            if (maxHealth < 1f) maxHealth = 1f;
            if (moveSpeed < 0f) moveSpeed = 0f;
            if (patrolWaitTime < 0f) patrolWaitTime = 0f;
            if (aggroRange < 0f) aggroRange = 0f;
            if (attackRange < 0f) attackRange = 0f;
            if (attackDamage < 0f) attackDamage = 0f;
            if (attackCooldown < 0.1f) attackCooldown = 0.1f;
            if (chaseMaxDistance < aggroRange) chaseMaxDistance = aggroRange;
            if (string.IsNullOrWhiteSpace(displayName)) displayName = "NPC";
            if (dialogueId == null) dialogueId = "";
            if (shopId == null) shopId = "";
            if (questId == null) questId = "";
        }

        private void OnValidate()
        {
            Validate();
        }
    }
}
