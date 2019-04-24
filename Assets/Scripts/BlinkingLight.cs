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
    [SerializeField] private bool isBlinking = true;
    [SerializeField] private Color startColor;
    [SerializeField] private Color endColor;
    private Color FinishedColor;
    private float count = 0;
    private float countTime = 0;

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

    private void Update()
    {
        if(isBlinking)
        {
            if (lt.intensity >= startingIntensity)
            {
                direction = -1;
            }
            else if (lt.intensity <= 0)
            {
                direction = 1;
            }
            lt.intensity += (Time.deltaTime * blinkSpeed) * direction;
            //print(lt.intensity);
        }
        else
        {
            pointLt.intensity = Mathf.Lerp(0, maxIntensity, count);
            lt.intensity = Mathf.Lerp(startingIntensity, maxIntensity, count);
            pointLt.color = Color.Lerp(startColor, endColor, count);
            count += Time.deltaTime/countTime;
        }
    }
}
