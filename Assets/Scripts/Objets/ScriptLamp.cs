using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptLamp : Objet
{
    [SerializeField] private Light lt = null;
    [SerializeField] private ScriptedSpider spider =null;
    bool isEnabled = false;
    [SerializeField] string trigger;
    [SerializeField] GameObject swingingLight;

    private void Start()
    {
        lt.enabled = isEnabled;
    }

    public void Swing()
    {
        swingingLight.GetComponent<Rigidbody>().AddForce(Vector3.left * 3000);
    }

    public override void Activate()
    {
        isEnabled = true;
        lt.enabled = isEnabled;
        spider.Script();
        AkSoundEngine.PostEvent(trigger,gameObject);
        //swing);
    }

    public override void Reset()
    {
        base.Reset();
        isEnabled = false;
        lt.enabled = isEnabled;
    }
}
