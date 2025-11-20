using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    [SerializeField] private float spiderSpawnZ = 500f;
    [SerializeField] private float spawnTimer = 3f * 60f;
    private Transform player;
    private List<GameObject> monsters;
    private bool isActive = false;
    private bool hasActivated = false;
    private bool isPaused = false;
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
        StartCoroutine(SpawnMonstersOnTimer());
    }

    private IEnumerator SpawnMonstersOnTimer()
	{
        yield return new WaitForSeconds(spawnTimer);
        SetMonstersActive(true);
	}

    // Update is called once per frame
    void Update()
    {
        if (!hasActivated)
		{
			if (player.position.z < spiderSpawnZ)
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
			TogglePause();
		}
	}

    public void Resume()
	{
		if (isPaused)
		{
			TogglePause();
		}
	}

    public void TogglePause()
	{
		if (hasActivated)
		{
			if (!isPaused)
            {
                SetMonstersActive(false);
                isPaused = true;
            }
            else
            {
                SetMonstersActive(true);
                isPaused = false;
            }
		}
	}
}
