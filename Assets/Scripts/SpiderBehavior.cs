using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpiderBehavior : MonoBehaviour
{
    [SerializeField] float moveSpeed = 1;

    [SerializeField] float delaySpot=1;
    [SerializeField] float delayChase = 3;
    float countSpot = 0;
    float countChase = 0;

    [SerializeField] bool isSleeping = true;
    [SerializeField]  bool isChasing = false;

    bool isLooked = false;
    bool canSeePlayer = false;

    bool chaseCoroutine = false;

    Vector3 SpawnZone;
    NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        SpawnZone = transform.position;
        agent.speed = moveSpeed;

        countChase = 0;
        countSpot = 0;
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

    IEnumerator CountSpot()
    {
        while (countSpot<delaySpot)
        {
            yield return new WaitForSeconds(0.1f);
            if (!isLooked)
            {
                countSpot = 0;
                yield return null;
            }
            delaySpot+=0.1f;
        }
        Chase();
        yield return null;
    }

    IEnumerator CountChase()
    {
        while(countChase<delayChase)
        {
            yield return new WaitForSeconds(0.5f);
            if (canSeePlayer)
            {
                countChase = 0;
                //print("break");
                yield return null;
            }
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
        countSpot = 0;
    }

    public void DetectPlayer(bool b)
    {
        canSeePlayer = b;
        if (b) Chase(); // EN ATTENDANT LE DETECTION PAR LA LUMIERE
    }
}
