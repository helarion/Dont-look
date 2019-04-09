using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillZone : MonoBehaviour
{
    private Enemy e;

    private void Start()
    {
        e = GetComponentInParent<Enemy>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null || !e.isChasing) return;
        GameManager.instance.Death();
    }
}
