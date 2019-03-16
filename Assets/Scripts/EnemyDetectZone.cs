using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetectZone : MonoBehaviour
{
    [SerializeField] Enemy e=null;

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
