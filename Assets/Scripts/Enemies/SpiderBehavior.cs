using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpiderBehavior : Enemy
{
    [SerializeField] private float bonusSpeed = 1;

    [SerializeField] private float delaySpot = 1;
    [SerializeField] private float delayChase = 3;
    [SerializeField] private float lengthDetection = 10;

    [SerializeField] private bool clickToSetDestination = false;

    [SerializeField] private float countLook = 0;
    private float countChase = 0;

    private bool canSeePlayer = false;

    private bool chaseCoroutine = false;
    private bool isMoving = false;
    private bool isLooked = false;

    private Vector3 lastPosition;

    NavMeshLink link;

    private void Start()
    {
        Initialize();
        animator.SetFloat("WakeMult", (1f / delaySpot));
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
            ChaseBehavior();
        }
        else
        {
            LightDetection();
        }

        DebugPath(); 
        animator.SetBool("IsMoving", isMoving);

        if (agent.isOnOffMeshLink) print("Using link right now");
    }

    private void DebugPath()
    {
        if (Input.GetMouseButtonDown(0) && clickToSetDestination)
        {
            Vector3 pos = GameManager.instance.player.GetLookAt();
            MoveTo(pos);
            print(pos);
        }
    }

    public override void ChaseBehavior()
    {
        // goes to the player
        isMoving = true;
        MoveTo(GameManager.instance.player.transform.position);

        if (!GameManager.instance.player.lightOn && !GameManager.instance.player.GetIsMoving())
        {
            if(!CheckSeePlayer())
            {
                if (!chaseCoroutine) StartCoroutine("CountChase");
                chaseCoroutine = true;
            }
        }
        else
        {
            StopCoroutine("CountChase");
        }
    }

    private bool CheckSeePlayer()
    {
        bool test = false;
        Vector3 playerPosition = GameManager.instance.player.transform.position;
        Vector3 playerToSpiderVec = transform.position - GameManager.instance.player.transform.position;
        float playerToSpiderLength = playerToSpiderVec.magnitude;

        if (playerToSpiderLength <= lengthDetection)
        {
            RaycastHit hit;
            Ray ray = new Ray(transform.position, playerPosition);
            Physics.Raycast(ray, out hit, Mathf.Infinity, GameManager.instance.GetWallsAndMobsLayer());

            if (hit.transform.gameObject.tag == "Player")
            {
                test = true;
            }
        }
        return test;
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
        StartChase();
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
        countChase = 0;
        countLook = 0;
    }

    // ARRETER LA CHASSE DU JOUEUR
    private void StopChase()
    {
        Respawn();
    }

    public void CheckCanSeePlayer()
    {

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

    public override void StartChase()
    {
        base.StartChase();
        countLook = 0;
    }

    // APPELER LORSQUE L'ARAIGNEE EST ECLAIREE
    public override void IsLit(bool b)
    {
        agent.speed = moveSpeed;
        if (b)
        {
            AkSoundEngine.PostEvent(WwiseLook.Id, gameObject);
            GameManager.instance.ShakeScreen(0.1f);
            agent.speed = moveSpeed+bonusSpeed;
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
