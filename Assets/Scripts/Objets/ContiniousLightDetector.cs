using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContiniousLightDetector : Objet
{
    [SerializeField] public Objet target;
    [SerializeField] public string playChargingSound = null;
    [SerializeField] public string stopChargingSound = null;
    [SerializeField] public ContinuousBlinkingLight blinkingLight;
    [SerializeField] private float chargeTime = 0.5f;
    [SerializeField] private GameObject[] brokenFeature;
    [SerializeField] public string breakSound;
    [SerializeField] private string fixSound;

    bool broken = false;
    bool isLooked = false;
    bool wasLooked = false;
    [HideInInspector] public float count=0;
    float timeLooked = 0.0f;

    bool chargeSoundPlaying = false;

    private void Start()
    {
        AkSoundEngine.SetRTPCValue("Pitch_Load_Light", 0);
        if (isBroken)
        {
            Break();
        }
    }

    private void Update()
    {
        if (broken) return;

        wasLooked = isLooked;
        isLooked = GameManager.instance.LightDetection(gameObject, true);
        target.isActivating = isLooked;
        if (isLooked)
        {
            LookFunction();
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

    public virtual void LookFunction()
    {
        count += Time.deltaTime;
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

    public override void Fix()
    {
        broken = false;
        base.Fix();
        AkSoundEngine.PostEvent(fixSound, gameObject);
        AkSoundEngine.SetRTPCValue("Pitch_Load_Light", 0);
        foreach (GameObject g in brokenFeature)
        {
            g.SetActive(false);
        }
        blinkingLight.Fix();
    }

    public override void Break()
    {
        broken = true;
        foreach(GameObject g in brokenFeature)
        {
            g.SetActive(true);
        }
        blinkingLight.Break();
        base.Break();
    }

    public override void Reset()
    {
        broken = false;
        base.Reset();
        target.Reset();
        AkSoundEngine.SetRTPCValue("Pitch_Load_Light", 0);
        if (isBroken)
        {
            blinkingLight.Break();
            Break();
        }
    }
}
