using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class QuitGame : MonoBehaviour
{
    public void QuitApplication()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
