using UnityEngine;

public class WaterCollision : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Water Collision Detected");
        // compare layer
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
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
            Debug.Log("Object with tag " + collision.gameObject.tag + " collided with water.");
        }
    }
}
