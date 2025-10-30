using UnityEngine;
using UnityEditor;

public class QuitGame : MonoBehaviour
{
    public void QuitApplication()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
