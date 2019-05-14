using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContiniousLightDetector : Objet
{
    [SerializeField] public Objet target;
    [SerializeField] private string playChargingSound = null;
    [SerializeField] private string stopChargingSound = null;
    [SerializeField] private BlinkingLight blinkingLight;
    [SerializeField] private float chargeTime = 0.5f;
    [SerializeField] private GameObject[] brokenFeature;

    private bool hasPlayedCharge = false;

    private void Start()
    {
        if(isBroken)
        {
            Break();
        }
        AkSoundEngine.SetRTPCValue("Pitch_Load_Light", 0);
    }

    private void Update()
    {
        bool isLooked = GameManager.instance.LightDetection(transform, true);
        target.isActivating=isLooked;
        if (isLooked)
        {
            if (!hasPlayedCharge)
            {
                hasPlayedCharge = true;
                AkSoundEngine.PostEvent(playChargingSound, gameObject);
                //AkSoundEngine.SetRTPCValue("Pitch_Load_Light",0);
                StartCoroutine(StartingCoroutine());
                blinkingLight.StartLook(chargeTime);
            }
        }
        else
        {
            if(hasPlayedCharge)
            {
                hasPlayedCharge = false;
                StartCoroutine(StoppingCoroutine());
                blinkingLight.StopLook(chargeTime);
            }
        }
    }

    private IEnumerator StartingCoroutine()
    {
        float count = 0;
        while(count<chargeTime)
        {
            AkSoundEngine.SetRTPCValue("Pitch_Load_Light",count.Remap(0, chargeTime, 0, 100));
            count += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    private IEnumerator StoppingCoroutine()
    {
        float count = chargeTime;
        while (count > 0)
        {
            AkSoundEngine.SetRTPCValue("Pitch_Load_Light",count.Remap(0, chargeTime, 0, 100));
            count -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        AkSoundEngine.PostEvent(stopChargingSound, gameObject);
        yield return null;
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
