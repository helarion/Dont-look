using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEnabler : Objet
{
    [SerializeField] WwiseTrigger wwiseTrigger;
    [SerializeField] Checkpoint checkPointTrigger;
    [SerializeField] GameObject spider;

    public override void Activate()
    {
        isActivated = true;
        if (wwiseTrigger != null) wwiseTrigger.gameObject.SetActive(true);
        if (checkPointTrigger != null) checkPointTrigger.gameObject.SetActive(true);
        if (spider != null) spider.SetActive(true);
    }

    public override void Reset()
    {
        base.Reset();
        if(wwiseTrigger!=null && wwiseTrigger.gameObject.activeSelf)
        {
            wwiseTrigger.Reset();
            wwiseTrigger.gameObject.SetActive(false);
        }
    }
}
