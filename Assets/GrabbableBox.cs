using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbableBox : MonoBehaviour
{
    bool isGrabbable = true;
    bool isPlayerInGrabZone = false;
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
        if (collisionGameObject.tag != "Ground" && collisionGameObject.GetComponent<PlayerController>() == null)
        {
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
        if (!b)
        {
            player.SetIsGrabbing(false, null, 0);
        }
    }
}
