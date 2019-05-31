using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContiniousLightDetector : Objet
{
    [SerializeField] public Objet target;
    [SerializeField] public string playChargingSound = null;
    [SerializeField] public string stopChargingSound = null;
    [SerializeField] public ContinuousBlinkingLight blinkingLight;
    [SerializeField] public ContinuousBlinkingLight targetBlinkLight1;
    [SerializeField] public ContinuousBlinkingLight targetBlinkLight2;
    [SerializeField] private float chargeTime = 0.5f;
    [SerializeField] private GameObject[] brokenFeature;
    [SerializeField] public string breakSound;
    [SerializeField] private string fixSound;

    protected bool broken = false;
    bool isLooked = false;
    bool wasLooked = false;
    bool firstTimeLook = false;
    float timeLooked = 0.0f;

    bool chargeSoundPlaying = false;

    //float value = 0.0f;

    public float count = 0.0f;

    private void Start()
    {
        AkSoundEngine.SetRTPCValue("Pitch_Load_Light", 0);
        //value = 0.0f;
        if (isBroken)
        {
            Break();
        }
    }

    public virtual void Update()
    {
        if (broken) return;


        wasLooked = isLooked;
        isLooked = GameManager.instance.LightDetection(gameObject, true);
        //if (wasLooked || isLooked) print("rtpc value :" + value);

        if (target.GetComponent<SlidingDoor>() != null)
        {
            target.GetComponent<SlidingDoor>().set_activation_state(isLooked);
        }
        else
        {
            target.isActivating = isLooked;
        }

        if (isLooked)
        {
            if (!firstTimeLook)
            {
                firstTimeLook = true;
                AkSoundEngine.SetRTPCValue("Pitch_Load_Light", 0);
                //value = 0.0f;
            }
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
                //value = timeLooked.Remap(0, chargeTime, 0, 100);
                timeLooked -= Time.deltaTime;
            }
            else if (chargeSoundPlaying)
            {
                AkSoundEngine.PostEvent(stopChargingSound, gameObject);
                chargeSoundPlaying = false;
                AkSoundEngine.SetRTPCValue("Pitch_Load_Light", 0);
                //value = 0.0f;
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
            //value = timeLooked.Remap(0, chargeTime, 0, 100);
            timeLooked += Time.deltaTime;
        }
    }

    public override void Fix()
    {
        broken = false;
        base.Fix();
        AkSoundEngine.PostEvent(fixSound, gameObject);
        AkSoundEngine.SetRTPCValue("Pitch_Load_Light", 0);
        //value = 0.0f;
        foreach (GameObject g in brokenFeature)
        {
            g.SetActive(false);
        }
        blinkingLight.Fix();
        if (targetBlinkLight1 != null)
        {
            targetBlinkLight1.StopBlink();
            targetBlinkLight1.enabled = false;
            targetBlinkLight2.StopBlink();
            targetBlinkLight2.enabled = false;
        }
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
        if (targetBlinkLight1 != null)
        {
            targetBlinkLight1.enabled = true;
            targetBlinkLight2.enabled = true;
        }
    }

    public override void Reset()
    {
        broken = false;
        base.Reset();
        target.Reset();
        firstTimeLook = false;
        AkSoundEngine.SetRTPCValue("Pitch_Load_Light", 0);
        //value = 0.0f;
        if (isBroken)
        {
            blinkingLight.Break();
            Break();
        }
    }
}
