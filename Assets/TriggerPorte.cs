using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerPorte : MonoBehaviour
{
    [SerializeField] SlidingDoor door;
    [SerializeField] BipedeBehavior bipede;
    [SerializeField] float newSpeed=2;

    public void SetSpeed()
    {
        door.SetNewCloseSpeed(newSpeed);
        bipede.isInCorridor = true;
    }
}
