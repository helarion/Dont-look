using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkingLight : MonoBehaviour
{
    [SerializeField] Light rectangleLight = null;
    [SerializeField] Light pointLightTop = null;
    [SerializeField] Light pointLightMid = null;
    [SerializeField] Light pointLightBot = null;
    
    [SerializeField] float activeIntensity = 200.0f;
    [SerializeField] float blinkSpeed = 20.0f;
    [SerializeField] float durationBetweenBlinks = 2.0f;

    [SerializeField] Color offColor;
    [SerializeField] Color chargeColor;
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
            pointLightTop.intensity = intensity;
            pointLightMid.intensity = intensity;
            pointLightBot.intensity = intensity;
            rectangleLight.intensity = intensity;
            color = Color.Lerp(Color.black, offColor, intensity / activeIntensity);
            pointLightTop.color = color;
            pointLightMid.color = color;
            pointLightBot.color = color;
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
                intensity = 0.0f;
                color = Color.black;
                blinkLightState = BlinkLightState.BLINK;
            }
        }
        else if (blinkLightState == BlinkLightState.START_LOOK)
        {
            if (timeLooked < maxTimeLooked)
            {
                float rate = timeLooked / maxTimeLooked;
                float rateTop = Mathf.Clamp((timeLooked / maxTimeLooked) - 0.66f, 0.0f, 0.33f) * 3.0f;
                float rateMid = Mathf.Clamp((timeLooked / maxTimeLooked) - 0.33f, 0.0f, 0.33f) * 3.0f;
                float rateBot = Mathf.Clamp01(rate * 3.0f);
                //print(rate + " " + rateTop + " " + rateMid + " " + rateBot);
                rectangleLight.intensity = Mathf.Lerp(0.0f, activeIntensity, rate);
                rectangleLight.color = Color.Lerp(Color.black, chargeColor, rate);
                pointLightTop.intensity = Mathf.Lerp(0.0f, activeIntensity, rateTop);
                pointLightMid.intensity = Mathf.Lerp(0.0f, activeIntensity, rateMid);
                pointLightBot.intensity = Mathf.Lerp(0.0f, activeIntensity, rateBot);
                pointLightTop.color = Color.Lerp(Color.black, chargeColor, rateTop);
                pointLightMid.color = Color.Lerp(Color.black, chargeColor, rateMid);
                pointLightBot.color = Color.Lerp(Color.black, chargeColor, rateBot);

                timeLooked += Time.deltaTime;
            }
            else
            {
                rectangleLight.intensity = activeIntensity;
                rectangleLight.color = chargeColor;
                pointLightTop.intensity = activeIntensity;
                pointLightMid.intensity = activeIntensity;
                pointLightBot.intensity = activeIntensity;
                pointLightTop.color = chargeColor;
                pointLightMid.color = chargeColor;
                pointLightBot.color = chargeColor;
                blinkLightState = BlinkLightState.ACTIVE;
            }
        }
        else if (blinkLightState == BlinkLightState.STOP_LOOK)
        {
            if (timeLooked > 0.0f)
            {
                float rate = timeLooked / maxTimeLooked;
                float rateTop = Mathf.Clamp((timeLooked / maxTimeLooked) - 0.66f, 0.0f, 0.33f) * 3.0f;
                float rateMid = Mathf.Clamp((timeLooked / maxTimeLooked) - 0.33f, 0.0f, 0.33f) * 3.0f;
                float rateBot = Mathf.Clamp01(rate * 3.0f);
                rectangleLight.intensity = Mathf.Lerp(0.0f, activeIntensity, rate);
                rectangleLight.color = Color.Lerp(Color.black, chargeColor, rate);
                pointLightTop.intensity = Mathf.Lerp(0.0f, activeIntensity, rateTop);
                pointLightMid.intensity = Mathf.Lerp(0.0f, activeIntensity, rateMid);
                pointLightBot.intensity = Mathf.Lerp(0.0f, activeIntensity, rateBot);
                pointLightTop.color = Color.Lerp(Color.black, chargeColor, rateTop);
                pointLightMid.color = Color.Lerp(Color.black, chargeColor, rateMid);
                pointLightBot.color = Color.Lerp(Color.black, chargeColor, rateBot);

                timeLooked -= Time.deltaTime;
            }
            else
            {
                rectangleLight.intensity = 0.0f;
                rectangleLight.color = Color.black;
                pointLightTop.intensity = 0.0f;
                pointLightMid.intensity = 0.0f;
                pointLightBot.intensity = 0.0f;
                pointLightTop.color = Color.black;
                pointLightMid.color = Color.black;
                pointLightBot.color = Color.black;
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
        pointLightTop.intensity = 0.0f;
        pointLightTop.color = Color.black;
        pointLightMid.intensity = 0.0f;
        pointLightMid.color = Color.black;
        pointLightBot.intensity = 0.0f;
        pointLightBot.color = Color.black;
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
        pointLightTop.enabled = true;
        pointLightMid.enabled = true;
        pointLightBot.enabled = true;
        rectangleLight.enabled = true;
        increase = true;
        intensity = rectangleLight.intensity;
        color = rectangleLight.color;
        blinkLightState = BlinkLightState.BLINK;
    }

    public void Break()
    {
        pointLightTop.enabled = false;
        pointLightMid.enabled = false;
        pointLightBot.enabled = false;
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
        blinkLightState = BlinkLightState.ACTIVE;
    }
}
