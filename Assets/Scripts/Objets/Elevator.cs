﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : Objet
{
    [SerializeField] Transform endPos;
    [SerializeField] float moveSpeed;
    [SerializeField] string playEngineSound;
    [SerializeField] string stopEngineSound;
    [SerializeField] string playMovingSound;
    [SerializeField] string stopMovingSound;
    [SerializeField] BoxCollider enterCol;
    [SerializeField] int direction = 1;
    [SerializeField] bool isStarted = false;
    [HideInInspector] public bool isPlayerOnBoard = false;
    [SerializeField] private bool addSpatialLine = false;
    [SerializeField] SpatialLine spatialLine;
    [SerializeField] SpatialRoom spatialRoom;
    [SerializeField] Light lamp;
    [SerializeField] Animator lampAnimator;
    private Vector3 startPos;

    private bool isMoving = false;

    private void Start()
    {
        startPos = transform.position;
        if(!isStarted)
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
        }
        else
        {
            StartMoving();
        }
        //AkSoundEngine.PostEvent(playEngineSound, gameObject);
    }

    public override void Reset()
    {
        base.Reset();
        if (!isStarted)
        {
            lamp.enabled = false;
            lampAnimator.enabled = false;
            enterCol.enabled = false;
        }
        StopCoroutine(MoveCoroutine());
        transform.position = startPos;
        isActivated = isStarted;
        //if(!isActivated) //AkSoundEngine.PostEvent(stopEngineSound, gameObject);
    }

    public void StartMoving()
    {
        if (!isMoving)
        {
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
            Vector3 save = startPos;
            startPos = endPos.position;
            endPos.position = save;
            isActivated = false;
            direction *= -1;
            spatialRoom.addSpatialLine(spatialLine);
        }
        else if(isStarted)
        {
            Vector3 save = startPos;
            startPos = endPos.position;
            endPos.position = save;
            isActivated = false;
            direction *= -1;
        }
    }
}
