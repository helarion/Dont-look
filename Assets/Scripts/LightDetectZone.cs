using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightDetectZone : MonoBehaviour
{
    SphereCollider collider;

    private void Start()
    {
        collider = GetComponent<SphereCollider>();
    }

    private void Update()
    {
        
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
