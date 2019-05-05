using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : Objet
{
    [SerializeField] Transform endPos;
    [SerializeField] float moveSpeed;
    [SerializeField] string activateSound;
    [SerializeField] string engineSound;
    [SerializeField] BoxCollider enterCol;

    public override void Activate()
    {
        if (isActivated) return;
        base.Activate();
        enterCol.enabled = true;
        //AkSoundEngine.PostEvent(activateSound, gameObject);
        //AkSoundEngine.PostEvent(engineSound, gameObject);
    }

    public override void Reset()
    {
        base.Reset();

    }
}
