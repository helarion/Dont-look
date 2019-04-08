using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : Objet
{
    [SerializeField] private GameObject door =null;

    public override void Activate()
    {
        door.transform.eulerAngles = new Vector3(0, 90, 0);
        // PLAY SOUND
    }
}
