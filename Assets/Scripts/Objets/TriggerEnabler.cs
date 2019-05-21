using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEnabler : Objet
{
    [SerializeField] WwiseTrigger wwiseTrigger;

    public override void Activate()
    {
        isActivated = true;
        wwiseTrigger.gameObject.SetActive(true);
    }

    public override void Reset()
    {
        base.Reset();
        if(wwiseTrigger.gameObject.activeSelf)
        {
            wwiseTrigger.Reset();
            wwiseTrigger.gameObject.SetActive(false);
        }
    }
}
