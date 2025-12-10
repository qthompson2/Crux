using UnityEngine;

public class WaterCollision : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "WaterTrigger")
        {
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
    }
}
