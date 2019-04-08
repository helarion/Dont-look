using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshakeTrigger : MonoBehaviour
{
    [SerializeField] private string triggerObjectName;
    [SerializeField] private float screenshakeDuration;

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == triggerObjectName)
        {
            GameManager.instance.ShakeScreen(screenshakeDuration);
        }
    }
}
