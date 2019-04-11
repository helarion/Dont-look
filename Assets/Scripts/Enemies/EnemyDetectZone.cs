using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetectZone : MonoBehaviour
{
    private Enemy e;

    private void Start()
    {
        e = GetComponentInParent<Enemy>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<PlayerController>()!=null)
        {
            e.DetectPlayer(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            e.DetectPlayer(false);
        }
    }
}
