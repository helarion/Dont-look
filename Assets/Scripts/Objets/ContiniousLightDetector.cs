using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContiniousLightDetector : Objet
{
    [SerializeField] public Objet target;
    [SerializeField] private string chargingSound = null;

    private void Update()
    {
        target.isActivating=GameManager.instance.LightDetection(transform,true);
    }



    public override void Reset()
    {
        base.Reset();
    }
}
