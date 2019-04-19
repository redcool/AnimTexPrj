namespace AnimTexture
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class TextureAnimation : MonoBehaviour
    {
        readonly int ID_ANIM_TEX = Shader.PropertyToID("_AnimTex");
        readonly int ID_LOOP = Shader.PropertyToID("_Loop");
        readonly int ID_CROSS_LERP = Shader.PropertyToID("_CrossLerp");
        readonly int ID_START_FRAME = Shader.PropertyToID("_StartFrame");
        readonly int ID_END_FRAME = Shader.PropertyToID("_EndFrame");
        readonly int ID_NEXT_START_FRAME = Shader.PropertyToID("_NextStartFrame");
        readonly int ID_NEXT_END_FRAME = Shader.PropertyToID("_NextEndFrame");
        readonly int ID_PLAY_TIME = Shader.PropertyToID("_PlayTime");
        readonly int ID_OFFSET_PLAY_TIME = Shader.PropertyToID("_OffsetPlayTime");

        public AnimTextureManifest manifest;

        public int curIndex;
        public int nextIndex;
        public float crossFadeTime = 1;

        public bool crossTest;
        public float playTime;
        public float offsetPlayTime;

        Renderer r;
        MaterialPropertyBlock block;
        Coroutine crossLerpCoroutine;
        bool needUpdateBlock;

        Dictionary<int, int> clipNameHashDict = new Dictionary<int, int>();
        public AnimTextureClipInfo curClipInfo;

        // Start is called before the first frame update
        void Awake()
        {
            r = GetComponent<Renderer>();
            r.sharedMaterial.SetTexture(ID_ANIM_TEX, manifest.atlas);

            block = new MaterialPropertyBlock();

            SetupDict();

            Play(curIndex);
        }

        // Update is called once per frame
        void Update()
        {
            playTime += Time.deltaTime;
            UpdatePlayTime();
            UpdateAnimLoop();

            if (crossTest)
            {
                crossTest = false;

                //Play(animIndex);
                CrossFade(curIndex, nextIndex, crossFadeTime);
            }

            if (needUpdateBlock)
            {
                needUpdateBlock = false;
                r.SetPropertyBlock(block);
            }
        }

        void SetupDict()
        {
            for (int i = 0; i < manifest.animInfos.Count; i++)
            {
                clipNameHashDict[manifest.animInfos[i].clipNameHash] = i;
            }
        }

        public int GetClipIndex(int stateNameHash)
        {
            return clipNameHashDict[stateNameHash];
        }

        void UpdateAnimTime(int index, int startNameHash,int endNameHash)
        {
            curClipInfo = manifest.animInfos[index];

            block.SetFloat(startNameHash, curClipInfo.startFrame);
            block.SetFloat(endNameHash, curClipInfo.endFrame);
            needUpdateBlock = true;
        }
        void UpdatePlayTime()
        {
            block.SetFloat(ID_PLAY_TIME, playTime);
            block.SetFloat(ID_OFFSET_PLAY_TIME, offsetPlayTime);
            needUpdateBlock = true;
        }

        void UpdateAnimLoop()
        {
            if (!curClipInfo.isLoop)
            {
                if (playTime > curClipInfo.length)
                {
                    //var loopLerp = block.GetFloat(ID_LOOP);
                    //Debug.Log(playTime + ":" + curClipInfo.length);
                    block.SetFloat(ID_LOOP, 1);
                }
            }
            else
                block.SetFloat(ID_LOOP, 0);
            needUpdateBlock = true;
        }

        void UpdateCrossLerp(float lerp)
        {
            block.SetFloat(ID_CROSS_LERP, lerp);
            needUpdateBlock = true;
        }

        public void Play(int index)
        {
            playTime = 0;
            offsetPlayTime = 0;

            UpdateAnimTime(index, ID_START_FRAME, ID_END_FRAME);
            UpdateAnimTime(index, ID_NEXT_START_FRAME, ID_NEXT_END_FRAME);
            UpdateCrossLerp(1);
        }

        public void CrossFade(int index,int nextIndex,float fadeTime)
        {
            playTime = 0;

            var animInfo = manifest.animInfos[index];
            offsetPlayTime = animInfo.length - fadeTime;

            UpdateAnimTime(index, ID_START_FRAME, ID_END_FRAME);
            UpdateAnimTime(nextIndex, ID_NEXT_START_FRAME, ID_NEXT_END_FRAME);

            if(crossLerpCoroutine != null)
                StopCoroutine(crossLerpCoroutine);

            crossLerpCoroutine = StartCoroutine(WaitForUpdateCrossLerp(nextIndex,fadeTime));
        }

        IEnumerator WaitForUpdateCrossLerp(int index,float fadeTime)
        {
            if (fadeTime <= 0)
            {
                UpdateCrossLerp(1);
                yield break;
            }

            var speed = 1.0f / fadeTime;
            var crossLerp = 0f;

            while (crossLerp < 1)
            {
                UpdateCrossLerp(crossLerp);
                crossLerp += speed * Time.deltaTime;
                yield return 0;
            }
            UpdateCrossLerp(1);
            StopCoroutine(crossLerpCoroutine);
        }
    }
}