﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingingLightTrigger : MonoBehaviour
{
    [SerializeField] private GameObject triggerObject;
    [SerializeField] bool doOnce = true;
    [SerializeField] private GameObject swingingLight;
    [SerializeField] private Vector3 swingVector = Vector3.zero;
    bool done = false;
/*
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == triggerObject && !done)
        {
            swing(swingVector);
            if (doOnce)
            {
                done = true;
            }
        }
    }



    public void swingEditor()
    {
        swing(swingVector);
    }*/
}
