using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptLamp : Objet
{
    [SerializeField] private Light lt = null;
    bool isEnabled = false;

    private void Start()
    {
        lt.enabled = isEnabled;
    }

    public override void Activate()
    {
        isEnabled = true;
        lt.enabled = isEnabled;
    }

    public override void Reset()
    {
        base.Reset();
        isEnabled = false;
        lt.enabled = isEnabled;
    }
}
