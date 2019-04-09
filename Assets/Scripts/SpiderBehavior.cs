using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpiderBehavior : Enemy
{
    [SerializeField] private float bonusSpeed = 1;

    [SerializeField] private float delaySpot = 1;
    [SerializeField] private float delayChase = 3;

    [SerializeField] private bool clickToSetDestination = false;

    [SerializeField] private float countLook = 0;
    private float countChase = 0;

    private bool canSeePlayer = false;

    private bool chaseCoroutine = false;
    private Animator animator;
    private bool isMoving = false;
    private bool isLooked = false;

    [SerializeField] private float velocity;
    private Vector3 lastPosition;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        animator.SetFloat("WakeMult", (1f / delaySpot));
        Initialize();
        countChase = 0;
        countLook = 0;
        lastPosition = transform.position;
    }

    private void Update()
    {
        velocity = (transform.position - lastPosition).magnitude;
        lastPosition = transform.position;

        isMoving = false;
        animator.SetFloat("mult", velocity);
        // SI L'ARAIGNEE CHASSE : SON COMPORTEMENT D'ALLER VERS LE JOUEUR ( PATHFINDING )
        if(isChasing)
        {
            isMoving = true;
            MoveTo(GameManager.instance.player.transform.position);
            if (!canSeePlayer && !GameManager.instance.player.lightOn)
            {
                if(!chaseCoroutine) StartCoroutine("CountChase");
                chaseCoroutine = true;
            }
        }
        else
        {
            LightDetection();
        }
        if (Input.GetMouseButtonDown(0) && clickToSetDestination)
        {
            Vector3 pos = GameManager.instance.player.GetLookAt();
            MoveTo(pos);
            print(pos);
        }
        animator.SetBool("IsMoving", isMoving);
    }

    // COROUTINE POUR COMPTER LE TEMPS QUE L'ARAIGNEE EST REGARDEE PAR LE JOUEUR
    private IEnumerator CountLook()
    {
        isLooked = true;
        animator.SetTrigger("WakesUp");
        while (countLook<delaySpot)
        {
            yield return new WaitForSeconds(0.1f);
            countLook+=0.1f;
            //print(countLook);
        }
        Chase();
        isLooked = false;
        yield return null;
    }

    // COROUTINE POUR COMPTER LE TEMPS QUE L'ARAIGNEE PASSE A CHASSER LE JOUEUR. POTENTIELLEMENT INUTILE ?
    private IEnumerator CountChase()
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
        //agent.isStopped = true;
        isChasing = false;
        countChase = 0;
        countLook = 0;
    }

    // ARRETER LA CHASSE DU JOUEUR
    private void StopChase()
    {
        Respawn();
    }

    // COMMENCER LA CHASSE DU JOUEUR
    private void Chase()
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
    public override void IsLit(bool b)
    {
        agent.speed = moveSpeed;
        if (b)
        {
            GameManager.instance.ShakeScreen(0.1f);
            agent.speed += bonusSpeed;
            if (!isLooked) StartCoroutine("CountLook");
        }
        else
        {
            StopCoroutine("CountLook");
            isLooked = false;
            //countLook = 0; // A UTILISER SI ON VEUT QUE LE TEMPS DE SPOT SE RESET
        }
    }
}
