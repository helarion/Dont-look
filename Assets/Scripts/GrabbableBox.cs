using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbableBox : MonoBehaviour
{
    public bool isGrabbable = true;
    public bool isPlayerInGrabZone = false;
    PlayerController player;
    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        player = GameManager.instance.player;
        rb = transform.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, 0), Time.fixedDeltaTime);
        if (isGrabbable && isPlayerInGrabZone)
        {
            if (GameManager.instance.controls.GetButtonDown("Interact"))
            {
                print("isGrabbing");
                rb.isKinematic = false;
                rb.useGravity = true;
                player.SetIsGrabbing(true, rb, transform.GetComponent<MeshRenderer>().bounds.size.x / 1.5f);
            }
            if (GameManager.instance.controls.GetButtonUp("Interact"))
            {
                print("isNotGrabbing");
                rb.isKinematic = true;
                rb.useGravity = false;
                player.SetIsGrabbing(false, null, 0);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject collisionGameObject = collision.gameObject;
        if (isGrabbable && collisionGameObject.tag != "Ground" && collisionGameObject.GetComponent<PlayerController>() == null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            player.SetIsGrabbing(false, null, 0);
        }
    }

    public void setIsGrabbable(bool b)
    {
        isGrabbable = b;
    }

    public void setIsPlayerInGrabZone(bool b)
    {
        isPlayerInGrabZone = b;
        if (!b && isGrabbable)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            player.SetIsGrabbing(false, null, 0);
        }
    }
}
