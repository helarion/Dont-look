using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectZone : MonoBehaviour
{
    [SerializeField] SpiderBehavior spider=null;

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<PlayerController>()!=null)
        {
            spider.DetectPlayer(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            spider.DetectPlayer(false);
        }
    }
}
