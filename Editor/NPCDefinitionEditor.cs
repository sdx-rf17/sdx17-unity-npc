using UnityEditor;
using UnityEngine;

namespace Sdx17.NPC.Editor
{
    /// <summary>
    /// Custom inspector for <see cref="NPCDefinition"/>: draws the fields in
    /// grouped sections and shows validation warnings for combinations that
    /// would not work at runtime (for example an attack range larger than
    /// the aggro range, or a merchant with no shop id).
    /// </summary>
    [CustomEditor(typeof(NPCDefinition))]
    public class NPCDefinitionEditor : UnityEditor.Editor
    {
        private SerializedProperty displayName;
        private SerializedProperty description;
        private SerializedProperty maxHealth;
        private SerializedProperty fleeHealthFraction;
        private SerializedProperty moveSpeed;
        private SerializedProperty patrolWaitTime;
        private SerializedProperty isAggressive;
        private SerializedProperty aggroRange;
        private SerializedProperty attackRange;
        private SerializedProperty attackDamage;
        private SerializedProperty attackCooldown;
        private SerializedProperty chaseMaxDistance;
        private SerializedProperty isMerchant;
        private SerializedProperty isQuestGiver;
        private SerializedProperty hasDialogue;
        private SerializedProperty dialogueId;
        private SerializedProperty shopId;
        private SerializedProperty questId;

        private void OnEnable()
        {
            displayName = serializedObject.FindProperty("displayName");
            description = serializedObject.FindProperty("description");
            maxHealth = serializedObject.FindProperty("maxHealth");
            fleeHealthFraction = serializedObject.FindProperty("fleeHealthFraction");
            moveSpeed = serializedObject.FindProperty("moveSpeed");
            patrolWaitTime = serializedObject.FindProperty("patrolWaitTime");
            isAggressive = serializedObject.FindProperty("isAggressive");
            aggroRange = serializedObject.FindProperty("aggroRange");
            attackRange = serializedObject.FindProperty("attackRange");
            attackDamage = serializedObject.FindProperty("attackDamage");
            attackCooldown = serializedObject.FindProperty("attackCooldown");
            chaseMaxDistance = serializedObject.FindProperty("chaseMaxDistance");
            isMerchant = serializedObject.FindProperty("isMerchant");
            isQuestGiver = serializedObject.FindProperty("isQuestGiver");
            hasDialogue = serializedObject.FindProperty("hasDialogue");
            dialogueId = serializedObject.FindProperty("dialogueId");
            shopId = serializedObject.FindProperty("shopId");
            questId = serializedObject.FindProperty("questId");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Identity", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(displayName);
            EditorGUILayout.PropertyField(description);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Health", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(maxHealth);
            EditorGUILayout.PropertyField(fleeHealthFraction);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Movement", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(moveSpeed);
            EditorGUILayout.PropertyField(patrolWaitTime);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Combat / Aggro", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(isAggressive);
            EditorGUILayout.PropertyField(aggroRange);
            EditorGUILayout.PropertyField(attackRange);
            EditorGUILayout.PropertyField(attackDamage);
            EditorGUILayout.PropertyField(attackCooldown);
            EditorGUILayout.PropertyField(chaseMaxDistance);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Roles", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(isMerchant);
            EditorGUILayout.PropertyField(isQuestGiver);
            EditorGUILayout.PropertyField(hasDialogue);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Hook Ids", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(dialogueId);
            EditorGUILayout.PropertyField(shopId);
            EditorGUILayout.PropertyField(questId);

            EditorGUILayout.Space();
            DrawWarnings();

            serializedObject.ApplyModifiedProperties();

            // Apply the same clamping the runtime uses, so the asset is
            // always in a valid state after editing.
            if (target is NPCDefinition definition)
            {
                definition.Validate();
            }
        }

        private void DrawWarnings()
        {
            if (attackRange.floatValue > aggroRange.floatValue)
            {
                EditorGUILayout.HelpBox(
                    "Attack Range is larger than Aggro Range. The NPC can never get close enough to attack — raise Aggro Range or lower Attack Range.",
                    MessageType.Warning);
            }

            if (chaseMaxDistance.floatValue < aggroRange.floatValue)
            {
                EditorGUILayout.HelpBox(
                    "Chase Max Distance is smaller than Aggro Range. The NPC will give up the chase immediately — raise Chase Max Distance.",
                    MessageType.Warning);
            }

            if (isMerchant.boolValue && string.IsNullOrEmpty(shopId.stringValue))
            {
                EditorGUILayout.HelpBox(
                    "This NPC is a merchant but has no Shop Id. Set one so the Inventory / shop system knows which shop to open.",
                    MessageType.Warning);
            }

            if (isQuestGiver.boolValue && string.IsNullOrEmpty(questId.stringValue))
            {
                EditorGUILayout.HelpBox(
                    "This NPC is a quest giver but has no Quest Id. Set one so the Quest system knows which quest to offer.",
                    MessageType.Warning);
            }

            if (hasDialogue.boolValue && string.IsNullOrEmpty(dialogueId.stringValue))
            {
                EditorGUILayout.HelpBox(
                    "Dialogue is enabled but no Dialogue Id is set. The NPC will log a warning when interacted with.",
                    MessageType.Info);
            }

            if (!isAggressive.boolValue && fleeHealthFraction.floatValue <= 0f)
            {
                EditorGUILayout.HelpBox(
                    "This NPC is not aggressive and never flees (Flee Health Fraction is 0). It will stand still while being attacked.",
                    MessageType.Info);
            }
        }
    }
}
