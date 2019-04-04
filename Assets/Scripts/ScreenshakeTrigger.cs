using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshakeTrigger : MonoBehaviour
{
    [SerializeField] string triggerObjectName;
    [SerializeField] float screenshakeDuration;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == triggerObjectName)
        {
            GameManager.instance.ShakeScreen(screenshakeDuration);
        }
    }
}
