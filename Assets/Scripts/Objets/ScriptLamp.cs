using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptLamp : Objet
{
    [SerializeField] private Light lt = null;
    [SerializeField] private SpiderBehavior spider =null;
    bool isEnabled = false;
    [SerializeField] AK.Wwise.Trigger trigger=null;
    [SerializeField] GameObject swingingLight;

    private void Start()
    {
        lt.enabled = isEnabled;
    }

    public void swing(Vector3 swingVelocity)
    {
        swingingLight.GetComponent<Rigidbody>().AddForce(swingVelocity * 1000);
    }

    public override void Activate()
    {
        isEnabled = true;
        lt.enabled = isEnabled;
        spider.StartChase();
        AkSoundEngine.PostEvent(trigger.Id,gameObject);
        swing(Vector3.right);
    }

    public override void Reset()
    {
        base.Reset();
        isEnabled = false;
        lt.enabled = isEnabled;
    }
}
