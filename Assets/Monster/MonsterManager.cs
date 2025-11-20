using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    [SerializeField] private float spiderSpawnZ = 500f;
    [SerializeField] private float ReleaseTime = 3f * 60f;
    private Transform player;
    private List<GameObject> monsters;
    private bool isActive = false;
    private bool hasActivated = false;
    private bool isPaused = false;
    [SerializeField] private float releaseTimer = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("Player").transform;
        monsters = new();
        for (int index = 0; index < transform.childCount; index++)
        {
            GameObject child = transform.GetChild(index).gameObject;
            monsters.Add(child);
            child.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasActivated)
		{
            if (!isPaused)
			{
				releaseTimer += Time.deltaTime;
			}

			if (player.position.z < spiderSpawnZ || releaseTimer > ReleaseTime)
            {
                SetMonstersActive(true);
            }
		}
    }

    private void SetMonstersActive(bool isActive)
	{
        if (this.isActive != isActive)
        {
            this.isActive = isActive;
            foreach (GameObject monster in monsters)
            {
                monster.SetActive(isActive);
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
				SetMonstersActive(false);
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
				SetMonstersActive(true);
			}
		}
	}
}
