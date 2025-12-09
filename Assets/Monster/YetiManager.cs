using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YetiManager : MonoBehaviour
{
    [SerializeField] private float yetiSpawnY = 180f;
    private Transform player;
    private List<GameObject> yetis;
    private bool isActive = false;
    private bool hasActivated = false;
    private bool isPaused = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("Player").transform;
        yetis = new();
        for (int index = 0; index < transform.childCount; index++)
        {
            GameObject child = transform.GetChild(index).gameObject;
            yetis.Add(child);
            child.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasActivated && player.position.y > yetiSpawnY)
		{
            SetYetisActive(true);
            hasActivated = true;
		}
    }

    private void SetYetisActive(bool isActive)
	{
        if (this.isActive != isActive)
        {
            this.isActive = isActive;
            foreach (GameObject yeti in yetis)
            {
                yeti.SetActive(isActive);
            }
        }

        hasActivated = true;
	}

    public void Pause()
	{
		if (!isPaused)
		{
			isPaused = true;
            if (hasActivated)
			{
				SetYetisActive(false);
			}
		}
	}

    public void Resume()
	{
		if (isPaused)
		{
			isPaused = false;
            if (hasActivated)
			{
				SetYetisActive(true);
			}
		}
	}
}
