using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : Objet
{
    [SerializeField] Transform endPos;
    [SerializeField] Transform endPosStep2;
    [SerializeField] float moveSpeed;
    [SerializeField] string playEngineSound;
    [SerializeField] string stopEngineSound;
    [SerializeField] string playMovingSound;
    [SerializeField] string stopMovingSound;
    [SerializeField] BoxCollider enterCol;
    [SerializeField] int direction = 1;
    bool isStarted = false;
    [HideInInspector] public bool isPlayerOnBoard = false;
    [SerializeField] private bool addSpatialLine = false;
    [SerializeField] private bool removeSpatialLine = false;
    [SerializeField] SpatialLine addedSpatialLine;
    [SerializeField] SpatialLine removedSpatialLine;
    [SerializeField] SpatialRoom spatialRoom;
    [SerializeField] Light lamp;
    [SerializeField] Animator lampAnimator;
    [SerializeField] bool scriptTwoSteps = false;
    [SerializeField] bool scriptMoveActivation = false;
    private Vector3 startPos;
    private bool startActivated=false;
    [SerializeField] bool isBidirectional = true;
    [SerializeField] bool isEnd = false;
    [SerializeField] string endSong;


    private bool isMoving = false;

    private void Start()
    {
        startActivated = isActivated;
        startPos = transform.position;
        if (isActivated)
        {
            isStarted = true;
        }
        if (!isStarted)
        {
            lamp.enabled = false;
            lampAnimator.enabled = false;
            enterCol.enabled = false;
        }
        //if(isActivated)AkSoundEngine.PostEvent(engineSound, gameObject);
        //rb = GetComponent<Rigidbody>();
    }

    public override void Activate()
    {
        if (isActivated) return;
        base.Activate();
        if(!isStarted)
        {
            lamp.enabled = true;
            lampAnimator.enabled = true;
            enterCol.enabled = true;
            isMoving = false;
            isStarted = true;
        }
        else
        {
            StartMoving();
        }
        if (scriptMoveActivation)
        {
            StartMoving();
            startActivated = true;
        }
        //AkSoundEngine.PostEvent(playEngineSound, gameObject);
    }

    public override void Reset()
    {
        base.Reset();
        if (!startActivated)
        {
            isStarted = false;
            lamp.enabled = false;
            lampAnimator.enabled = false;
            enterCol.enabled = false;
        }
        StopCoroutine(MoveCoroutine());
        isMoving = false;
        transform.position = startPos;
        isActivated = isStarted;
        //if(!isActivated) //AkSoundEngine.PostEvent(stopEngineSound, gameObject);
    }

    public void StartMoving()
    {
        if (!isMoving)
        {
            if(isEnd) AkSoundEngine.PostEvent(endSong, gameObject);
            AkSoundEngine.PostEvent(playMovingSound, gameObject);
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
        AkSoundEngine.PostEvent(stopMovingSound, gameObject);
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
            spatialRoom.addSpatialLine(addedSpatialLine);
        }
        if(removeSpatialLine)
        {
            //spatialRoom.
        }

        if(scriptTwoSteps && isBidirectional)
        {
            startPos = endPos.position;
            endPos.position = endPosStep2.position;
            lamp.enabled = false;
            lampAnimator.enabled = false;
            isActivated = false;
            enterCol.enabled = false;
            isStarted = false;
        }
        else if(isStarted && isBidirectional)
        {
            Vector3 save = startPos;
            startPos = endPos.position;
            endPos.position = save;
            isActivated = false;
            direction *= -1;
        }
    }
}
