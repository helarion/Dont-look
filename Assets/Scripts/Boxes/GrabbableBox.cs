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

    Vector3 vec3_modulo(Vector3 vec, int modulo)
    {
        return new Vector3(vec.x % modulo, vec.y % modulo, vec.z % modulo);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (GetComponent<HingeJoint>() == null)
        {
            Vector3 rotationOffset = vec3_modulo(transform.rotation.eulerAngles, 90);
            Vector3 rotationRounded = transform.rotation.eulerAngles - rotationOffset;
            Vector3 add90OrNot = new Vector3(rotationOffset.x > 45 ? 1 : 0, rotationOffset.y > 45 ? 1 : 0, rotationOffset.z > 45 ? 1 : 0);
            Vector3 closestGoodRotation = rotationRounded + add90OrNot * 90;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(closestGoodRotation), Time.fixedDeltaTime);
        }

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
