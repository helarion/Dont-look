using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WwiseTrigger : MonoBehaviour
{
    private float z;
    [SerializeField] GameObject triggerObject;
    [SerializeField] private AK.Wwise.Event triggerEvent;
    [SerializeField] bool doOnce = true;
    [SerializeField] private GameObject emitter = null;
    bool done = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == triggerObject && !done)
        {
            AkSoundEngine.PostEvent(triggerEvent.Id, emitter);
            if (doOnce)
            {
                done = true;
            }
        }
    }

    [ExecuteInEditMode]
    private void OnDrawGizmos()
    {
        z = 6;
        BoxCollider bCollider = GetComponent<BoxCollider>();
        Vector3 pos = transform.position + bCollider.center;
        Gizmos.color = Color.green;
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
