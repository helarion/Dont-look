using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ShadowSpider : Enemy
{
    [SerializeField] Transform endPos;

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        VelocityCount();
        LightDetection();
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
            MoveTo(endPos.position);
            isMoving = true;
        }
    }
}
