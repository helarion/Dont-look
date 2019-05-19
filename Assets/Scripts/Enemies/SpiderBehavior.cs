using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpiderBehavior : Enemy
{
    [SerializeField] private float delaySpot = 1;
    [SerializeField] private float lengthDetection = 10;
    [SerializeField] private bool clickToSetDestination = false;
    [SerializeField] private float malusSpeedStart = 2;
    [SerializeField] private float malusStartDuration = 2;
    [SerializeField] private bool isChangingPlaces = false;
    [SerializeField] private float changingTime = 15;

    private bool isSearching = false;
    private bool canSeePlayer = false;
    private bool lowerSpeedChase = true;
    private bool isTransitionning = false;
    private bool isCountingChange = false;

    NavMeshLink link;

    private void Start()
    {
        Initialize();
        agent.autoTraverseOffMeshLink = false;
    }

    private void Update()
    {
        if (agent.isOnOffMeshLink)
        {
            //print("On Link");
            if (isTransitionning) return;
            //print("StartTransition");
            isTransitionning = true;
            agent.isStopped = true;
            isMoving = false;
            animator.SetBool("IsMoving", isMoving);
            animator.SetTrigger("Transition");
        }

        VelocityCount();
        IsLit(GameManager.instance.LightDetection(gameObject, false));
        // SI L'ARAIGNEE CHASSE : SON COMPORTEMENT D'ALLER VERS LE JOUEUR ( PATHFINDING )
        if (isChasing)
        {
            ChaseBehavior();
        }
        else if(isChangingPlaces &&!isCountingChange)
        {
            isCountingChange = true;
            StartCoroutine("CountingChange");
        }

        DebugPath(); 
    }

    IEnumerator CountingChange()
    {
        yield return new WaitForSeconds(changingTime);
        Respawn();
    }

    private void DebugPath()
    {
        if (Input.GetMouseButtonDown(0) && clickToSetDestination)
        {
            Vector3 pos = p.GetLookAt();
            MoveTo(pos);
            print(pos);
        }
    }

    public void FinishedTransition()
    {
        print("FinishedTransition");
        if (!isTransitionning) return;
        agent.CompleteOffMeshLink();
        agent.isStopped = false;
        isMoving = true;
        animator.SetBool("IsMoving", isMoving);
        isTransitionning = false;
    }

    public override void ChaseBehavior()
    {
        if (isTransitionning) return;
        base.ChaseBehavior();
        // goes to the player
        MoveTo(GameManager.instance.player.transform.position);

        if (!p.lightOn && !p.GetIsMoving() && p.GetIsHidden())
        {
            if(!canSeePlayer)
            {
                if (!isSearching) StartCoroutine(CountChase());
                isSearching = true;
            }
        }
        else
        {
            StopCoroutine(CountChase());
            isSearching = false;
            agent.speed = moveSpeed;
        }
    }

    public override void StartChase()
    {
        base.StartChase();
        StopCoroutine("CountingChange");
        isCountingChange = true;
        StartCoroutine(LowerSpeedCoroutine());
    }

    private IEnumerator LowerSpeedCoroutine()
    {
        yield return new WaitForSeconds(malusStartDuration);
        lowerSpeedChase = false;
        yield return null;
    }

    // COROUTINE POUR COMPTER LE TEMPS QUE L'ARAIGNEE PASSE A CHASSER LE JOUEUR. POTENTIELLEMENT INUTILE ?
    private IEnumerator CountChase()
    {
        float countChase = 0;
        while (countChase < delayChase)
        {
            agent.speed = moveSpeed - bonusSpeed;
            countChase += Time.deltaTime;
            yield return new WaitForEndOfFrame();
            //print(countChase);
        }
        StopChase();
        isSearching = false;
        yield return null;
    }

    // FAIT REAPARAITRE L'ARAIGNEE ALEATOIREMENT DANS UNE DE SES ZONES DE SPAWN
    public override void Respawn()
    {
        base.Respawn();
        StopCoroutine("CountingChange");
        isCountingChange = false;
        lowerSpeedChase = true;
        animator.SetBool("IsSleeping", true);
    }

    // APPELER POUR DIRE SI L'ARAIGNEE PEUT ENCORE DETECTER LE JOUEUR OU NON
    public override void DetectPlayer(bool b)
    {
        canSeePlayer = b;
        if (!b)
        {
            StopCoroutine(CountChase());
            isSearching = false;
            animator.SetBool("WakesUp", false);
            isSearching= false;
        }
    }

    // APPELER LORSQUE L'ARAIGNEE EST ECLAIREE
    public override void IsLit(bool b)
    {
        base.IsLit(b);
        float speed = moveSpeed;
        if (lowerSpeedChase) speed -= malusSpeedStart;
        if (b)
        {
            if (!isChasing && !isLooked)
            {
                StopCoroutine("CountingChange");
                isCountingChange = true;
                isLooked = true;
                animator.SetBool("IsSleeping", false);
                animator.SetTrigger("WakesUp");
            }
            else
            {
                agent.speed = speed + bonusSpeed;
            }
        }
        else
        {
            if (isChasing) agent.speed = speed;
        }
    }
}
