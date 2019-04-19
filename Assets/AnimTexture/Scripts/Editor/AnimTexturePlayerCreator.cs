#if UNITY_EDITOR
namespace AnimTexture
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.AI;

    public class AnimTexturePlayerCreator 
    {
        [MenuItem("AnimTexture/CreatePlayer")]
        static void CreatePlayer()
        {
            var prefabs = Selection.GetFiltered<GameObject>(SelectionMode.Assets);
            foreach (var p in prefabs)
            {
                var parentGo = new GameObject(p.name);
                AddAgent(parentGo);

                var go = Object.Instantiate(p);

                go.name = p.name+"_Animator";
                go.transform.SetParent(parentGo.transform);
                SetupAnimTexture(go,p.name);

                SetupAnimator(go);
                SetupRenderer(go);
            }
            Debug.Log("Create player done.");
        }

        static void SetupAnimTexture(GameObject go,string goName)
        {
            Object.DestroyImmediate(go.GetComponentInChildren<Animation>());

            var manifest = AssetDatabase.LoadAssetAtPath<AnimTextureManifest>(AnimTextureEditor.GetManifestPath(goName));
            var anim = GetOrAdd<TextureAnimation>(go);
            anim.manifest = manifest;
        }

        private static void SetupRenderer(GameObject go)
        {
            var skin = go.GetComponentInChildren<SkinnedMeshRenderer>();
            var mf = GetOrAdd<MeshFilter>(go);
            mf.sharedMesh = skin.sharedMesh;
            Object.DestroyImmediate(skin);

            var mr = GetOrAdd<MeshRenderer>(go);
            mr.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>($"Assets/{AnimTextureEditor.ANIM_TEXTURE_PATH}/Shaders/AnimTexture.mat");
        }

        private static void SetupAnimator(GameObject go)
        {
            var animator = GetOrAdd<Animator>(go);
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>($"Assets/{AnimTextureEditor.ANIM_TEXTURE_PATH}/OtherRes/SimpleController.controller");

            GetOrAdd<MecanimController>(go);
        }

        static void AddAgent(GameObject go)
        {
            GetOrAdd<NavMeshAgent>(go);
            GetOrAdd<AgentPlayer>(go);
        }

        static T GetOrAdd<T>(GameObject go) where T : Component
        {
            if (!go)
                return default(T);
            var comp = go.GetComponent<T>();
            if (!comp)
                comp = go.AddComponent<T>();
            return comp;
        }
    }
}
#endif