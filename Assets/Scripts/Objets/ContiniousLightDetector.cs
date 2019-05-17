using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContiniousLightDetector : Objet
{
    [SerializeField] public Objet target;
    [SerializeField] private string playChargingSound = null;
    [SerializeField] private string stopChargingSound = null;
    [SerializeField] private ContinuousBlinkingLight blinkingLight;
    [SerializeField] private float chargeTime = 0.5f;
    [SerializeField] private GameObject[] brokenFeature;

    bool isLooked = false;
    bool wasLooked = false;

    float timeLooked = 0.0f;

    bool chargeSoundPlaying = false;

    private void Start()
    {
        if (isBroken)
        {
            Break();
        }
        AkSoundEngine.SetRTPCValue("Pitch_Load_Light", 0);
    }

    private void Update()
    {
        wasLooked = isLooked;
        isLooked = GameManager.instance.LightDetection(transform, true);
        target.isActivating = isLooked;
        if (isLooked)
        {
            if (!wasLooked)
            {
                AkSoundEngine.PostEvent(playChargingSound, gameObject);
                chargeSoundPlaying = true;
                blinkingLight.StartLook(chargeTime);
                timeLooked = 0.0f;
            }

            if (timeLooked < chargeTime)
            {
                AkSoundEngine.SetRTPCValue("Pitch_Load_Light", timeLooked.Remap(0, chargeTime, 0, 100));
                timeLooked += Time.deltaTime;
            }
        }
        else
        {
            if (wasLooked)
            {
                blinkingLight.StopLook();
                timeLooked = chargeTime;
            }

            if (timeLooked > 0.0f)
            {
                AkSoundEngine.SetRTPCValue("Pitch_Load_Light", timeLooked.Remap(0, chargeTime, 0, 100));
                timeLooked -= Time.deltaTime;
            }
            else if (chargeSoundPlaying)
            {
                AkSoundEngine.PostEvent(stopChargingSound, gameObject);
                chargeSoundPlaying = false;
            }
        }
    }

    public override void Fix()
    {
        base.Fix();
        foreach (GameObject g in brokenFeature)
        {
            g.SetActive(false);
        }
        blinkingLight.Fix();
    }

    public override void Break()
    {
        foreach(GameObject g in brokenFeature)
        {
            g.SetActive(true);
        }
        blinkingLight.Break();
        base.Break();
    }

    public override void Reset()
    {
        base.Reset();
        target.Reset();
        if (isBroken)
        {
            blinkingLight.Break();
            Break();
        }
    }
}
