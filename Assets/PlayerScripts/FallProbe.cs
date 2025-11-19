using UnityEngine;

public class FallProbe : MonoBehaviour
{
    public Rigidbody rb;
    public float lastY;

    public void Activate(Vector3 position)
    {
        gameObject.SetActive(true);

        transform.position = position;
        lastY = position.y;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void Deactivate()
    {
        // Disable simulation entirely
        gameObject.SetActive(false);
    }
}
