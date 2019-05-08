using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivableLamp : Objet
{
    [SerializeField] private Light lt = null;

    private void Start()
    {
        lt.enabled = isActivated;
    }

    public override void Activate()
    {
        isActivated = true;
        lt.enabled = isActivated;
    }

    public override void Desactivate()
    {
        isActivated = false;
        lt.enabled = isActivated;
    }

    public override void Reset()
    {
        base.Reset();
        lt.enabled = isActivated;
    }
}
