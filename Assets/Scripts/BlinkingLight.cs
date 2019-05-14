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
    private bool isBlinking = false;
    private bool isActivated = false;
    private bool isLooked = false;

    private void Start()
    {
        pointLt.intensity = 0;
        pointLt.color = startColor;
        if (isBroken)
        {
            Break();
        }
        StartBlink();
    }

    public void StartBlink()
    {
        if (isBlinking) return;
        isBlinking = true;
        count = 0;
        StartCoroutine(Blink());
    }

    public void StartLook(float time)
    {
        if (isLooked) return;
        isLooked = true;
        countTime = time;
        isBlinking = false;
        StopCoroutine(Blink());
        StopCoroutine(StopLook());
        StartCoroutine(StartLook());
    }

    public void StopLook(float time)
    {
        if (!isLooked) return;
        countTime = time;
        isLooked = false;
        isBlinking = false;
        StopCoroutine(StartLook());
        StartCoroutine(StopLook());
    }

    private IEnumerator StopLook()
    {
        while (count> 0)
        {
            pointLt.color = Color.Lerp(pointLt.color, startColor, count);
            pointLt.intensity = Mathf.Lerp(pointLt.intensity, 0, count);
            lt.intensity = Mathf.Lerp(lt.intensity, 0, count);
            lt.color = Color.Lerp(pointLt.color, startColor, count);

            count -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        count = 0;
        pointLt.color = startColor;
        pointLt.intensity = 0;
        lt.color = startColor;
        lt.intensity = 0;
        StartBlink();
        yield return null;
    }

    private IEnumerator StartLook()
    {
        Color newColor;
        if (isContinious) newColor = activatedColor;
        else newColor = endColor;
        while (count < countTime)
        {
            lt.color = Color.Lerp(pointLt.color, newColor, countTime);
            pointLt.color = Color.Lerp(pointLt.color, newColor, countTime);

            pointLt.intensity = Mathf.Lerp(pointLt.intensity, maxIntensity, count);
            lt.intensity = Mathf.Lerp(lt.intensity, maxIntensity, count);
            count += Time.deltaTime / countTime;
            yield return new WaitForEndOfFrame();
        }
        count = countTime;
        pointLt.color = newColor;
        pointLt.intensity = maxIntensity;
        lt.color = newColor;
        lt.intensity = maxIntensity;
        yield return null;
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

    private IEnumerator Blink()
    {
        while(isBlinking)
        {
            if (pointLt.intensity > 0)
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
            else if (lt.intensity <= 0 && !isWaiting)
            {
                direction = 1;
                StartCoroutine(DarkWait());
            }
            if (!isWaiting) lt.intensity += (Time.deltaTime * blinkSpeed) * direction;
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    public void Activate()
    {
        if (isContinious) return;
        isActivated = true;
        pointLt.color = activatedColor;
        lt.color = activatedColor;
        pointLt.intensity = maxIntensity;
        lt.intensity = maxIntensity;
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
