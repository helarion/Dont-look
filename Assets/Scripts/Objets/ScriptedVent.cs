using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptedVent : Objet
{
    [SerializeField] GameObject normalVent;
    [SerializeField] GameObject brokenVent;

    public override void Activate()
    {
        isActivated = true;
        normalVent.SetActive(false);
        brokenVent.SetActive(true);
    }

    public override void Reset()
    {
        base.Reset();
        normalVent.SetActive(true);
        brokenVent.SetActive(false);
    }
}
