using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkingLight : MonoBehaviour
{
    [SerializeField] Light lt=null;
    [SerializeField] Light pointLt = null;
    [SerializeField] float startingIntensity;
    [SerializeField] private int direction=1;
    [SerializeField] private float blinkSpeed = 20;
    [SerializeField] private float maxIntensity = 200;
    [SerializeField] private float wait = 2;
    [SerializeField] private Color startColor;
    [SerializeField] private Color endColor;
    [SerializeField] private Color activatedColor;
    private Color FinishedColor;
    private float count = 0;
    private float countTime = 0;
    private bool isWaiting = false;
    private bool isBlinking = true;
    private bool isActivated = false;

    public void StartBlink()
    {
        isBlinking = true;
        lt.intensity = 0;
        lt.color = startColor;
        pointLt.intensity = 0;
        pointLt.color = startColor;
    }

    public void StartLook(float time)
    {
        count = 0;
        countTime = time;
        lt.intensity = startingIntensity;
        isBlinking = false;
    }

    public void Reset()
    {
        count = 0;
        pointLt.intensity = 0;
        lt.intensity = startingIntensity;
        pointLt.color = startColor;
        isWaiting = false;
        isBlinking = true;
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
            if (lt.intensity >= startingIntensity)
            {
                direction = -1;
            }
            else if (lt.intensity <= 0 &&!isWaiting)
            {
                direction = 1;
                StartCoroutine("DarkWait");
            }
            if(!isWaiting)lt.intensity += (Time.deltaTime * blinkSpeed) * direction;
        }
        else if(!isActivated)
        {
            pointLt.intensity = Mathf.Lerp(0, maxIntensity, count);
            lt.intensity = Mathf.Lerp(startingIntensity, maxIntensity, count);
            pointLt.color = Color.Lerp(startColor, endColor, count);
            count += Time.deltaTime/countTime;
        }
    }

    public void Activate()
    {
        isActivated = true;
        pointLt.color = activatedColor;
    }
}
