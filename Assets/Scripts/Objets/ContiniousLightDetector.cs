﻿using System.Collections;
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
    [SerializeField] private bool scriptEndBreak = false;
    [SerializeField] private float scriptEndTimeLook = 1.5f;
    [SerializeField] private Elevator scriptEndElevator;
    [SerializeField] private string breakSound;
    [SerializeField] private string fixSound;

    bool isLooked = false;
    bool wasLooked = false;
    float count=0;
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
        wasLooked = isLooked;
        isLooked = GameManager.instance.LightDetection(gameObject, true);
        target.isActivating = isLooked;
        if (isLooked)
        {
            count += Time.deltaTime;
            if (scriptEndBreak && count > scriptEndTimeLook &&!isBroken)
            {
                target.isActivating = false;
                isBroken = true;
                scriptEndElevator.Activate();
                AkSoundEngine.PostEvent(stopChargingSound, gameObject);
                AkSoundEngine.PostEvent(breakSound, gameObject);
                blinkingLight.Break();
                Break();
            }

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
        AkSoundEngine.PostEvent(fixSound, gameObject);
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
