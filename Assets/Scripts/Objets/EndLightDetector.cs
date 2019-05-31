using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndLightDetector : ContiniousLightDetector
{
    [SerializeField] string dogDeathSound;
    [SerializeField] private float scriptEndTimeLook = 1.5f;
    [SerializeField] private Elevator scriptEndElevator;
    [SerializeField] float waitTime = 5.0f;
    [SerializeField] CameraBlock cameraBlock = null;
    [SerializeField] float newYaw = 30.0f;
    [SerializeField] float newZ = 2.0f;
    [SerializeField] float cinematicBarsTime= 1.5f;
    [SerializeField] float cinematicBarsSize = 2;

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
            StartCoroutine(DogDeathCoroutine());
        }
        else
        {
            base.LookFunction();
        }
    }

    IEnumerator DogDeathCoroutine()
    {
        UIManager.instance.cinematicBars.Show(cinematicBarsSize, cinematicBarsTime);
        float oldYaw = cameraBlock.maxCameraYawAngle;
        float oldZ = GameManager.instance.camHandler.zStartPosition;
        GameManager.instance.player.StopMove();
        cameraBlock.maxCameraYawAngle = newYaw;
        GameManager.instance.camHandler.SetNewZ(newZ);
        yield return new WaitForSeconds(waitTime);
        GameManager.instance.player.ResumeMove();
        cameraBlock.maxCameraYawAngle = oldYaw;
        GameManager.instance.camHandler.SetNewZ(oldZ);
        UIManager.instance.cinematicBars.Hide(cinematicBarsTime);
        yield return null;
    }
}
