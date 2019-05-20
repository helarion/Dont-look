using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    List<Collider> colliders = new List<Collider>();
    bool isGrounded = false;
    string surface = "";

    // Update is called once per frame
    void Update()
    {
        if (colliders.Count != 0)
        {
            isGrounded = true;
            surface=colliders[0].tag;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger)
        {
            //print("Ground detector collided with :" + other.name);
            colliders.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.isTrigger)
        {
            colliders.Remove(other);
        }
    }

    public string GetSurface()
    {
        return surface;
    }

    public bool GetIsGrounded()
    {
        return isGrounded;
    }
}
