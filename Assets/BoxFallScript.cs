using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxFallScript : MonoBehaviour
{
    [SerializeField] Rigidbody boxRigidbody;
    bool touchedGround = false;
    bool kinematic = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (kinematic)
        {
            return;
        }
        if (touchedGround)
        {
            if (boxRigidbody.velocity.y == 0)
            {
                kinematic = true;
                boxRigidbody.isKinematic = true;
                boxRigidbody.useGravity = false;
                boxRigidbody.gameObject.tag = "Climbable";
                boxRigidbody.gameObject.transform.GetComponentInChildren<GrabZone>().transform.rotation = Quaternion.Euler(new Vector3 (0, 90, 0));
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody == boxRigidbody)
        {
            touchedGround = true;
        }
    }
}
