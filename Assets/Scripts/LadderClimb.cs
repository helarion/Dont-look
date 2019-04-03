using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderClimb : MonoBehaviour
{
    BoxCollider col;
    float maxHeight;
    PlayerController player =null;

    private void Start()
    {
        col = GetComponent<BoxCollider>();
        maxHeight = col.bounds.max.y;
    }

    private void OnTriggerStay(Collider other)
    {
        player = other.GetComponent<PlayerController>();
        if (player == null) return;
        if(GameManager.instance.controls.GetButtonDown("Interact"))
        {
            StartClimb();
        }
        if(GameManager.instance.controls.GetButtonDown("Jump") && player.GetIsClimbing())
        {
            StopClimb();
            player.Jump();
        }
        if(player.transform.position.y>maxHeight)
        {
            player.SetHasReachedTop(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;
        StopClimb();
    }

    void StartClimb()
    {
        Vector3 v = player.transform.position;
        v.x = transform.position.x;
        player.transform.position = v;
        player.transform.eulerAngles += new Vector3(0, -90, 0);
        player.SetIsClimbing(true);
    }

    void StopClimb()
    {
        if (!player.GetIsClimbing()) return;
        player.transform.eulerAngles += new Vector3(0, 90, 0);
        player.SetIsClimbing(false);
        player.SetHasReachedTop(false);
    }
}
