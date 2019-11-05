using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopChaseTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() == null) return;
        GameManager.instance.RespawnEnemies();
    }
}
