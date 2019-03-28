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
        // SI L'ARAIGNEE CHASSE : SON COMPORTEMENT D'ALLER VERS LE JOUEUR ( PATHFINDING )
        if(isChasing)
        {
            agent.destination=GameManager.instance.player.transform.position;
            agent.isStopped = false;
            if (!canSeePlayer && !GameManager.instance.player.lightOn)
            {
                if(!chaseCoroutine) StartCoroutine("CountChase");
                chaseCoroutine = true;
            }
        }
        /*if (Input.GetMouseButtonDown(0))
        {
            //agent.SetDestination(GameManager.instance.player.GetLookAt());
            agent.destination = GameManager.instance.player.GetLookAt();
            agent.isStopped = false;
            print("Agent sent to " + GameManager.instance.player.GetLookAt());
        }*/
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

    // COROUTINE POUR COMPTER LE TEMPS QUE L'ARAIGNEE EST REGARDEE PAR LE JOUEUR
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

    // COROUTINE POUR COMPTER LE TEMPS QUE L'ARAIGNEE PASSE A CHASSER LE JOUEUR. POTENTIELLEMENT INUTILE ?
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

    // FAIT REAPARAITRE L'ARAIGNEE ALEATOIREMENT DANS UNE DE SES ZONES DE SPAWN
    public override void Respawn()
    {
        base.Respawn();
        agent.isStopped = true;
        isChasing = false;
        countChase = 0;
        countLook = 0;
    }

    // ARRETER LA CHASSE DU JOUEUR
    void StopChase()
    {
        Respawn();
    }

    // COMMENCER LA CHASSE DU JOUEUR
    void Chase()
    {
        agent.isStopped = false;
        isChasing = true;
        countLook = 0;
    }

    // APPELER POUR DIRE SI L'ARAIGNEE PEUT ENCORE DETECTER LE JOUEUR OU NON
    public override void DetectPlayer(bool b)
    {
        canSeePlayer = b;
        if (!b)
        {
            StopCoroutine("CountChase");
            chaseCoroutine = false;
        }
    }

    // APPELER LORSQUE L'ARAIGNEE EST ECLAIREE
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
