using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndLightDetector : ContiniousLightDetector
{
    [SerializeField] string dogDeathSound;
    [SerializeField] private float scriptEndTimeLook = 1.5f;
    [SerializeField] private Elevator scriptEndElevator;
    [SerializeField] float waitTime=7;

    public override void LookFunction()
    {
        if (count > scriptEndTimeLook && !isBroken)
        {
            target.isActivating = false;
            isBroken = true;
            AkSoundEngine.PostEvent(stopChargingSound, gameObject);
            AkSoundEngine.PostEvent(breakSound, gameObject);
            AkSoundEngine.PostEvent(dogDeathSound, target.gameObject);
            blinkingLight.Break();
            Break();
        }
        base.LookFunction();
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(waitTime);
        scriptEndElevator.Activate();
    }
}
