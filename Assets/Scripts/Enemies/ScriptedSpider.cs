﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ScriptedSpider : Enemy
{
    private bool isMoving = false;
    private Vector3 lastPosition;
    [SerializeField] Transform pos1;
    [SerializeField] Transform pos2;
    [SerializeField] ScriptLamp lamp;
    [SerializeField] float shakeIntensity;

    private bool objective1 = false;
    private bool objective2 = false;
    float save;

    private void Start()
    {
        Initialize();
        lastPosition = transform.position;
    }

    private void Update()
    {
        velocity = (transform.position - lastPosition).magnitude * 10;
        lastPosition = transform.position;

        animator.SetFloat("Velocity", velocity);
        //print(velocity);

        if (isChasing)
        {
            ChaseBehavior();
        }

        animator.SetBool("IsMoving", isMoving);

        if (agent.isOnOffMeshLink)
        {
            agent.CompleteOffMeshLink();
            agent.isStopped = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DetectZone"))
        {
            save = GameManager.instance.GetShakeIntensity();
            GameManager.instance.SetShakeIntensity(shakeIntensity);
            GameManager.instance.ShakeScreen(0.2f);
            lamp.Swing();
        }
    }

    public void Script()
    {
        isMoving = true;
        StartCoroutine("ScriptRoutine");
    }

    private IEnumerator ScriptRoutine()
    {
        int count = 0;
        int step = 1;
        while (count <3)
        {
            if (step == 1) MoveTo(pos1.position);
            else if(step==2) MoveTo(pos2.position);
            else
            {
                count++;
                step = 1;
                if(count <3)MoveTo(pos1.position);
            }
            yield return new WaitForSeconds(0.5f);
            while (agent.remainingDistance >0.5f)
            {
                print("remaining distance:"+agent.remainingDistance);
                print("Step" + step);
                yield return new WaitForSeconds(0.1f);
            }
            step++;
            yield return new WaitForEndOfFrame();
        }
        GameManager.instance.SetShakeIntensity(save);
        Destroy(gameObject);        
        yield return null;
    }
}
