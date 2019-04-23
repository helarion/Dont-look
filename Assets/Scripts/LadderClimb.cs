using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderClimb : MonoBehaviour
{
    private BoxCollider col;
    private float maxHeight;
    private float minHeight;
    private PlayerController player =null;
    [SerializeField] bool isReusable = false;
    bool isUsed = false;
    bool downStart = false;

    private void Start()
    {
        col = GetComponent<BoxCollider>();
        maxHeight = col.bounds.max.y - 2;
        minHeight = col.bounds.min.y;
        if (GameManager.instance.player.transform.position.y < transform.position.y) downStart = true; 
    }

    private void OnTriggerEnter(Collider other)
    {
        player = other.GetComponent<PlayerController>();
        if (player == null) return;
        StartClimb();
    }

    private void OnTriggerStay(Collider other)
    {
        player = other.GetComponent<PlayerController>();
        if (player == null) return;

        if (player.transform.position.y > maxHeight) player.SetHasReachedTop(true);
        else player.SetHasReachedTop(false);

        if (player.transform.position.y < minHeight) player.SetHasReachedBottom(true);
        else player.SetHasReachedBottom(false);
    }

    /*private void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;
        StopClimb();
    }*/

    private void StartClimb()
    {
        Vector3 v = player.transform.position;
        v.y += 0.2f;
        v.x = transform.position.x;
        v.z = transform.position.z - 1;
        player.StartClimbLadder(v);
    }
}
