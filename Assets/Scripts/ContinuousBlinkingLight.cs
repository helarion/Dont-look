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

    [SerializeField] Color offColor;
    [SerializeField] Color activeColor;
    [SerializeField] bool isBroken = false;

    float timeLooked = 0.0f;
    float maxTimeLooked = 0.0f;

    float darkWaitTime = 0.0f;

    bool increase = false;
    float intensity = 0.0f;
    Color color = Color.black;

    enum BlinkLightState
    {
        BLINK,
        DARK_WAIT,
        START_LOOK,
        STOP_LOOK,
        ACTIVE,
        BROKEN
    }

    BlinkLightState blinkLightState = BlinkLightState.BLINK;

    private void Start()
    {
        Reset();
    }

    private void Update()
    {
        if (blinkLightState == BlinkLightState.BLINK)
        {
            pointLight.intensity = intensity;
            rectangleLight.intensity = intensity;
            color = Color.Lerp(Color.black, offColor, intensity / activeIntensity);
            pointLight.color = color;
            rectangleLight.color = color;
            
            if (increase)
            {
                intensity += (Time.deltaTime * activeIntensity) / blinkSpeed;
                if (intensity >= activeIntensity)
                {
                    intensity = activeIntensity;
                    increase = false;
                }
            }
            else
            {
                intensity -= (Time.deltaTime * activeIntensity) / blinkSpeed;
                if (intensity <= 0.0f)
                {
                    intensity = 0.0f;
                    increase = true;
                    darkWaitTime = 0.0f;
                    blinkLightState = BlinkLightState.DARK_WAIT;
                }
            }
        }
        else if (blinkLightState == BlinkLightState.DARK_WAIT)
        {
            if (darkWaitTime < durationBetweenBlinks)
            {
                darkWaitTime += Time.deltaTime;
            }
            else
            {
                increase = true;
                intensity = rectangleLight.intensity;
                color = rectangleLight.color;
                blinkLightState = BlinkLightState.BLINK;
            }
        }
        else if (blinkLightState == BlinkLightState.START_LOOK)
        {
            if (timeLooked < maxTimeLooked)
            {
                float rate = timeLooked / maxTimeLooked;
                rectangleLight.intensity = Mathf.Lerp(offIntensity, activeIntensity, rate);
                rectangleLight.color = Color.Lerp(Color.black, activeColor, rate);
                pointLight.intensity = Mathf.Lerp(offIntensity, activeIntensity, rate);
                pointLight.color = Color.Lerp(Color.black, activeColor, rate);

                timeLooked += Time.deltaTime;
            }
            else
            {
                rectangleLight.intensity = activeIntensity;
                rectangleLight.color = activeColor;
                pointLight.intensity = activeIntensity;
                pointLight.color = activeColor;
                blinkLightState = BlinkLightState.ACTIVE;
            }
        }
        else if (blinkLightState == BlinkLightState.STOP_LOOK)
        {
            if (timeLooked > 0.0f)
            {
                float rate = timeLooked / maxTimeLooked;
                rectangleLight.intensity = Mathf.Lerp(offIntensity, activeIntensity, rate);
                rectangleLight.color = Color.Lerp(Color.black, activeColor, rate);
                pointLight.intensity = Mathf.Lerp(offIntensity, activeIntensity, rate);
                pointLight.color = Color.Lerp(Color.black, activeColor, rate);

                timeLooked -= Time.deltaTime;
            }
            else
            {
                rectangleLight.intensity = offIntensity;
                rectangleLight.color = Color.black;
                pointLight.intensity = offIntensity;
                pointLight.color = Color.black;
                darkWaitTime = 0.0f;
                blinkLightState = BlinkLightState.DARK_WAIT;
            }
        }
        else if (blinkLightState == BlinkLightState.ACTIVE)
        {
            ;
        }
        else if (blinkLightState == BlinkLightState.BROKEN)
        {
            ;
        }
    }

    public void Reset()
    {
        timeLooked = 0.0f;
        pointLight.intensity = 0.0f;
        pointLight.color = Color.black;
        rectangleLight.intensity = 0.0f;
        rectangleLight.color = Color.black;
        if (isBroken)
        {
            Break();
        }
        else
        {
            increase = true;
            intensity = rectangleLight.intensity;
            color = rectangleLight.color;
            blinkLightState = BlinkLightState.BLINK;
        }
    }

    public void Fix()
    {
        pointLight.enabled = true;
        rectangleLight.enabled = true;
        increase = true;
        intensity = rectangleLight.intensity;
        color = rectangleLight.color;
        blinkLightState = BlinkLightState.BLINK;
    }

    public void Break()
    {
        pointLight.enabled = false;
        rectangleLight.enabled = false;
        blinkLightState = BlinkLightState.BROKEN;
    }

    public void StartLook(float time)
    {
        maxTimeLooked = time;
        blinkLightState = BlinkLightState.START_LOOK;
    }

    public void StopLook()
    {
        blinkLightState = BlinkLightState.STOP_LOOK;
    }
}
