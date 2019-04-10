using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabZone : MonoBehaviour
{
    public bool isGrabbable = true;

    private void OnTriggerStay(Collider other)
    {
        if (isGrabbable)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null) return;
            if (GameManager.instance.controls.GetButtonDown("Interact"))
            {
                print("isGrabbing");
                player.SetIsGrabbing(true, transform.parent);
            }
            if (GameManager.instance.controls.GetButtonUp("Interact"))
            {
                print("isNotGrabbing");
                player.SetIsGrabbing(false, null);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;
        player.SetIsGrabbing(false, null);
    }
}
