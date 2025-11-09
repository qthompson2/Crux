using System.Collections;
using System.Diagnostics;
using UnityEngine;


public abstract class ItemClass : MonoBehaviour
{
    [Header("General Item Properties")]
    public string itemName;
    [Range(0f, 1f)] public float weight;
    public float useTime = 1f;
    public GameObject prefab;
    


    private bool isBeingUsed = false;
    private Coroutine useRoutine;

    // Reference to the use indicator UI
    public UseIndicatorUI useIndicator;

    public void BeginUse()
    {
        if (isBeingUsed) return;
        useRoutine = StartCoroutine(UseRoutine());
    }

    public void CancelUse()
    {
        if (!isBeingUsed) return;

        StopCoroutine(useRoutine);
        isBeingUsed = false;

        if (useIndicator != null)
            useIndicator.ResetProgress();

        UnityEngine.Debug.Log($"{itemName} use cancelled!");
    }

    private IEnumerator UseRoutine()
    {
        isBeingUsed = true;
        UnityEngine.Debug.Log($"Started using {itemName}...");

        float elapsed = 0f;
        while (elapsed < useTime)
        {
            elapsed += Time.deltaTime;

            // Update progress in the UI
            if (useIndicator != null)
                useIndicator.UpdateProgress(elapsed / useTime);

            yield return null;
        }

        // Completed use
        isBeingUsed = false;

        if (useIndicator != null)
            useIndicator.ResetProgress();

        Use();
        UnityEngine.Debug.Log($"{itemName} use complete!");
    }

    public abstract void Use();
}
