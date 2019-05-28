using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndLightDetector : ContiniousLightDetector
{
    [SerializeField] string dogDeathSound;
    [SerializeField] private float scriptEndTimeLook = 1.5f;
    [SerializeField] private Elevator scriptEndElevator;

    public override void LookFunction()
    {
        if (count > scriptEndTimeLook && !isBroken)
        {
            if (target.GetComponent<SlidingDoor>() != null)
            {
                target.GetComponent<SlidingDoor>().force_activation_state(false);
            }
            else
            {
                target.isActivating = false;
            }
            broken = true;
            AkSoundEngine.PostEvent(stopChargingSound, gameObject);
            AkSoundEngine.PostEvent(breakSound, gameObject);
            AkSoundEngine.PostEvent(dogDeathSound, target.gameObject);
            blinkingLight.Break();
            Break();
            scriptEndElevator.Activate();
        }
        else
        {
            base.LookFunction();
        }
    }
}
