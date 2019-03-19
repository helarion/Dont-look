using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    // a reflechir s'il y a besoin d'autres informations liés à un checkpoint ?

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;
        GameManager.instance.SetNewCheckpoint(this);
        print("New Checkpoint activated");
    }
}
