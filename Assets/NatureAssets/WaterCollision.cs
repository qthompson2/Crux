using UnityEngine;

public class WaterCollision : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Water Collision Detected");
        // compare layer
        if (other.gameObject.tag == "WaterTrigger")
        {
            Debug.Log("Player collided with water.");
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                StaminaManager staminaManager = player.GetComponent<StaminaManager>();
                if (staminaManager != null)
                {
                    staminaManager.AddDamage(1f);
                }
            }
            
        }
        else {
            // log the tag of the object that collided with water
            Debug.Log("Object with tag " + other.gameObject.tag + " collided with water.");
        }
    }
}
