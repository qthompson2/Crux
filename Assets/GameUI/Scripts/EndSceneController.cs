using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class EndSceneController : MonoBehaviour
{
    private readonly string escapedString = "You Have Escaped";
    private readonly string thanksString = "Thank You For Playing!\n\n                :)";
    private float timer = 0;
    private readonly float showThanksStringAt = 8;
    private bool thanksStringShown = false;
    [SerializeField] private TMP_Text messageText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioListener.pause = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > showThanksStringAt && !thanksStringShown)
		{
			thanksStringShown = true;
            StartCoroutine(TypeOut(thanksString, 0.1f));
		} 
        else if (!thanksStringShown)
		{
			timer += Time.deltaTime;
		}
    }

    private IEnumerator TypeOut(string text, float delay)
	{
		while (messageText.text != "")
		{
			messageText.text = messageText.text[..^1];
            yield return new WaitForSeconds(delay);
		}
        foreach (char chara in text)
		{
			messageText.text += chara;
            yield return new WaitForSeconds(delay);
		}
	}
}
