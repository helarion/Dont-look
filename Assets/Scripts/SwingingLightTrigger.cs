using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingingLightTrigger : MonoBehaviour
{
    [SerializeField] private string triggerObjectName;
    [SerializeField] private GameObject swingingLight;
    [SerializeField] private Vector3 swingVector = Vector3.zero;

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == triggerObjectName)
        {
            swing(swingVector);
        }
    }

    public void swing(Vector3 swingVelocity)
    {
        swingingLight.GetComponent<Rigidbody>().AddForce(swingVelocity);
    }

    public void swingEditor()
    {
        swingingLight.GetComponent<Rigidbody>().AddForce(swingVector);
    }
}
