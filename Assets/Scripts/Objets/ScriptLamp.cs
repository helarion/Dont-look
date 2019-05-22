using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptLamp : Objet
{
    [SerializeField] private Light lt = null;
    [SerializeField] private ScriptedSpider spider =null;
    [SerializeField] string swingingLampSound;
    [SerializeField] string lampActivateSound;
    [SerializeField] GameObject swingingLight;

    private void Start()
    {
        lt.enabled = isActivated;
    }

    public void Swing()
    {
        swingingLight.GetComponent<Rigidbody>().AddForce(Vector3.left * 3000);
    }

    public override void Activate()
    {
        isActivated = true;
        lt.enabled = isActivated;
        AkSoundEngine.PostEvent(lampActivateSound, gameObject);
        spider.Script();
        //AkSoundEngine.PostEvent(swingingLampSound,gameObject);
    }

    public override void Reset()
    {
        base.Reset();
        lt.enabled = isActivated;
    }
}
