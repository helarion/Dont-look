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
    [SerializeField] int direction = 1;
    [SerializeField] bool isStarted = false;
    [HideInInspector] public bool isPlayerOnBoard = false;
    [SerializeField] private bool addSpatialLine = false;
    [SerializeField] SpatialLine spatialLine;
    [SerializeField] SpatialRoom spatialRoom;
    private Vector3 startPos;
    //private Rigidbody rb;

    private bool isMoving = false;

    private void Start()
    {
        startPos = transform.position;
        //rb = GetComponent<Rigidbody>();
    }

    public override void Activate()
    {
        if (isActivated) return;
        base.Activate();
        if(!isStarted)
        {
            enterCol.enabled = true;
            isMoving = false;
        }
        else
        {
            StartMoving();
        }
        //AkSoundEngine.PostEvent(activateSound, gameObject);
        //AkSoundEngine.PostEvent(engineSound, gameObject);
    }

    public override void Reset()
    {
        base.Reset();
        StopCoroutine(MoveCoroutine());
        enterCol.enabled = false;
        transform.position = startPos;
    }

    public void StartMoving()
    {
        if (!isMoving)
        {
            isMoving = true;
            StartCoroutine(MoveCoroutine());
        }
    }

    IEnumerator MoveCoroutine()
    {
        if(isPlayerOnBoard) GameManager.instance.player.StopMove();
        float distance = (transform.position - endPos.position).magnitude;
        while(distance>0.1f)
        {
            transform.position += (transform.up* direction) * Time.deltaTime*moveSpeed;
            distance = (transform.position - endPos.position).magnitude;
            yield return new WaitForEndOfFrame();
        }
        transform.position = endPos.position;
        //AkSoundEngine.PostEvent(stopSound, gameObject);
        //GameManager.instance.player.ResumeMove();
        ReachEnd();
        yield return null;
    }

    private void ReachEnd()
    {
        if(isPlayerOnBoard) GameManager.instance.player.ResumeMove();
        isMoving = false;
        if (addSpatialLine)
        {
            Vector3 save = startPos;
            startPos = endPos.position;
            endPos.position = save;
            isActivated = false;
            direction *= -1;
            spatialRoom.addSpatialLine(spatialLine);
        }
    }
}
