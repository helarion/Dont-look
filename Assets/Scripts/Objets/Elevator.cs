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
    [SerializeField] SpatialRoom nextSpatialRoom;
    [SerializeField] Light lamp;
    [SerializeField] Animator lampAnimator;
    [SerializeField] RandomFlicker randomFlicker;
    [SerializeField] bool scriptTwoSteps = false;
    [SerializeField] bool scriptMoveActivation = false;
    [SerializeField] bool playsActivateSound = false;
    [SerializeField] string activateSound;
    [SerializeField] string breakSound;
    private Vector3 startPos;
    private bool startActivated=false;
    [SerializeField] bool isBidirectional = true;
    [SerializeField] bool isEnd = false;
    [SerializeField] string endSong;
    [SerializeField] ContinuousBlinkingLight blinkingLight;

    [SerializeField] float waitTime = 12;
    private bool isMoving = false;
    private bool isWaiting = true;

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
            randomFlicker.enabled=false;
            enterCol.enabled = false;
        }
        //if(isActivated)AkSoundEngine.PostEvent(engineSound, gameObject);
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(waitTime);
        isWaiting = false;
        Activate();
    }

    public override void Activate()
    {
        if (isActivated) return;
        else if (isEnd && isWaiting)
        {
            StartCoroutine(Wait());
            return;
        }
        base.Activate();
        if(!isStarted)
        {
            if (playsActivateSound) AkSoundEngine.PostEvent(activateSound, gameObject);
            lamp.enabled = true;
            lampAnimator.enabled = true;
            randomFlicker.enabled = true;
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
        if (blinkingLight != null && blinkingLight.enabled)
        {
            blinkingLight.StopBlink();
            blinkingLight.enabled = false;
            lamp.enabled = true;
            lampAnimator.enabled = true;
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
        float distance = transform.position.y - endPos.position.y;
        while((direction == -1 && distance > -0.2f) || (direction == 1 && distance < 0.2f))
        {
            if (isPlayerOnBoard)
            {
                GameManager.instance.player.transform.position += (transform.up * direction) * Time.deltaTime * moveSpeed;
            }
            transform.position += (transform.up* direction) * Time.deltaTime*moveSpeed;
            distance = transform.position.y - endPos.position.y;
            yield return new WaitForEndOfFrame();
        }
        Vector3 oldPos;
        AkSoundEngine.PostEvent(stopMovingSound, gameObject);
        if (isPlayerOnBoard) GameManager.instance.player.ResumeMove();
        while (Mathf.Abs(transform.position.y - endPos.position.y) > 0.01f)
        {
            oldPos = transform.position;
            transform.position = Vector3.Lerp(transform.position, endPos.position, Time.deltaTime * moveSpeed * 2);
            if (isPlayerOnBoard)
            {
                GameManager.instance.player.transform.position += (transform.position - oldPos);
            }
            yield return new WaitForEndOfFrame();
        }
        transform.position = endPos.position;
        //GameManager.instance.player.ResumeMove();
        ReachEnd();
        yield return null;
    }

    private void ReachEnd()
    {
        isMoving = false;
        if (addSpatialLine)
        {
            spatialRoom.addSpatialLine(addedSpatialLine);
        }
        if(removeSpatialLine)
        {
            nextSpatialRoom.removeSpatialLine(removedSpatialLine);
            SpatialRoom save = spatialRoom;
            spatialRoom = nextSpatialRoom;
            nextSpatialRoom = save;
        }

        if(scriptTwoSteps && isBidirectional)
        {
            startPos = endPos.position;
            endPos.position = endPosStep2.position;
            lampAnimator.enabled = false;
            lamp.enabled = true;
            isActivated = false;
            enterCol.enabled = false;
            isStarted = false;
            AkSoundEngine.PostEvent(breakSound, gameObject);
            if (blinkingLight != null)
            {
                lampAnimator.enabled = false;
                lamp.enabled = true;
                blinkingLight.enabled = true;
            }
            scriptTwoSteps = false;
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
