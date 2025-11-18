using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class SpiderController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float followSpeed = 10f;
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float stickForce = 1f;
    [SerializeField] [Range(0, 1)] private float gravityModifier = 0.1f;
    private new Rigidbody rigidbody;
    private Animator animator;
    private bool alignAllowed = true;

	void Start()
	{
        rigidbody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        rigidbody.useGravity = false;
	}

    void Update()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, target.position);
        float scaledSpeed = followSpeed * distance;
        rigidbody.AddForce(scaledSpeed * Time.deltaTime * direction);

        if (rigidbody.linearVelocity.sqrMagnitude > 0.2f) // Avoid jitter when nearly stationary
        {
            Quaternion targetRotation = Quaternion.LookRotation(rigidbody.linearVelocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            alignAllowed = false;
		}
		else
		{
            alignAllowed = true;
		}
        animator.SetFloat("Speed", math.clamp(math.abs(direction.x + direction.y + direction.z), 0, 1));

        rigidbody.AddForce(gravityModifier * 9.8f * Vector3.down, ForceMode.Acceleration);
    }

    void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            rigidbody.AddForce(-contact.normal * stickForce, ForceMode.Acceleration);
            AlignToSurface(contact.normal);
        }
    }
    
    private void AlignToSurface(Vector3 surfaceNormal)
    {
        if (alignAllowed)
		{
            Quaternion targetRotation = Quaternion.LookRotation(transform.forward, surfaceNormal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
		}
    }

}
