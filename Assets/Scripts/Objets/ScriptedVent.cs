using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptedVent : Objet
{
    [SerializeField] GameObject normalVent;
    [SerializeField] GameObject brokenVent;
    [SerializeField] string grilleFallSound;

    public override void Activate()
    {
        isActivated = true;
        AkSoundEngine.PostEvent(grilleFallSound, gameObject);
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
