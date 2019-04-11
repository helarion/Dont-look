using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingBox : MonoBehaviour
{
    Rigidbody rb;
    public bool touchedGround = false;
    public bool kinematic = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = transform.GetComponent<Rigidbody>();
        transform.GetComponent<GrabbableBox>().setIsGrabbable(false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (kinematic)
        {
            return;
        }
        if (touchedGround)
        {
            print(rb.velocity.y);
            if (Mathf.Abs(rb.velocity.y) < 0.0001f)
            {
                print("SALU");
                kinematic = true;
                rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                rb.mass = 500;
                rb.isKinematic = true;
                rb.useGravity = false;
                transform.tag = "Climbable";
                transform.GetComponentInChildren<GrabbableBox>().setIsGrabbable(true);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            touchedGround = true;
        }
    }
}
