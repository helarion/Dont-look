using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderClimb : MonoBehaviour
{
    BoxCollider col;
    float maxHeight;

    private void Start()
    {
        col = GetComponent<BoxCollider>();
        maxHeight = col.bounds.max.y;
    }

    private void OnTriggerStay(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;
        if(GameManager.instance.controls.GetButtonDown("Interact"))
        {
            Vector3 v = player.transform.position;
            v.x= transform.position.x;
            player.transform.position = v;
            player.SetIsClimbing(true);
        }
        if(GameManager.instance.controls.GetButtonDown("Jump") && player.GetIsClimbing())
        {
            player.SetIsClimbing(false);
            player.SetHasReachedTop(false);
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
        player.SetIsClimbing(false);
    }
}
