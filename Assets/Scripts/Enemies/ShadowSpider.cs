using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ShadowSpider : Enemy
{
    [SerializeField] Transform pos1;
    [SerializeField] float scriptIntensity;
    [SerializeField] private bool hasPlayedLook = false;

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        VelocityCount();
        LightDetection();
        if (isChasing)
        {
            ChaseBehavior();
        }

        if (agent.isOnOffMeshLink)
        {
            agent.CompleteOffMeshLink();
            agent.isStopped = false;
        }
    }


    // APPELER LORSQUE L'ARAIGNEE EST ECLAIREE
    public override void IsLit(bool b)
    {
        if (b)
        {
            if (!hasPlayedLook)
            {
                AkSoundEngine.PostEvent(WwiseLook, gameObject);
                hasPlayedLook = true;
            }
            GameManager.instance.ShakeScreen(0.1f, lookShakeIntensity);
        }
    }
}
