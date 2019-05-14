using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BipedeBehavior : Enemy
{
    [SerializeField] private float walkShakeIntensity=0.3f;
    [SerializeField] private float walkShakeDuration = 1;

    private float currentWalkIntensity;
    private BoxCollider detectZone;
    private bool canSeePlayer = false;
    // Start is called before the first frame update
    void Start()
    {
        Initialize();
        agent.speed = moveSpeed;
        detectZone = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        VelocityCount();
        if (isChasing)
        {
            ChaseBehavior();
        }
        //DebugPath(); 
    }

    public void PlayWalk()
    {
        GameManager.instance.ShakeScreen(walkShakeDuration, currentWalkIntensity);
    }

    private void OnTriggerStay(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        bool test = false;
        if (player == null) return;
        test = true;
        canSeePlayer = test;
        if(!isChasing) StartChase();
    }

    public override void IsPathInvalid()
    {
        if (canSeePlayer) return;
        base.IsPathInvalid();
    }

    public override void ChaseBehavior()
    {
        base.ChaseBehavior();
        IsLit(GameManager.instance.LightDetection(transform, needsConcentration));
        float distanceMax = (detectZone.bounds.size.x / 2);
        float rate = playerDistance.Remap(0, distanceMax, 0.2f, 1.2f);
        currentWalkIntensity = walkShakeIntensity * rate;
        if(!isLooked)
        {
            // goes to the player
            MoveTo(GameManager.instance.player.transform.position);
        }
    }

    // APPELER LORSQUE LE BIPEDE EST ECLAIREE
    public override void IsLit(bool b)
    {
        if (b)
        {
            isLooked = true;
            if (GameManager.instance.player.GetConcentration()) LitConcentrated();
            else LitNormal();

            animator.SetBool("IsMoving", isMoving);
        }
        else
        {
            //agent.speed = moveSpeed;
            isLooked = false;
            agent.isStopped = false;
            isMoving = true;
            animator.SetBool("IsLooked", false);
            animator.SetBool("IsMoving", isMoving);
        }
    }

    private void LitNormal()
    {
        agent.isStopped = false;
        isMoving = true;
        agent.speed = moveSpeed - bonusSpeed;
    }

    private void LitConcentrated()
    {
        agent.isStopped = true;
        isMoving = false;
        animator.SetBool("IsLooked", true);
        
    }

    public override void Respawn()
    {
        base.Respawn();
        hasPlayedLook = false;
        isLooked = false;
    }
}
