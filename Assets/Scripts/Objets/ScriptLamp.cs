using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptLamp : Objet
{
    [SerializeField] private Light lt = null;
    [SerializeField] private SpiderBehavior spider =null;
    bool isEnabled = false;
    [SerializeField] AK.Wwise.Trigger trigger=null;

    private void Start()
    {
        lt.enabled = isEnabled;
    }

    public override void Activate()
    {
        isEnabled = true;
        lt.enabled = isEnabled;
        spider.StartChase();
        AkSoundEngine.PostEvent(trigger.Id,gameObject);
    }

    public override void Reset()
    {
        base.Reset();
        isEnabled = false;
        lt.enabled = isEnabled;
    }
}
