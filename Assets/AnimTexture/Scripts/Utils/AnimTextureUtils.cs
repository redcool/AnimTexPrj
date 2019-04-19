namespace AnimTexture
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class AnimTextureUtils
    {

        public static Texture2D BakeMeshToTexture(SkinnedMeshRenderer skin, GameObject clipGo, AnimationClip clip)
        {
            var width = skin.sharedMesh.vertexCount;
            var frameCount = (int)(clip.length * clip.frameRate);
            var timePerFrame = clip.length / frameCount;
            var tex = new Texture2D(width, frameCount, TextureFormat.RGBAHalf, false, false);
            tex.name = clip.name;

            float time = 0;
            Mesh mesh = new Mesh();
            for (int y = 0; y < frameCount; y++)
            {
                clip.SampleAnimation(clipGo, time += timePerFrame);
                skin.BakeMesh(mesh);

                var colors = new Color[mesh.vertexCount];

                for (int x = 0; x < mesh.vertexCount; x++)
                {
                    var v = mesh.vertices[x];
                    colors[x] = new Vector4(v.x, v.y, v.z);
                }
                tex.SetPixels(0, y, width, 1, colors);
            }
            tex.Apply();
            return tex;
        }
    }
}