using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingZone : MonoBehaviour
{
    [SerializeField] GameObject grabZone;
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;
        rb.isKinematic = false;
        rb.useGravity = true;
        grabZone.SetActive(true);
        GameObject.Destroy(this);
    }

}
