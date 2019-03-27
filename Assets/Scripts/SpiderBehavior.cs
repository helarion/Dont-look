using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpiderBehavior : Enemy
{
    [SerializeField] float bonusSpeed = 1;

    [SerializeField] float delaySpot=1;
    [SerializeField] float delayChase = 3;

    float countLook = 0;
    float countChase = 0;

    bool isLooked = false;
    bool canSeePlayer = false;

    bool chaseCoroutine = false;

    void Start()
    {
        Initialize();
        countChase = 0;
        countLook = 0;
    }

    void Update()
    {
        if(isChasing)
        {
            agent.SetDestination(GameManager.instance.player.transform.position);
            if (!canSeePlayer && !GameManager.instance.player.lightOn)
            {
                if(!chaseCoroutine) StartCoroutine("CountChase");
                chaseCoroutine = true;
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            agent.SetDestination(GameManager.instance.player.GetLookAt());
            print("Agent sent to " + GameManager.instance.player.GetLookAt());
        }
    }

    public void Looked()
    {
        if (isLooked) return;
        isLooked = true;
    }

    public void StopLook()
    {
        if (!isLooked) return;
        isLooked = false;

    }

    IEnumerator CountLook()
    {
        while (countLook<delaySpot)
        {
            yield return new WaitForSeconds(0.1f);
            countLook+=0.1f;
            //print(countLook);
        }
        Chase();
        yield return null;
    }

    IEnumerator CountChase()
    {
        while(countChase<delayChase)
        {
            yield return new WaitForSeconds(0.5f);
            countChase+=0.5f;
            //print(countChase);
        }
        StopChase();
        chaseCoroutine = false;
        yield return null;
    }

    public override void Respawn()
    {
        base.Respawn();
        agent.isStopped = true;
        isChasing = false;
        countChase = 0;
        countLook = 0;
    }

    void StopChase()
    {
        Respawn();
    }

    void Chase()
    {
        agent.isStopped = false;
        isChasing = true;
        countLook = 0;
    }

    public override void DetectPlayer(bool b)
    {
        canSeePlayer = b;
        if (!b)
        {
            StopCoroutine("CountChase");
            chaseCoroutine = false;
        }
    }

    public override void isLit(bool b)
    {
        agent.speed = moveSpeed;
        if (b)
        {
            StartCoroutine("CountLook");
            agent.speed += bonusSpeed;
            Looked();
        }
        else
        {
            StopCoroutine("CountLook");
            //countLook = 0; // A UTILISER SI ON VEUT QUE LE TEMPS DE SPOT SE RESET
        }
    }
}
