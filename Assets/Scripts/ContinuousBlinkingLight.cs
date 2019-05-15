using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousBlinkingLight : MonoBehaviour
{
    [SerializeField] Light rectangleLight = null;
    [SerializeField] Light pointLight = null;

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
        pointLight.intensity = 0.0f;
        pointLight.color = offColor;
        rectangleLight.intensity = 0.0f;
        rectangleLight.color = offColor;
        if (isBroken)
        {
            Break();
        }
        else
        {
            StartCoroutine(BlinkCoroutine());
            print("start blink");
        }
    }

    public void Fix()
    {
        pointLight.enabled = true;
        rectangleLight.enabled = true;
        blink = true;
        StartCoroutine(BlinkCoroutine());
        print("start blink");
    }

    public void Break()
    {
        print("break");
        pointLight.enabled = false;
        rectangleLight.enabled = false;
        StopCoroutine(StartLookCoroutine());
        StopCoroutine(StopLookCoroutine());
        blink = false;
        StopCoroutine(BlinkCoroutine());
        print("stop blink");
        StopCoroutine(DarkWaitCoroutine());
    }

    public void StartLook(float time)
    {
        print("start");
        maxTimeLooked = time;
        StopCoroutine(StopLookCoroutine());
        blink = false;
        StopCoroutine(BlinkCoroutine());
        print("stop blink");
        StopCoroutine(DarkWaitCoroutine());
        StartCoroutine(StartLookCoroutine());
    }

    public void StopLook()
    {
        print("stop");
        StopCoroutine(StartLookCoroutine());
        StartCoroutine(StopLookCoroutine());
    }

    private IEnumerator StartLookCoroutine()
    {
        while (timeLooked < maxTimeLooked)
        {
            float rate = timeLooked / maxTimeLooked;
            rectangleLight.intensity = Mathf.Lerp(offIntensity, activeIntensity, rate);
            rectangleLight.color = Color.Lerp(offColor, activeColor, rate);
            pointLight.intensity = Mathf.Lerp(offIntensity, activeIntensity, rate);
            pointLight.color = Color.Lerp(offColor, activeColor, rate);

            timeLooked += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        rectangleLight.intensity = activeIntensity;
        rectangleLight.color = activeColor;
        pointLight.intensity = activeIntensity;
        pointLight.color = activeColor;
        yield return null;
    }

    private IEnumerator StopLookCoroutine()
    {
        while (timeLooked > 0.0f)
        {
            float rate = timeLooked / maxTimeLooked;
            rectangleLight.intensity = Mathf.Lerp(offIntensity, activeIntensity, rate);
            rectangleLight.color = Color.Lerp(offColor, activeColor, rate);
            pointLight.intensity = Mathf.Lerp(offIntensity, activeIntensity, rate);
            pointLight.color = Color.Lerp(offColor, activeColor, rate);

            timeLooked -= Time.deltaTime;
            print("isstoppin");
            yield return new WaitForEndOfFrame();
        }
        rectangleLight.intensity = 0.0f;
        rectangleLight.color = offColor;
        pointLight.intensity = 0.0f;
        pointLight.color = offColor;
        StartCoroutine(BlinkCoroutine());
        yield return null;
    }

    private IEnumerator BlinkCoroutine()
    {
        float intensity = 0.0f;
        blink = true;
        bool increase = true;
        while (blink)
        {
            //print("je blink");
            if (darkWait)
            {
                //print("mais dark wait");
            }
            else
            {
                //print("et ça tourne");
                pointLight.intensity = intensity;
                rectangleLight.intensity = intensity;

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
            }
            yield return new WaitForEndOfFrame();
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
