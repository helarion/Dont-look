using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightDetectZone : MonoBehaviour
{
    SphereCollider sCollider;

    private void Start()
    {
        sCollider = GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Enemy e = other.GetComponent<Enemy>();
        if (e!=null)
        {
            e.isLit(true);
            print("looking at enemy");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Enemy e = other.GetComponent<Enemy>();
        if (e != null)
        {
            e.isLit(false);
            print("stopped looking at enemy");
        }
    }
}
