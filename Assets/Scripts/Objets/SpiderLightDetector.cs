using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderLightDetector : LightDetector
{

    [SerializeField] private bool scriptSpider = false;
    [SerializeField] private GameObject[] brokenFeature;
    [SerializeField] private SpiderBehavior spider;
    [SerializeField] private float timeBeforeActivatingSpider = 0.5f;

    public override void Activate()
    {
        base.Activate();
        StartCoroutine(ActivateSpider());
    }

    IEnumerator ActivateSpider()
    {
        yield return new WaitForSeconds(timeBeforeActivatingSpider);
        spider.StartChase();
    }

    public override void Break()
    {
        broken = true;
        foreach (GameObject g in brokenFeature)
        {
            g.SetActive(true);
        }
        blinkLight.Break();
        base.Break();
    }

    public override void Fix()
    {
        broken = false;
        foreach (GameObject g in brokenFeature)
        {
            g.SetActive(false);
        }
        blinkLight.Fix();
        base.Fix();
    }
}
