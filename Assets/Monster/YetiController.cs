using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class YetiController : MonoBehaviour
{
    [SerializeField] GameObject agent;

	void Update()
	{
		transform.position = agent.transform.position;
        Quaternion sourceRot = transform.rotation;
        Quaternion targetRot = agent.transform.rotation;

        // Extract Euler angles
        Vector3 sourceEuler = sourceRot.eulerAngles;
        sourceEuler.y = targetRot.eulerAngles.y;

        // Reapply
        transform.rotation = Quaternion.Euler(sourceEuler);
	}

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            StaminaManager staminaManager = other.gameObject.GetComponent<StaminaManager>();
            if (staminaManager != null)
            {
                if (!staminaManager.hasBeenLifted)
                {
                    staminaManager.hasBeenLifted = true;
                }
                staminaManager.AddDamage();
            }
        }
    }
}
