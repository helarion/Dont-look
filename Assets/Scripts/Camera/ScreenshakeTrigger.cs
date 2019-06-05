using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshakeTrigger : MonoBehaviour
{
    [SerializeField] private GameObject triggerObject;
    [SerializeField] bool doOnce = true;
    [SerializeField] private float screenshakeDuration;
    [SerializeField] private float shakeIntensity;
    bool done = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == triggerObject && !done)
        {
            GameManager.instance.ShakeScreen(screenshakeDuration, shakeIntensity);
            if (doOnce)
            {
                done = true;
            }
        }
    }
}
