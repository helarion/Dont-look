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

    void Start()
    {
        Initialize();
        agent.speed = moveSpeed;
        detectZone = GetComponent<BoxCollider>();
    }

    void Update()
    {
        VelocityCount();
        if (isChasing)
        {
            ChaseBehavior();
        }
    }

    public void PlayWalk()
    {
        GameManager.instance.ShakeScreen(walkShakeDuration, currentWalkIntensity);
    }

    public override void PlayChase()
    {
        AkSoundEngine.PostEvent(GameManager.instance.ChaseBipedeAmbPlay, p.modelTransform.gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        bool test = false;
        if (player != null)
        {
            test = true;
            if (!isChasing) StartChase();
        }
        canSeePlayer = test;
    }

    public override void IsPathInvalid()
    {
        print("ispathinvalid");
        if (canSeePlayer) return;
        print("cannotSeePlayer");
        base.IsPathInvalid();
        print("ispathinvalid");

    }

    public override void ChaseBehavior()
    {
        base.ChaseBehavior();
        IsPathInvalid();
        IsLit(GameManager.instance.LightDetection(gameObject, false));
        float distanceMax = (detectZone.bounds.size.x / 2);
        float rate = playerDistance.Remap(0, distanceMax, 0.2f, 1.2f);
        currentWalkIntensity = walkShakeIntensity * rate;

        if(isMoving)
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
        animator.SetBool("IsLooked", false);
        animator.SetBool("IsMoving", isMoving);
    }

    private void LitConcentrated()
    {
        agent.isStopped = true;
        isMoving = false;
        animator.SetBool("IsLooked", true);
        animator.SetBool("IsMoving", isMoving);
    }

    public override void Respawn()
    {
        base.Respawn();
        hasPlayedLook = false;
        isLooked = false;
    }
}
