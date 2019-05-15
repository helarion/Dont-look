using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkingLight : MonoBehaviour
{
    [SerializeField] Light rectangleLight = null;
    [SerializeField] Light pointLightTop = null;
    [SerializeField] Light pointLightMid = null;
    [SerializeField] Light pointLightBot = null;

    [SerializeField] float offIntensity = 0.0f;
    [SerializeField] float activeIntensity = 200.0f;
    [SerializeField] float blinkSpeed = 20.0f;
    [SerializeField] float durationBetweenBlinks = 2.0f;
    [SerializeField] float backSpeed = 0.5f;

    [SerializeField] Color offColor;
    [SerializeField] Color activeColor;
    [SerializeField] bool isBroken = false;

    float timeLooked = 0.0f;
    float maxTimeLooked = 0.0f;

    bool darkWait = false;
    bool blink = false;

    private void Start()
    {
        Reset();
    }

    public void Reset()
    {
        timeLooked = 0.0f;
        pointLightTop.intensity = 0.0f;
        pointLightTop.color = offColor;
        pointLightMid.intensity = 0.0f;
        pointLightMid.color = offColor;
        pointLightBot.intensity = 0.0f;
        pointLightBot.color = offColor;
        rectangleLight.intensity = 0.0f;
        rectangleLight.color = offColor;
        if (isBroken)
        {
            Break();
        }
        else
        {
            StartCoroutine(BlinkCoroutine());
        }
    }

    public void Fix()
    {
        pointLightTop.enabled = true;
        pointLightMid.enabled = true;
        pointLightBot.enabled = true;
        rectangleLight.enabled = true;
        blink = true;
        StartCoroutine(BlinkCoroutine());
    }

    public void Break()
    {
        pointLightTop.enabled = false;
        pointLightMid.enabled = false;
        pointLightBot.enabled = false;
        rectangleLight.enabled = false;
        StopCoroutine(StartLookCoroutine());
        StopCoroutine(StopLookCoroutine());
        StopCoroutine(BlinkCoroutine());
        StopCoroutine(DarkWaitCoroutine());
    }

    public void StartLook(float time)
    {
        maxTimeLooked = time;
        blink = false;
        StopCoroutine(StopLookCoroutine());
        StopCoroutine(BlinkCoroutine());
        StopCoroutine(DarkWaitCoroutine());
        StartCoroutine(StartLookCoroutine());
    }

    public void StopLook()
    {
        StopCoroutine(StartLookCoroutine());
        StartCoroutine(StopLookCoroutine());
    }

    public void Activate()
    {
        rectangleLight.intensity = activeIntensity;
        rectangleLight.color = activeColor;
        pointLightTop.intensity = activeIntensity;
        pointLightTop.color = activeColor;
        pointLightMid.intensity = activeIntensity;
        pointLightMid.color = activeColor;
        pointLightBot.intensity = activeIntensity;
        pointLightBot.color = activeColor;
    }

    private IEnumerator StartLookCoroutine()
    {
        while (timeLooked < maxTimeLooked)
        {
            float rate = timeLooked / maxTimeLooked;
            float rateTop = Mathf.Clamp((timeLooked / maxTimeLooked) - 0.66f, 0.0f, 0.33f) * 3.0f;
            float rateMid = Mathf.Clamp((timeLooked / maxTimeLooked) - 0.33f, 0.0f, 0.33f) * 3.0f;
            float rateBot = Mathf.Clamp01(rate * 3.0f);
            rectangleLight.intensity = Mathf.Lerp(offIntensity, activeIntensity, rate);
            rectangleLight.color = Color.Lerp(offColor, activeColor, rate);
            pointLightTop.intensity = Mathf.Lerp(offIntensity, activeIntensity, rateTop);
            pointLightTop.color = Color.Lerp(offColor, activeColor, rateTop);
            pointLightMid.intensity = Mathf.Lerp(offIntensity, activeIntensity, rateMid);
            pointLightMid.color = Color.Lerp(offColor, activeColor, rateMid);
            pointLightBot.intensity = Mathf.Lerp(offIntensity, activeIntensity, rateBot);
            pointLightBot.color = Color.Lerp(offColor, activeColor, rateBot);

            timeLooked += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        rectangleLight.intensity = activeIntensity;
        rectangleLight.color = activeColor;
        pointLightTop.intensity = activeIntensity;
        pointLightTop.color = activeColor;
        pointLightMid.intensity = activeIntensity;
        pointLightMid.color = activeColor;
        pointLightBot.intensity = activeIntensity;
        pointLightBot.color = activeColor;
        yield return null;
    }

    private IEnumerator StopLookCoroutine()
    {
        while (timeLooked > 0.0f)
        {
            float rate = timeLooked / maxTimeLooked;
            float rateTop = Mathf.Clamp((timeLooked / maxTimeLooked) - 0.66f, 0.0f, 0.33f) * 3.0f;
            float rateMid = Mathf.Clamp((timeLooked / maxTimeLooked) - 0.33f, 0.0f, 0.33f) * 3.0f;
            float rateBot = Mathf.Clamp01(rate * 3.0f);
            rectangleLight.intensity = Mathf.Lerp(offIntensity, activeIntensity, rate);
            rectangleLight.color = Color.Lerp(offColor, activeColor, rate);
            pointLightTop.intensity = Mathf.Lerp(offIntensity, activeIntensity, rateTop);
            pointLightTop.color = Color.Lerp(offColor, activeColor, rateTop);
            pointLightMid.intensity = Mathf.Lerp(offIntensity, activeIntensity, rateMid);
            pointLightMid.color = Color.Lerp(offColor, activeColor, rateMid);
            pointLightBot.intensity = Mathf.Lerp(offIntensity, activeIntensity, rateBot);
            pointLightBot.color = Color.Lerp(offColor, activeColor, rateBot);

            timeLooked -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        rectangleLight.intensity = 0.0f;
        rectangleLight.color = offColor;
        pointLightTop.intensity = 0.0f;
        pointLightTop.color = offColor;
        pointLightMid.intensity = 0.0f;
        pointLightMid.color = offColor;
        pointLightBot.intensity = 0.0f;
        pointLightBot.color = offColor;
        StartCoroutine(BlinkCoroutine());
        yield return null;
    }

    private IEnumerator BlinkCoroutine()
    {
        blink = true;
        float intensity = 0.0f;
        bool increase = true;
        while (blink)
        {
            if (darkWait)
            {
                yield return new WaitForEndOfFrame();
            }
            else
            {
                pointLightTop.intensity = intensity;
                pointLightMid.intensity = intensity;
                pointLightBot.intensity = intensity;
                rectangleLight.intensity = intensity;
                print(intensity);
                if (increase)
                {
                    intensity += (Time.deltaTime * offIntensity) / blinkSpeed;
                    if (intensity >= offIntensity)
                    {
                        intensity = offIntensity;
                        increase = false;
                    }
                }
                else
                {
                    intensity -= (Time.deltaTime * offIntensity) / blinkSpeed;
                    if (intensity <= 0.0f)
                    {
                        intensity = 0.0f;
                        increase = true;
                        StartCoroutine(DarkWaitCoroutine());
                    }
                }
                yield return new WaitForEndOfFrame();
            }
        }
    }

    private IEnumerator DarkWaitCoroutine()
    {
        darkWait = true;
        float darkWaitTime = 0.0f;
        while (darkWaitTime < durationBetweenBlinks)
        {
            darkWaitTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        darkWait = false;
        yield return null;
    }
}
