namespace AnimTexture
{
    using AnimTexture;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// AnimatorController controlTextureAnimation .
    /// controller' StateName need equals AnimationClip's Name.
    /// </summary>
    [RequireComponent(typeof(Animator), typeof(TextureAnimation))]
    public class MecanimController : MonoBehaviour
    {
        [HideInInspector] public Animator animator;
        [HideInInspector] public TextureAnimation texAnim;
        public int layerIndex = 0;

        int lastTransitionNameHash;
        // Start is called before the first frame update
        void Start()
        {
            animator = GetComponent<Animator>();
            texAnim = GetComponent<TextureAnimation>();

            var stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
            var curIndex = texAnim.GetClipIndex(stateInfo.shortNameHash);
            texAnim.Play(curIndex);

            //BaseComponentSystem.GetInstance<MecanimControllerSystem>().Add(this);
        }

        // Update is called once per frame
        public void Update()
        {
            if (!animator)
                return;

            var trans = animator.GetAnimatorTransitionInfo(layerIndex);
            if (trans.nameHash == 0)
                return;

            var stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
            var curIndex = texAnim.GetClipIndex(stateInfo.shortNameHash);

            //Debug.Log("trans:" + trans.nameHash);
            //var nsi = animator.GetNextAnimatorStateInfo(layerIndex);
            //Debug.Log("state:" + nsi.shortNameHash);

            if (trans.nameHash != lastTransitionNameHash)
            {
                lastTransitionNameHash = trans.nameHash;

                var nextStateInfo = animator.GetNextAnimatorStateInfo(layerIndex);
                var nextIndex = texAnim.GetClipIndex(nextStateInfo.shortNameHash);
                texAnim.CrossFade(curIndex, nextIndex, trans.duration);
                //texAnim.Play(nextIndex);
            }

        }
    }

}