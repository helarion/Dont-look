using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingingLightTrigger : MonoBehaviour
{
    [SerializeField] string triggerObjectName;
    [SerializeField] GameObject swingingLight;
    [SerializeField] Vector3 swingVector = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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
