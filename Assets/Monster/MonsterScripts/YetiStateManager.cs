using UnityEngine;
using UnityEngine.Rendering;
using VolFx;
using System.Collections;

public class YetiStateManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Transform player;
    [SerializeField] public Transform target;
    [SerializeField] public AgentController agentController;
    [SerializeField] public VhsVol vhsEffect;
    
    void Awake()
    {
        agentController = GetComponent<AgentController>();
        Volume volume = GameObject.FindGameObjectWithTag("PostProcess").GetComponent<Volume>();
        if (volume != null)
        {
            volume.profile.TryGet<VhsVol>(out vhsEffect);
        }
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }
    
    void Update()
    {
        target.position = player.position;
        vhsEffect._weight.value = 0.5f;
    }
}
