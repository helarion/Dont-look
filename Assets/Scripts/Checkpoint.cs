﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    // a stocker plus tard : les puzzles déja faits pour ne pas les réinitaliser, ou l'inverse : les puzzles à réini
    float z;

    private void Start()
    {
        z = GameManager.instance.mainCamera.transform.position.z+2;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;
        GameManager.instance.SetNewCheckpoint(this);
    }

    [ExecuteInEditMode]
    private void OnDrawGizmos()
    {
        BoxCollider bCollider = GetComponent<BoxCollider>();
        Vector3 pos = transform.position + bCollider.center;
        Gizmos.color = Color.yellow;
        float wHalf = (bCollider.size.x * .5f);
        float hHalf = (bCollider.size.y * .5f);
        Vector3 topLeftCorner = new Vector3(pos.x - wHalf, pos.y + hHalf, z);
        Vector3 topRightCorner = new Vector3(pos.x + wHalf, pos.y + hHalf, z);
        Vector3 bottomLeftCorner = new Vector3(pos.x - wHalf, pos.y - hHalf, z);
        Vector3 bottomRightCorner = new Vector3(pos.x + wHalf, pos.y - hHalf, z);
        Gizmos.DrawLine(topLeftCorner, topRightCorner);
        Gizmos.DrawLine(topRightCorner, bottomRightCorner);
        Gizmos.DrawLine(bottomRightCorner, bottomLeftCorner);
        Gizmos.DrawLine(bottomLeftCorner, topLeftCorner);
    }
}
