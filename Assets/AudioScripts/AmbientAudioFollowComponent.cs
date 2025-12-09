using UnityEngine;

public class AmbientAudioFollowComponent : MonoBehaviour
{
    public Collider Area;
    public GameObject Player;
    // Update is called once per frame
    void Update()
    {
        Vector3 closestPont = Area.ClosestPoint(Player.transform.position);
        transform.position = closestPont;
    }
}
