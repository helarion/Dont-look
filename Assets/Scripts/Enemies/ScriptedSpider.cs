using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ScriptedSpider : Enemy
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

    private PlayerController p;

    private void Start()
    {
        Initialize();
        p = GameManager.instance.player;
        animator.SetFloat("WakeMult", (1f / delaySpot));
        countChase = 0;
        countLook = 0;
        lastPosition = transform.position;
    }

    private void Update()
    {
        velocity = (transform.position - lastPosition).magnitude * 10;
        lastPosition = transform.position;

        isMoving = false;
        animator.SetFloat("Velocity", velocity);
        //print(velocity);

        // SI L'ARAIGNEE CHASSE : SON COMPORTEMENT D'ALLER VERS LE JOUEUR ( PATHFINDING )
        if (isChasing)
        {
            ChaseBehavior();
        }
        else
        {
            LightDetection();
        }

        DebugPath();
        animator.SetBool("IsMoving", isMoving);

        if (agent.isOnOffMeshLink)
        {
            OffMeshLinkData linkData = agent.currentOffMeshLinkData;

            //print("Using link right now");
            agent.CompleteOffMeshLink();
            agent.isStopped = false;
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
        // goes to the player
        
        MoveTo(GameManager.instance.player.transform.position);

        if (!p.lightOn && !p.GetIsMoving() && p.GetIsHidden())
        {
            if (!canSeePlayer)
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

    public void StartScript()
    {
        isMoving = true;
        MoveTo(GameManager.instance.player.transform.position);
    }

}
