using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContiniousLightDetector : Objet
{
    [SerializeField] public Objet target;
    [SerializeField] private string chargingSound = null;
    [SerializeField] private BlinkingLight blinkingLight;
    [SerializeField] private GameObject[] brokenFeature;

    private void Start()
    {
        if(isBroken)
        {
            Break();
        }
    }

    private void Update()
    {
        target.isActivating=GameManager.instance.LightDetection(transform,true);
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
