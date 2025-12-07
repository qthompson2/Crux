using UnityEngine;

public class RotateBeam : MonoBehaviour
{
    [Tooltip("Degrees per second")]
    public float speed = 30f;

    void Update()
    {
        transform.Rotate(Vector3.up, speed * Time.deltaTime, Space.Self);
    }
}
