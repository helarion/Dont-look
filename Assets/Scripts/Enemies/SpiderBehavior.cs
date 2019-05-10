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

    private bool isSearching = false;
    private bool canSeePlayer = false;
    private bool lowerSpeedChase = true;

    NavMeshLink link;

    private void Start()
    {
        Initialize();
        animator.SetFloat("WakeMult", (1f / delaySpot));
    }

    private void Update()
    {
        VelocityCount();
        IsLit(GameManager.instance.LightDetection(transform, needsConcentration));
        // SI L'ARAIGNEE CHASSE : SON COMPORTEMENT D'ALLER VERS LE JOUEUR ( PATHFINDING )
        if (isChasing)
        {
            ChaseBehavior();
        }

        DebugPath(); 

        if (agent.isOnOffMeshLink)
        {
            OffMeshLinkData linkData = agent.currentOffMeshLinkData;

            //print("Using link right now");
            agent.CompleteOffMeshLink();
            agent.isStopped=false;
        }
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

    public override void ChaseBehavior()
    {
        base.ChaseBehavior();
        // goes to the player
        isMoving = true;
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
