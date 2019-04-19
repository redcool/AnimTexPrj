#if UNITY_EDITOR
namespace AnimTexture
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;
    using System.IO;
    using System.Text;

    public class AnimTextureEditor
    {
        //if you change AnimTexture path, need change this path.
        public const string ANIM_TEXTURE_PATH = "AnimTexture";
        public const string DEFAULT_TEX_DIR = ANIM_TEXTURE_PATH+"/AnimTexPath";

        [MenuItem("AnimTexture/BakeAnimClipsToAtlas")]
        static void BakeAllInOne()
        {
            var objs = Selection.GetFiltered<GameObject>(SelectionMode.DeepAssets);
            if(objs.Length == 0)
            {
                Debug.Log("Select Model with Animations.");
                return;
            }

            foreach (var go in objs)
            {
                var newInst = Object.Instantiate(go);
                newInst.name = go.name;

                int clipCount = BakeAllClips(newInst);
                Object.DestroyImmediate(newInst);
                ShowResult(go, clipCount);
            }
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>($"Assets/{DEFAULT_TEX_DIR}"));
        }

        static void ShowResult(GameObject go,int clipCount)
        {
            if (clipCount == 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine("AnimationClips cannt found! Checks:");
                sb.AppendLine("1 (Mesh or AnimationMesh)'s Animation Type must be Legacy!");
                sb.AppendLine("2 Animation Component's Animations maybe has nothing!");
                Debug.Log(sb);
            }
            else
                Debug.Log($"{go} ,clips:{clipCount}");
        }

        /**
        [MenuItem("AnimTexture/BakeAnimToTexture")]
        static void BakeOneByOne()
        {
            var objs = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);

            var q = from obj in objs
                    let go = obj as GameObject
                    where go
                    select go;

            foreach (var item in q)
            {
                var newInst = Object.Instantiate(item);
                newInst.name = item.name;

                Bake(newInst);
                Object.DestroyImmediate(newInst);
            }
            Debug.Log("Bake done.");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath($"Assets/{DEFAULT_TEX_DIR}", typeof(Object));
        }
        */

        public static void Bake(GameObject go)
        {
            var skin = go.GetComponentInChildren<SkinnedMeshRenderer>();
            var anim = go.GetComponentInChildren<Animation>();
            var dir = $"{Application.dataPath}/{DEFAULT_TEX_DIR}/";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            foreach (AnimationState state in anim)
            {
                var tex = AnimTextureUtils.BakeMeshToTexture(skin, go, state.clip);
                AssetDatabase.CreateAsset(tex, $"Assets/{DEFAULT_TEX_DIR}/{tex.name}.asset");
            }
            AssetDatabase.Refresh();
        }

        public static int BakeAllClips(GameObject go)
        {
            var skin = go.GetComponentInChildren<SkinnedMeshRenderer>();
            var anim = go.GetComponentInChildren<Animation>();
            var dir = $"{Application.dataPath}/{DEFAULT_TEX_DIR}/";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var manifest = ScriptableObject.CreateInstance<AnimTextureManifest>();
            var yList = GenerateAtlas(skin, anim, out manifest.atlas);
            var count = BakeClip(go, skin, anim, manifest, yList);
            manifest.atlas.Apply();

            //output atlas
            AssetDatabase.CreateAsset(manifest.atlas, $"Assets/{DEFAULT_TEX_DIR}/{go.name}_AnimTexture.asset");
            //File.WriteAllBytes(Application.dataPath + $"/{DEFAULT_TEX_DIR}/{go.name}_AnimTexture.png", manifest.atlas.EncodeToJPG());
            //output infos
            AssetDatabase.CreateAsset(manifest, $"Assets/{DEFAULT_TEX_DIR}/{go.name}_{typeof(AnimTextureManifest).Name}.asset");

            AssetDatabase.Refresh();
            return count;
        }

        private static int BakeClip(GameObject go, SkinnedMeshRenderer skin, Animation anim, AnimTextureManifest manifest, List<int> yList)
        {
            var index = 0;
            foreach (AnimationState state in anim)
            {
                //tex
                var y = yList[index];
                var tex = AnimTextureUtils.BakeMeshToTexture(skin, go, state.clip);
                manifest.atlas.SetPixels(0, y, tex.width, tex.height, tex.GetPixels());
                Object.DestroyImmediate(tex);

                manifest.animInfos.Add(new AnimTextureClipInfo(state.name, y, yList[index + 1])
                {
                    isLoop = state.clip.isLooping,
                    length = state.clip.length
                });
                index++;
            }

            return index;
        }

        static List<int> GenerateAtlas(SkinnedMeshRenderer skin, Animation anim, out Texture2D atlas)
        {
            var yList = new List<int>();
            var clipCount = anim.GetClipCount();
            var width = skin.sharedMesh.vertexCount;
            var y = 0;
            yList.Add(0);

            foreach (AnimationState state in anim)
            {
                y += (int)(state.length * state.clip.frameRate);
                yList.Add(y);
            }
            atlas = new Texture2D(width, y, TextureFormat.RGBAHalf, false);
            atlas.filterMode = FilterMode.Point;
            return yList;
        }

        public static string GetManifestPath(string goName)
        {
            string path = $"Assets/{DEFAULT_TEX_DIR}/{goName}_{typeof(AnimTextureManifest).Name}.asset";
            return path;
        }
    }
}
#endif