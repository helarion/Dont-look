using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : Objet
{
    [SerializeField] Transform endPos;
    [SerializeField] float moveSpeed;
    [SerializeField] string activateSound;
    [SerializeField] string engineSound;
    [SerializeField] string movingSound;
    [SerializeField] string stopSound;
    [SerializeField] BoxCollider enterCol;

    private bool isMoving = false;

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

    public void StartMoving()
    {
        if (!isMoving)
        {
            isMoving = true;
            StartCoroutine("MoveCoroutine");
        }
    }

    IEnumerator MoveCoroutine()
    {
        Vector3 startPos = transform.position;
        while(transform.position.y<endPos.position.y)
        {
            transform.position = Vector3.Lerp(startPos, endPos.position, Time.deltaTime*moveSpeed);
            yield return new WaitForEndOfFrame();
        }
        //AkSoundEngine.PostEvent(stopSound, gameObject);
        isMoving = false;
        yield return null;
    }
}
