using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpiderBehavior : Enemy
{
    [SerializeField] float moveSpeed = 1;
    [SerializeField] float bonusSpeed = 1;

    [SerializeField] float delaySpot=1;
    [SerializeField] float delayChase = 3;
    float countLook = 0;
    float countChase = 0;

    [SerializeField] bool isSleeping = true;
    [SerializeField]  bool isChasing = false;

    bool isLooked = false;
    bool canSeePlayer = false;

    bool chaseCoroutine = false;

    Vector3 SpawnZone;
    NavMeshAgent agent;

    [SerializeField] bool MeshVisibleSystem=false;
    MeshRenderer meshRenderer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        meshRenderer = GetComponent<MeshRenderer>();
        SpawnZone = transform.position;
        agent.speed = moveSpeed;

        countChase = 0;
        countLook = 0;
        isVisible(false);
    }

    void Update()
    {
        if(isChasing)
        {
            agent.SetDestination(GameManager.instance.player.transform.position);
            //if(detectZone.)
            if (!canSeePlayer && !GameManager.instance.player.lightOn)
            {
                if(!chaseCoroutine) StartCoroutine("CountChase");
                chaseCoroutine = true;
            }
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
            print(countLook);
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

    void StopChase()
    {
        transform.position = SpawnZone;
        agent.isStopped=true;
        isChasing = false;
        isSleeping = true;
        countChase = 0;
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
            isVisible(true);
            agent.speed += bonusSpeed;
            Looked();
        }
        else
        {
            StopCoroutine("CountLook");
            //countLook = 0; // A UTILISER SI ON VEUT QUE LE TEMPS DE SPOT SE RESET
            isVisible(false);
        }
    }

    public override void isVisible(bool b)
    {
        if(MeshVisibleSystem) meshRenderer.enabled = b;
    }
}
