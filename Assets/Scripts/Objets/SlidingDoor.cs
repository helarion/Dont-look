using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingDoor : Objet
{
    [SerializeField] private string doorOpenSound;
    [SerializeField] private float moveSpeed = 1;
    [SerializeField] private Transform endPos;
    [SerializeField] private float shakeOnIntensity = 0.08f;
    [SerializeField] private float shakeOffIntensity = 0.04f;

    Vector3 startPosition;
    float distance; 

    private void Start()
    {
        startPosition = transform.localPosition;
    }

    private void Update()
    {
        if (isActivating) ContiniousActivate();
        else ContiniousDesactivate();
    }

    private void ContiniousActivate()
    {
        distance = (transform.localPosition - endPos.localPosition).magnitude;
        if (distance > 0.1f)
        {
            GameManager.instance.ShakeScreen(0.5f, shakeOnIntensity);
            transform.localPosition += (transform.up) * Time.deltaTime * moveSpeed;
        }
        else transform.localPosition = endPos.localPosition;
    }

    private void ContiniousDesactivate()
    {
        distance = (transform.localPosition - startPosition).magnitude;
        if (distance > 0.1f)
        {
            GameManager.instance.ShakeScreen(0.5f, shakeOnIntensity);
            transform.localPosition += (transform.up * -1) * Time.deltaTime * moveSpeed;
        }
        else transform.localPosition = startPosition;
    }

    public override void Reset()
    {
        base.Reset();
        transform.localPosition = startPosition;
    }
}
