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

    private float countLook = 0;
    private float countChase = 0;

    private bool canSeePlayer = false;

    private bool chaseCoroutine = false;
    private Animator animator;
    private bool isMoving = false;

    [SerializeField] private float velocity;
    private Vector3 lastPosition;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
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
            Vector3 playerPosition = GameManager.instance.player.transform.position;
            Vector3 lightVec = GameManager.instance.player.GetLookAt() - playerPosition;
            Vector3 playerToSpiderVec = transform.position - GameManager.instance.player.transform.position;

            float playerToSpiderLength = playerToSpiderVec.magnitude;//- 2;
            Light playerLight = GameManager.instance.player.getLight();
            float lightRange = playerLight.range;
            float lightAngle = playerLight.spotAngle / 2.0f;// - 5;
            if (playerToSpiderLength > lightRange)
            {
                //print(gameObject.name + " is too far from player : " + playerToSpiderLength + " > " + lightRange + ".");
            }
            else
            {
                //print(gameObject.name + " is in player light range : " + playerToSpiderLength + ".");
                float angleFromLight = Mathf.Acos(Vector3.Dot(lightVec, playerToSpiderVec) / (lightVec.magnitude * playerToSpiderVec.magnitude)) * Mathf.Rad2Deg;
                if (angleFromLight > lightAngle)
                {
                    //print(gameObject.name + " is not in light : " + angleFromLight + " > " + lightAngle + ".");
                }
                else
                {
                    //print(gameObject.name + " is in player light : " + angleFromLight + ".");
                    lightVec = Vector3.RotateTowards(lightVec, playerToSpiderVec, angleFromLight, Mathf.Infinity);

                    RaycastHit hit;
                    Ray ray = new Ray(playerPosition, lightVec);
                    Physics.Raycast(ray, out hit, Mathf.Infinity, GameManager.instance.GetWallsAndMobsLayer());

                    if (hit.transform.gameObject.tag == "Spider")
                    {
                        IsLit(true);
                    }

                    //print("Touched " + hit.transform.gameObject.name);
                }
            }
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
            StartCoroutine("CountLook");
            agent.speed += bonusSpeed;
        }
        else
        {
            StopCoroutine("CountLook");
            //countLook = 0; // A UTILISER SI ON VEUT QUE LE TEMPS DE SPOT SE RESET
        }
    }
}
