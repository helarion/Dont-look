using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightDetector : Objet
{
    [SerializeField] public Objet[] targets =null;
    [SerializeField] private float delayActivate = 1.5f;
    [SerializeField] private BlinkingLight blinkLight = null;
    [SerializeField] private string playChargingSound = null;
    [SerializeField] private string StopChargingSound = null;
    [SerializeField] private string activateSound = null;
    [SerializeField] private bool desactivate = false;
    [SerializeField] private bool scriptSpider = false;
    [SerializeField] private GameObject[] brokenFeature;
    private MeshRenderer model;
    [HideInInspector] public bool isLooked = false;
    private float countLook=0f;

    private void Start()
    {
        model = GetComponentInChildren<MeshRenderer>();
        if(isBroken)
        {
            Break();
        }
    }

    private void Update()
    {
        //print(GameManager.instance.LightDetection(transform.position, true));
        IsLit(GameManager.instance.LightDetection(transform,true));
    }

    private void IsLit(bool b)
    {
        if (!isLooked && !isActivated && b)
        {
            isLooked = true;
            StopCoroutine(StopLook());
            StartCoroutine(CountLook());
        }
        else if (!b && !isActivated && !scriptSpider)
        {
            StopCoroutine(CountLook());
            StartCoroutine(StopLook());
            isLooked = false;
            //countLook = 0f;
        }
    } 
    
    private IEnumerator StopLook()
    {
        blinkLight.StopLook(delayActivate);
        while (countLook > 0)
        {
            AkSoundEngine.SetRTPCValue("Pitch_Load_Light" + countLook.Remap(0, delayActivate, 0, 100), 0);
            countLook -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        isLooked = false;
        yield return null;
    }

    // COROUTINE POUR COMPTER LE TEMPS QUE L'OBJET EST REGARDE PAR LE JOUEUR
    private IEnumerator CountLook()
    {
        isLooked = true;
        blinkLight.StartLook(delayActivate);
        AkSoundEngine.PostEvent(playChargingSound, gameObject);
        while (countLook < delayActivate)
        {
            AkSoundEngine.SetRTPCValue("Pitch_Load_Light", countLook.Remap(0,delayActivate,0,100));
            countLook +=Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Activate();
        isLooked = false;
        yield return null;
    }

    public void ForceLit()
    {
        StartCoroutine(CountLook());
    }

    public override void Activate()
    {
        if (isActivated && !desactivate) return;
        base.Activate();
        if (scriptSpider) 
        if (isBroken)
        {
            blinkLight.enabled = true;
            blinkLight.Fix();
        }
        blinkLight.Activate();
        AkSoundEngine.PostEvent(activateSound, gameObject);
        foreach(Objet o in targets)
        {
            if (!o.enabled)
            {
                o.enabled = true;
                o.Fix();
            }
            else if (o.isActivated && desactivate) o.Desactivate();
            else o.Activate();
        }
        isActivated = true;
        print("Object activated");
    }

    public override void Break()
    {
        foreach (GameObject g in brokenFeature)
        {
            g.SetActive(true);
        }
        blinkLight.Break();
        base.Break();
    }

    public override void Fix()
    {
        foreach (GameObject g in brokenFeature)
        {
            g.SetActive(false);
        }
        blinkLight.Fix();
        base.Fix();
    }

    public override void Reset()
    {
        base.Reset();
        blinkLight.Reset();
        isLooked = false;
        countLook = 0f;
        if (isBroken)
        {
            blinkLight.Break();
            Break();
        }
    }
}
