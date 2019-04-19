namespace AnimTexture
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Fps : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 100, 0, 100, 40));
            GUILayout.Label(string.Format("FPS:{0:f2}", 1.0f / Time.smoothDeltaTime));
            GUILayout.EndArea();
        }
    }
}