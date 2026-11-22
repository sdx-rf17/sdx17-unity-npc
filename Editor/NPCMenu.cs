using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Sdx17.NPC.Editor
{
    /// <summary>
    /// Menu helpers under GameObject &gt; Sdx17 &gt; NPC. They create the
    /// GameObjects and assets a new NPC needs, with the required components
    /// already attached, so setup is one menu click instead of manual wiring.
    /// </summary>
    public static class NPCMenu
    {
        [MenuItem("GameObject/Sdx17/NPC/Create NPC Definition", false, 10)]
        public static void CreateNPCDefinition()
        {
            NPCDefinition definition = ScriptableObject.CreateInstance<NPCDefinition>();
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/NewNPCDefinition.asset");
            AssetDatabase.CreateAsset(definition, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = definition;
            Debug.Log($"[Sdx17 NPC] Created NPC Definition at {path}. Edit its values, then assign it to an NPCController.");
        }

        [MenuItem("GameObject/Sdx17/NPC/Create NPC (with Controller + Health)", false, 11)]
        public static void CreateNPCGameObject()
        {
            GameObject npc = new GameObject("NPC");
            npc.AddComponent<NPCHealth>();
            npc.AddComponent<NPCController>();
            npc.AddComponent<NPCInteraction>();
            NavMeshAgent agent = npc.AddComponent<NavMeshAgent>();
            agent.speed = 3.5f;
            agent.stoppingDistance = 0.5f;

            // A capsule gives the NPC a visible body and a collider in the scene.
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(npc.transform, false);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);

            Selection.activeGameObject = npc;
            Undo.RegisterCreatedObjectUndo(npc, "Create Sdx17 NPC");
            Debug.Log("[Sdx17 NPC] Created NPC GameObject. Assign an NPCDefinition to its NPCController, add patrol waypoints if wanted, and bake a NavMesh (see Samples~/Demo/README.md).");
        }

        [MenuItem("GameObject/Sdx17/NPC/Create NPC Spawner", false, 12)]
        public static void CreateNPCSpawner()
        {
            GameObject spawnerObject = new GameObject("NPC Spawner");
            spawnerObject.AddComponent<NPCSpawner>();
            Selection.activeGameObject = spawnerObject;
            Undo.RegisterCreatedObjectUndo(spawnerObject, "Create Sdx17 NPC Spawner");
            Debug.Log("[Sdx17 NPC] Created NPC Spawner. Assign an NPC Prefab and at least one Spawn Point in its Inspector.");
        }
    }
}
