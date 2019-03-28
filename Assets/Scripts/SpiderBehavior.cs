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
            agent.destination=GameManager.instance.player.transform.position;
            agent.isStopped = false;
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

            float playerToSpiderLength =  playerToSpiderVec.magnitude - 2;
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
                    Physics.Raycast(ray, out hit, Mathf.Infinity, GameManager.instance.getWallsAndMobsLayer());

                    if (hit.transform.gameObject.name == "Model")
                    {
                        GameManager.instance.ShakeScreen(0.1f);
                    }

                    //print("Touched " + hit.transform.gameObject.name);
                }
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
