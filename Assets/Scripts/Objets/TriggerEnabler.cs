using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEnabler : Objet
{
    [SerializeField] WwiseTrigger wwiseTrigger;
    [SerializeField] Checkpoint checkPointTrigger;

    public override void Activate()
    {
        isActivated = true;
        if (wwiseTrigger != null) wwiseTrigger.gameObject.SetActive(true);
        if (checkPointTrigger != null) checkPointTrigger.gameObject.SetActive(true);
    }

    public override void Reset()
    {
        base.Reset();
        if(wwiseTrigger!=null && wwiseTrigger.gameObject.activeSelf)
        {
            wwiseTrigger.Reset();
            wwiseTrigger.gameObject.SetActive(false);
        }
        /*else if (checkPointTrigger != null && checkPointTrigger.gameObject.activeSelf)
        {
            wwiseTrigger.gameObject.SetActive(false);
        }*/
    }
}
