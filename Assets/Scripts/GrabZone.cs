using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabZone : MonoBehaviour
{
    [SerializeField] GrabbableBox grabbableBox;

    private void FixedUpdate()
    {
        transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            grabbableBox.setIsPlayerInGrabZone(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            grabbableBox.setIsPlayerInGrabZone(false);
        }
    }
}
