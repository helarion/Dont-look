using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightDetector : Objet
{
    [SerializeField] public Objet[] targets =null;
    [SerializeField] private float delayActivate = 1.5f;
    [SerializeField] public float dechargeRate = 1.0f;
    [SerializeField] protected BlinkingLight blinkLight = null;
    [SerializeField] private string playChargingSound = null;
    [SerializeField] private string StopChargingSound = null;
    [SerializeField] private string activateSound = null;
    [SerializeField] private bool desactivate = false;
    //[SerializeField] private GameObject[] brokenFeature;
    private MeshRenderer model;

    [HideInInspector] public bool broken = false;

    bool wasLooked = false;
    [HideInInspector] public bool isLooked = false;

    private float timeLooked = 0.0f;

    bool chargeSoundPlaying = false;
    bool firstTimeLook = false;
    bool forceLit = false;

    private void Start()
    {
        blinkLight.dechargeRate = dechargeRate;
        model = GetComponentInChildren<MeshRenderer>();
        timeLooked = 0.0f;
        AkSoundEngine.SetRTPCValue("Pitch_Load_Light", 0);
        if (isBroken)
        {
            Break();
        }
    }

    private void Update()
    {
        if (isActivated || broken) return;

        wasLooked = isLooked;
        isLooked = GameManager.instance.LightDetection(gameObject, true);

        if (isLooked || forceLit)
        { 
            if(!firstTimeLook)
            {
                firstTimeLook = true;
                AkSoundEngine.SetRTPCValue("Pitch_Load_Light", 0);
            }
            if (!wasLooked)
            {
                if (!chargeSoundPlaying)
                {
                    AkSoundEngine.PostEvent(playChargingSound, gameObject);
                    chargeSoundPlaying = true;
                }
                blinkLight.StartLook(delayActivate);
            }

            if (timeLooked < delayActivate)
            {
                AkSoundEngine.SetRTPCValue("Pitch_Load_Light", timeLooked.Remap(0, delayActivate, 0, 100));
                //print(timeLooked.Remap(0, delayActivate, 0, 100));
                timeLooked += Time.deltaTime;
            }
            else if (!isActivated)
            {
                Activate();
            }
        }
        //else if (!scriptSpider)
        else
        {
            if (wasLooked)
            {
                blinkLight.StopLook();
                //timeLooked = delayActivate;
            }

            if (timeLooked > 0.0f)
            {
                AkSoundEngine.SetRTPCValue("Pitch_Load_Light", timeLooked.Remap(0, delayActivate, 0, 100));
                timeLooked -= Time.deltaTime * dechargeRate;
            }
            else if (chargeSoundPlaying)
            {
                AkSoundEngine.PostEvent(StopChargingSound, gameObject);
                chargeSoundPlaying = false;
                AkSoundEngine.SetRTPCValue("Pitch_Load_Light", 0);
            }
        }
    }

    public void ForceLit()
    {
        forceLit = true;
    }

    public override void Activate()
    {
        if (isActivated && !desactivate) return;
        base.Activate();
        if (isBroken)
        {
            blinkLight.enabled = true;
            blinkLight.Fix();
        }
        blinkLight.Activate();
        AkSoundEngine.PostEvent(activateSound, gameObject);
        foreach(Objet o in targets)
        {
            if (!o.enabled)
            {
                o.enabled = true;
                o.Fix();
            }
            else if (o.isActivated && desactivate) o.Desactivate();
            else o.Activate();
        }
        isActivated = true;
        //print("Object activated");
    }

    public override void Reset()
    {
        base.Reset();
        forceLit = false;
        firstTimeLook = false;
        blinkLight.Reset();
        isLooked = false;
        broken = false;
        chargeSoundPlaying = false;
        timeLooked = 0.0f;
        AkSoundEngine.SetRTPCValue("Pitch_Load_Light", 0);
        if (isBroken)
        {
            blinkLight.Break();
            Break();
        }
    }
}
