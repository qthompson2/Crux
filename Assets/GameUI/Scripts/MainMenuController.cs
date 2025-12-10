using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
	private static WaitForSeconds _waitForSeconds0_1 = new(0.05f);
	[SerializeField] private Button startButton;
    [SerializeField] private Button helpButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject helpScreen;
    [SerializeField] private Image fadeOutPanel;
    [SerializeField] private AudioClip tapePlay;
    [SerializeField] private AudioSource source;
	[SerializeField] private AudioSource buttonPress;

	void Start()
	{
        ResetFadeOut();
		startButton.onClick.AddListener(OnStartButtonPressed);
		helpButton.onClick.AddListener(OnHelpButtonPressed);
		quitButton.onClick.AddListener(OnQuitButtonPressed);
		helpScreen.GetComponent<HelpScreenController>().SetReturnButtonOnPress(() => {buttonPress.Play(); helpScreen.SetActive(false);});
        source.Stop();
        source.loop = false;
        source.playOnAwake = false;
		AudioListener.pause = false;
	}

	public void OnStartButtonPressed()
	{
		StartCoroutine(FadeOut());
	}

    public void OnHelpButtonPressed()
	{
		buttonPress.Play();
		helpScreen.SetActive(true);
	}
    
    public void OnQuitButtonPressed()
	{
		buttonPress.Play();
		Application.Quit();
	}

    private IEnumerator FadeOut()
	{
        fadeOutPanel.gameObject.SetActive(true);
        source.resource = tapePlay;
        source.volume = 1;
        source.Play();
        while (fadeOutPanel.color.a < 1)
        {
			fadeOutPanel.color = new(fadeOutPanel.color.r, fadeOutPanel.color.g, fadeOutPanel.color.b, fadeOutPanel.color.a + 0.03f);
            yield return _waitForSeconds0_1;
        }
        while (source.isPlaying)
		{
			yield return _waitForSeconds0_1;
		}
        SceneManager.LoadScene("GameScene");
	}

    private void ResetFadeOut()
	{
        fadeOutPanel.gameObject.SetActive(false);
		fadeOutPanel.color = new(fadeOutPanel.color.r, fadeOutPanel.color.g, fadeOutPanel.color.b, 0);
	}
}
