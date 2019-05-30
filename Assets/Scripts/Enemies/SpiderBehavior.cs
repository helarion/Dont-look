using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpiderBehavior : Enemy
{
    [SerializeField] private float delaySpot = 1;
    [SerializeField] private bool clickToSetDestination = false;
    [SerializeField] private float malusSpeedStart = 2;
    [SerializeField] private float malusStartDuration = 2;
    [SerializeField] private bool isChangingPlaces = false;
    [SerializeField] private float changingTime = 15;
    [SerializeField] private float canSeePlayerDistance = 0.5f;
    [SerializeField] private float stopChaseDistance = 15;
    [SerializeField] private string reveilSound;
    [SerializeField] private string screamSound;
    [SerializeField] private string stopWalkFadeOutSound;

    private bool isSearching = false;
    private bool canSeePlayer = false;
    private bool lowerSpeedChase = true;
    private bool isTransitionning = false;
    private bool isCountingChange = false;

    NavMeshLink link;

    SpatialRoom currentSpatialRoom = null;

    Transform[] savedSpawnZone;

    private void Start()
    {
        savedSpawnZone = spawnZones;
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

        //DebugPath(); 
    }

    private void OnTriggerEnter(Collider other)
    {
        SpatialRoom spatialRoom = other.GetComponent<SpatialRoom>();
        if (spatialRoom != null)
        {
            currentSpatialRoom = spatialRoom;
        }
    }

    IEnumerator CountingChange()
    {
        yield return new WaitForSeconds(changingTime);
        //if (isChangingPlaces) StopChase();
        //else 
        Respawn(isChangingPlaces);
    }

    public override void PlayChase()
    {
        AkSoundEngine.PostEvent(GameManager.instance.ChaseSpiderAmbPlay, player.modelTransform.gameObject);
    }

    public override void StopChaseSounds()
    {
        base.StopChaseSounds();
        if(player.getIsAlive())
        {
            AkSoundEngine.PostEvent(stopWalkFadeOutSound, gameObject);
        }
    }

    private void DebugPath()
    {
        if (Input.GetMouseButtonDown(0) && clickToSetDestination)
        {
            Vector3 pos = player.GetLookAt();
            MoveTo(pos);
            print(pos);
        }
    }

    public void FinishedTransition()
    {
        //print("FinishedTransition");
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

        canSeePlayer = (transform.position - GameManager.instance.player.transform.position).magnitude < canSeePlayerDistance && !isSearching;

        if (!isSearching)
        {
            MoveTo(GameManager.instance.player.transform.position);

            if (!player.lightOn && !player.GetIsMoving() && player.GetIsHidden() && !canSeePlayer)
            {
                if (transform.position.x - GameManager.instance.player.transform.position.x > 0)
                {
                    //print("je vais vers la gauche");
                    MoveTo(currentSpatialRoom.spiderGoalLeft.position);
                }
                else
                {
                    //print("je vais vers la droite");
                    MoveTo(currentSpatialRoom.spiderGoalRight.position);
                }
                StartCoroutine(CountChase());
                isSearching = true;
            }
        }
        else
        {
            if (player.lightOn || player.GetIsMoving() || !player.GetIsHidden() || canSeePlayer)
            {
                StopCoroutine(CountChase());
                isSearching = false;
                agent.speed = moveSpeed;
            }
        }
    }

    public override void StartChase()
    {
        base.StartChase();
        StopCoroutine("CountingChange");
        isCountingChange = true;
        AkSoundEngine.PostEvent(screamSound, gameObject);
        StartCoroutine(LowerSpeedCoroutine());
    }

    public override void StopChase()
    {
        if (isChasing && isChangingPlaces)
        {
            RemoveAt(ref spawnZones, spawnIndex);
        }
        base.StopChase();
    }

    public static void RemoveAt<T>(ref T[] arr, int index)
    {
        // replace the element at index with the last element
        arr[index] = arr[arr.Length - 1];
        // finally, let's decrement Array's size by one
        Array.Resize(ref arr, arr.Length - 1);
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
        while ((transform.position - GameManager.instance.player.transform.position).magnitude < stopChaseDistance || (transform.position.x > agent.destination.x == transform.position.x > GameManager.instance.player.transform.position.x))
        {
            agent.speed = moveSpeed;
            countChase += Time.deltaTime;
            yield return new WaitForEndOfFrame();
            //print(countChase);
        }
        StopChase();
        isSearching = false;
        yield return null;
    }

    // FAIT REAPARAITRE L'ARAIGNEE ALEATOIREMENT DANS UNE DE SES ZONES DE SPAWN
    public override void Respawn(bool farOfPlayer)
    {
        if (spawnZones.Length == 0) Destroy(gameObject);
        if (isChangingPlaces && !player.getIsAlive()) spawnZones = savedSpawnZone;
        else if(isChangingPlaces && player.getIsAlive())
        {
            isCountingChange = false;
            if ((transform.position - player.transform.position).magnitude < spawnDistanceFromPlayer) return;
        }
        base.Respawn(farOfPlayer);
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
                AkSoundEngine.PostEvent(reveilSound, gameObject);
            }
            else
            {
                agent.speed = speed + malusSpeed;
            }
        }
        else
        {
            if (isChasing) agent.speed = speed;
        }
    }
}
