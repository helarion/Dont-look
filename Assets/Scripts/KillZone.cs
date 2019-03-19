using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillZone : MonoBehaviour
{
    Enemy e;

    void Start()
    {
        e = GetComponentInParent<Enemy>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;
        GameManager.instance.Death();
    }
}
