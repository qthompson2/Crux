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
        Debug.Log(other.gameObject.name);
        Debug.Log(other.gameObject.tag);
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<StaminaManager>().AddDamage();
        }
    }
}
