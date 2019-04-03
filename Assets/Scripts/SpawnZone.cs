using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnZone : MonoBehaviour
{
    [ExecuteInEditMode]
    private void OnDrawGizmos()
    {
        Vector3 pos = transform.position;
        Gizmos.color = Color.red;
        float wHalf = (0.5f);
        float hHalf = (0.5f);
        Vector3 topLeftCorner = new Vector3(pos.x - wHalf, pos.y + hHalf, pos.z);
        Vector3 topRightCorner = new Vector3(pos.x + wHalf, pos.y + hHalf, pos.z);
        Vector3 bottomLeftCorner = new Vector3(pos.x - wHalf, pos.y - hHalf, pos.z);
        Vector3 bottomRightCorner = new Vector3(pos.x + wHalf, pos.y - hHalf, pos.z);
        Gizmos.DrawLine(topLeftCorner, topRightCorner);
        Gizmos.DrawLine(topRightCorner, bottomRightCorner);
        Gizmos.DrawLine(bottomRightCorner, bottomLeftCorner);
        Gizmos.DrawLine(bottomLeftCorner, topLeftCorner);
    }
}
