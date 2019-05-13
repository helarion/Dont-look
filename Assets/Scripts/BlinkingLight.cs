using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkingLight : MonoBehaviour
{
    [SerializeField] Light lt=null;
    [SerializeField] Light pointLt = null;

    [SerializeField] float startingIntensity;
    [SerializeField] private float blinkSpeed = 20;
    [SerializeField] private float maxIntensity = 200;
    [SerializeField] private float wait = 2;
    [SerializeField] private float backSpeed = 0.5f;
    private int direction = 1;

    [SerializeField] private Color startColor;
    [SerializeField] private Color endColor;
    [SerializeField] private Color activatedColor;
    [SerializeField] private bool isContinious = false;
    [SerializeField] private bool isBroken = false;

    private Color FinishedColor;
    private float count = 0;
    private float countTime = 0;
    private bool isWaiting = false;
    private bool isBlinking = true;
    private bool isActivated = false;

    private void Start()
    {
        StartBlink();
        if (isBroken)
        {
            Break();
        }
    }

    public void StartBlink()
    {
        if (isBlinking) return;
        isBlinking = true;
        count = 0;
        /*lt.intensity = 0;
        lt.color = startColor;
        pointLt.intensity = 0;
        pointLt.color = startColor;*/
    }

    public void StartLook(float time)
    {
        if (!isBlinking) return;
        count = 0;
        countTime = time;
        lt.intensity = startingIntensity;
        isBlinking = false;
    }

    public void Reset()
    {
        count = 0;
        pointLt.intensity = 0;
        pointLt.color = startColor;
        isWaiting = false;
        isActivated = false;
        StartBlink();
        if (isBroken) Break();
    }

    private IEnumerator DarkWait()
    {
        if(!isWaiting)
        {
            float count = 0;
            isWaiting = true;
            while (count < wait)
            {
                count += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            lt.intensity = 0.1f;
            isWaiting = false;
            yield return null;
        }
    }

    private void Update()
    {
        if(isBlinking)
        {
            if(pointLt.intensity>0)
            {
                pointLt.intensity = Mathf.Lerp(pointLt.intensity, 0, count);
                pointLt.color = Color.Lerp(pointLt.color, startColor, count);
                lt.color = Color.Lerp(lt.color, startColor, count);
                count += Time.deltaTime / backSpeed;
            }
            else if (lt.intensity >= startingIntensity)
            {
                direction = -1;
            }
            else if (lt.intensity <= 0 &&!isWaiting)
            {
                direction = 1;
                StartCoroutine(DarkWait());
            }
            if(!isWaiting)lt.intensity += (Time.deltaTime * blinkSpeed) * direction;
        }
        else if(!isActivated)
        {
            if(isContinious)
            {
                lt.intensity = Mathf.Lerp(startingIntensity, maxIntensity, count);
                pointLt.color = Color.Lerp(startColor, activatedColor, count);
                lt.color = Color.Lerp(startColor, activatedColor, count);
            }
            else
            {
                pointLt.color = Color.Lerp(startColor, endColor, count);
            }
            pointLt.intensity = Mathf.Lerp(0, maxIntensity, count);
            lt.intensity = Mathf.Lerp(startingIntensity, maxIntensity, count);
            count += Time.deltaTime/countTime;
        }
    }

    public void Activate()
    {
        if (isContinious) return;
        isActivated = true;
        pointLt.color = activatedColor;
    }

    public void Break()
    {
        pointLt.enabled = false;
        lt.enabled = false;
        this.enabled = false;
    }

    public void Fix()
    {
        pointLt.enabled = true;
        lt.enabled = true;
    }
}
